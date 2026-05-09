using Microsoft.AspNetCore.Mvc;
using BankApp.Models.DTOs.Auth;
using BankApp.Models.Entities;
using BankApp.Server.Repositories.Interfaces;
using BankApp.Server.Services.Infrastructure.Interfaces;
using BankApp.Server.Utilities;
using Google.Apis.Auth;

namespace BankApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository authRepository;
        private readonly IHashService hashService;
        private readonly IJWTService jwtService;
        private readonly IOTPService otpService;
        private readonly IEmailService emailService;

        private const int MaxFailedAttempts = 5;
        private const int LockoutMinutes = 30;

        public AuthController(
            IAuthRepository authRepository,
            IHashService hashService,
            IJWTService jwtService,
            IOTPService otpService,
            IEmailService emailService)
        {
            this.authRepository = authRepository;
            this.hashService = hashService;
            this.jwtService = jwtService;
            this.otpService = otpService;
            this.emailService = emailService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            LoginResponse response = LoginInternal(request);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            RegisterResponse response = RegisterInternal(request);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("verify-otp")]
        public IActionResult VerifyOTP([FromBody] VerifyOTPRequest request)
        {
            LoginResponse response = VerifyOtpInternal(request);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new { error = "Email is required." });
            }
            RequestPasswordResetInternal(request.Email);

            // Always return an OK response with a generic message ( prevent malicious operations )
            return Ok(new { message = "If an account with that email exists, a password reset link has been sent." });
        }

        [HttpPost("reset-password")]
        public IActionResult ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest(new { error = "Token and new password are required." });
            }

            if (!BankApp.Server.Utilities.ValidationUtil.IsStrongPassword(request.NewPassword))
            {
                return BadRequest(new { error = "Password must be at least 8 characters with uppercase, lowercase, a digit, and a special character." });
            }

            bool isSuccess = ResetPasswordInternal(request.Token, request.NewPassword);
            if (!isSuccess)
            {
                return BadRequest(new { error = "Invalid, expired, or already used reset token." });
            }

            return Ok(new { message = "Password reset successfully. You may now log in with your new password." });
        }

        [HttpPost("logout")]
        public IActionResult Logout([FromHeader(Name = "Authorization")] string authorization)
        {
            // Bogdan: this implementation is not enough, still need to invalidate JWT, but this is not on original diagram
            // can expand in the future.
            if (string.IsNullOrWhiteSpace(authorization) || !authorization.StartsWith("Bearer "))
            {
                return BadRequest(new { error = "No token provided." });
            }

            string token = authorization.Substring("Bearer ".Length);

            if (!LogoutInternal(token))
            {
                return BadRequest(new { error = "Invalid session." });
            }

            return Ok(new { message = "Logged out successfully." });
        }

        [HttpPost("resend-otp")]
        public IActionResult ResendOTP([FromQuery] int userId, [FromQuery] string method = "email")
        {
            ResendOtpInternal(userId, method);
            return Ok(new { message = "If the user exists, a new code has been sent." });
        }

        [HttpPost("oauth-login")]
        public async Task<IActionResult> OAuthLogin([FromBody] OAuthLoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Provider) || string.IsNullOrWhiteSpace(request.ProviderToken))
            {
                return BadRequest(new { error = "Provider and ProviderToken are required." });
            }

            LoginResponse response = await OAuthLoginInternalAsync(request);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        public class VerifyTokenDto
        {
            public string Token { get; set; } = string.Empty;
        }

        [HttpPost("verify-reset-token")]
        public IActionResult VerifyResetToken([FromBody] VerifyTokenDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Token))
            {
                return BadRequest(new { error = "Token is required." });
            }

            bool isValid = VerifyResetTokenInternal(request.Token);
            if (!isValid)
            {
                return BadRequest(new { error = "Invalid or expired token." });
            }

            return Ok(new { message = "Token is valid." });
        }

        private LoginResponse LoginInternal(LoginRequest request)
        {
            if (!ValidationUtil.IsValidEmail(request.Email))
            {
                return new LoginResponse { Success = false, Error = "Invalid mail format." };
            }

            User? user = authRepository.FindUserByEmail(request.Email);
            if (user == null)
            {
                return new LoginResponse { Success = false, Error = "Invalid email or password." };
            }

            LoginResponse? lockCheck = CheckAccountLock(user);
            if (lockCheck != null)
            {
                return lockCheck;
            }

            if (!hashService.Verify(request.Password, user.PasswordHash))
            {
                return HandleFailedPassword(user);
            }

            if (user.Is2FAEnabled)
            {
                return Handle2FA(user);
            }

            return CompleteLogin(user);
        }

        private RegisterResponse RegisterInternal(RegisterRequest request)
        {
            string? validationError = ValidateRegistration(request);
            if (validationError != null)
            {
                return new RegisterResponse { Success = false, Error = validationError };
            }

            User? existingUser = authRepository.FindUserByEmail(request.Email);
            if (existingUser != null)
            {
                return new RegisterResponse { Success = false, Error = "Email is already registered." };
            }

            User user = new User
            {
                Email = request.Email,
                PasswordHash = hashService.GetHash(request.Password),
                FullName = request.FullName,
                PreferredLanguage = "en",
                Is2FAEnabled = false,
                IsLocked = false,
                FailedLoginAttempts = 0
            };

            bool created = authRepository.CreateUser(user);
            return created
                ? new RegisterResponse { Success = true }
                : new RegisterResponse { Success = false, Error = "Failed to create account." };
        }

        private async Task<LoginResponse> OAuthLoginInternalAsync(OAuthLoginRequest request)
        {
            if (!request.Provider.Equals("Google", StringComparison.OrdinalIgnoreCase))
            {
                return new LoginResponse { Success = false, Error = "Unsupported OAuth Provider." };
            }

            GoogleJsonWebSignature.Payload payload;
            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(request.ProviderToken);
            }
            catch (InvalidJwtException)
            {
                return new LoginResponse { Success = false, Error = "Invalid Google authentication token." };
            }

            string providerUserId = payload.Subject;
            string email = payload.Email;
            string fullName = payload.Name;

            OAuthLink? link = authRepository.FindOAuthLink(request.Provider, providerUserId);
            User? user = link != null ? authRepository.FindUserById(link.UserId) : null;

            if (user == null)
            {
                user = authRepository.FindUserByEmail(email);
                if (user == null)
                {
                    string randomPassword = Guid.NewGuid().ToString() + "A1a!";
                    user = new User
                    {
                        Email = email,
                        PasswordHash = hashService.GetHash(randomPassword),
                        FullName = fullName,
                        PreferredLanguage = "en",
                        Is2FAEnabled = false,
                        IsLocked = false,
                        FailedLoginAttempts = 0
                    };

                    if (!authRepository.CreateUser(user))
                    {
                        return new LoginResponse { Success = false, Error = "Failed to create user account." };
                    }

                    user = authRepository.FindUserByEmail(email);
                }

                OAuthLink newLink = new OAuthLink
                {
                    User = user!,
                    Provider = request.Provider,
                    ProviderUserId = providerUserId,
                    ProviderEmail = email
                };
                authRepository.CreateOAuthLink(newLink);
            }

            LoginResponse? lockCheck = CheckAccountLock(user);
            if (lockCheck != null)
            {
                return lockCheck;
            }

            if (user.Is2FAEnabled)
            {
                return Handle2FA(user);
            }

            return CompleteLogin(user);
        }

        private LoginResponse VerifyOtpInternal(VerifyOTPRequest request)
        {
            User? user = authRepository.FindUserById(request.UserId);
            if (user == null)
            {
                return new LoginResponse { Success = false, Error = "User not found." };
            }

            bool isValid = otpService.VerifyTOTP(request.UserId, request.OTPCode);
            if (!isValid)
            {
                return new LoginResponse { Success = false, Error = "Invalid or expired OTP code." };
            }

            otpService.InvalidateOTP(user.Id);
            return CompleteLogin(user);
        }

        private void ResendOtpInternal(int userId, string method)
        {
            User? user = authRepository.FindUserById(userId);
            if (user == null)
            {
                return;
            }

            string otp = otpService.GenerateTOTP(user.Id);
            if (method == "email" || string.Equals(user.Preferred2FAMethod, "email", StringComparison.OrdinalIgnoreCase))
            {
                emailService.SendOTPCode(user.Email, otp);
            }
        }

        private void RequestPasswordResetInternal(string email)
        {
            User? user = authRepository.FindUserByEmail(email);
            if (user == null)
            {
                return;
            }

            string rawToken = System.Security.Cryptography.RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            PasswordResetToken resetToken = new PasswordResetToken
            {
                Id = user.Id,
                TokenHash = rawToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                CreatedAt = DateTime.UtcNow
            };

            authRepository.SavePasswordResetToken(resetToken);
            emailService.SendPasswordResetLink(user.Email, rawToken);
        }

        private bool ResetPasswordInternal(string token, string newPassword)
        {
            PasswordResetToken? resetToken = authRepository.FindPasswordResetToken(token);
            if (resetToken == null || resetToken.UsedAt != null || resetToken.ExpiresAt < DateTime.UtcNow)
            {
                return false;
            }

            string finalPasswordHash = hashService.GetHash(newPassword);
            bool updated = authRepository.UpdatePassword(resetToken.Id, finalPasswordHash);
            if (!updated)
            {
                return false;
            }

            resetToken.UsedAt = DateTime.UtcNow;
            authRepository.SavePasswordResetToken(resetToken);
            authRepository.InvalidateAllSessions(resetToken.Id);
            return true;
        }

        private bool VerifyResetTokenInternal(string token)
        {
            PasswordResetToken? resetToken = authRepository.FindPasswordResetToken(token);
            return !(resetToken == null || resetToken.UsedAt != null || resetToken.ExpiresAt < DateTime.UtcNow);
        }

        private bool LogoutInternal(string token)
        {
            Session? session = authRepository.FindSessionByToken(token);
            if (session == null)
            {
                return false;
            }

            authRepository.UpdateSessionToken(session.Id);
            return true;
        }

        private LoginResponse? CheckAccountLock(User user)
        {
            if (!user.IsLocked)
            {
                return null;
            }

            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
            {
                return new LoginResponse { Success = false, Error = "Account is locked. Try again later." };
            }

            authRepository.ResetFailedAttempts(user.Id);
            return null;
        }

        private LoginResponse HandleFailedPassword(User user)
        {
            authRepository.IncrementFailedAttempts(user.Id);

            if (user.FailedLoginAttempts + 1 >= MaxFailedAttempts)
            {
                authRepository.LockAccount(user.Id, DateTime.UtcNow.AddMinutes(LockoutMinutes));
                emailService.SendLockNotification(user.Email);
                return new LoginResponse { Success = false, Error = "Account locked due to too many failed attempts." };
            }

            return new LoginResponse { Success = false, Error = "Invalid email or password." };
        }

        private LoginResponse Handle2FA(User user)
        {
            string otp = otpService.GenerateTOTP(user.Id);
            if (string.Equals(user.Preferred2FAMethod, "email", StringComparison.OrdinalIgnoreCase))
            {
                emailService.SendOTPCode(user.Email, otp);
            }

            return new LoginResponse
            {
                Success = true,
                Requires2FA = true,
                UserId = user.Id,
                Token = null
            };
        }

        private LoginResponse CompleteLogin(User user)
        {
            authRepository.ResetFailedAttempts(user.Id);
            string token = jwtService.GenerateToken(user.Id);
            authRepository.CreateSession(user.Id, token, null, null, null);
            emailService.SendLoginAlert(user.Email);
            return new LoginResponse
            {
                Success = true,
                Token = token,
                Requires2FA = false,
                UserId = user.Id
            };
        }

        private static string? ValidateRegistration(RegisterRequest request)
        {
            if (!ValidationUtil.IsValidEmail(request.Email))
            {
                return "Invalid email format.";
            }

            if (!ValidationUtil.IsStrongPassword(request.Password))
            {
                return "Password must be at least 8 characters with uppercase, lowercase, and a digit.";
            }

            if (string.IsNullOrWhiteSpace(request.FullName))
            {
                return "Full name is required.";
            }

            return null;
        }
    }
}