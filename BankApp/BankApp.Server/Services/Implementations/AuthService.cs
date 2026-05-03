using BankApp.Models.DTOs.Auth;
using BankApp.Models.Entities;
using BankApp.Server.Repositories.Interfaces;
using BankApp.Server.Services.Infrastructure.Implementations;
using BankApp.Server.Services.Infrastructure.Interfaces;
using BankApp.Server.Services.Interfaces;
using BankApp.Server.Utilities;
using Google.Apis.Auth;

namespace BankApp.Server.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository authRepository;
        private readonly IHashService hashService;
        private readonly IJWTService jwtService;
        private readonly IOTPService otpService;
        private readonly IEmailService emailService;

        private const int MaxFailedAttempts = 5;
        private const int LockoutMinutes = 30;

        public AuthService(IAuthRepository authRepository, IHashService hashService, IJWTService jwtService, IOTPService otpService, IEmailService emailService)
        {
            this.authRepository = authRepository;
            this.hashService = hashService;
            this.jwtService = jwtService;
            this.otpService = otpService;
            this.emailService = emailService;
        }

        public LoginResponse Login(LoginRequest request)
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

        public RegisterResponse Register(RegisterRequest request)
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

            User user = CreateUserFromRequest(request);
            bool created = authRepository.CreateUser(user);

            if (!created)
            {
                return new RegisterResponse { Success = false, Error = "Failed to create account." };
            }

            return new RegisterResponse { Success = true };
        }

        public async Task<LoginResponse> OAuthLoginAsync(OAuthLoginRequest request)
        {
            if (request.Provider.Equals("Google", StringComparison.OrdinalIgnoreCase))
            {
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
                User? user = null;

                if (link != null)
                {
                    user = authRepository.FindUserById(link.UserId);
                }

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
                        UserId = user!.Id,
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

            return new LoginResponse { Success = false, Error = "Unsupported OAuth Provider." };
        }

        public RegisterResponse OAuthRegister(OAuthRegisterRequest request)
        {
            if (!ValidationUtil.IsValidEmail(request.Email))
            {
                return new RegisterResponse { Success = false, Error = "Invalid email format." };
            }

            OAuthLink? existingLink = authRepository.FindOAuthLink(request.Provider, request.ProviderToken);
            if (existingLink != null)
            {
                return new RegisterResponse { Success = false, Error = "This OAuth account is already registered. Please login." };
            }

            User? existingUser = authRepository.FindUserByEmail(request.Email);
            int targetUserId;
            if (existingUser != null)
            {
                targetUserId = existingUser.Id;
            }
            else
            {
                string randomPassword = Guid.NewGuid().ToString() + "A1a!";
                User newUser = new User
                {
                    Email = request.Email,
                    PasswordHash = hashService.GetHash(randomPassword),
                    FullName = request.FullName,
                    PreferredLanguage = "en",
                    Is2FAEnabled = false,
                    IsLocked = false,
                    FailedLoginAttempts = 0
                };

                bool created = authRepository.CreateUser(newUser);
                if (!created)
                {
                    return new RegisterResponse { Success = false, Error = "Failed to create user account." };
                }

                User? savedUser = authRepository.FindUserByEmail(request.Email);
                if (savedUser == null)
                {
                    return new RegisterResponse { Success = false, Error = "Error retrieving created user." };
                }

                targetUserId = savedUser.Id;
            }

            OAuthLink newLink = new OAuthLink
            {
                UserId = targetUserId,
                Provider = request.Provider,
                ProviderUserId = request.ProviderToken,
                ProviderEmail = request.Email
            };

            bool linkCreated = authRepository.CreateOAuthLink(newLink);
            if (!linkCreated)
            {
                return new RegisterResponse { Success = false, Error = "Failed to link OAuth account to user." };
            }

            return new RegisterResponse { Success = true };
        }

        public LoginResponse VerifyOTP(VerifyOTPRequest request)
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

        public void ResendOTP(int userId, string method)
        {
            User? user = authRepository.FindUserById(userId);
            if (user == null)
            {
                return;
            }

            string otp = otpService.GenerateTOTP(user.Id);
            if (method == "email" || user.Preferred2FAMethod == "email")
            {
                emailService.SendOTPCode(user.Email, otp);
            }
        }

        public void RequestPasswordReset(string email)
        {
            User? user = authRepository.FindUserByEmail(email);
            if (user == null)
            {
                return;
            }

            string rawToken = System.Security.Cryptography.RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            PasswordResetToken resetToken = new PasswordResetToken
            {
                UserId = user.Id,
                TokenHash = rawToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                CreatedAt = DateTime.UtcNow
            };

            authRepository.SavePasswordResetToken(resetToken);
            emailService.SendPasswordResetLink(user.Email, rawToken);
        }

        public bool ResetPassword(string token, string newPassword)
        {
            PasswordResetToken? resetToken = authRepository.FindPasswordResetToken(token);

            if (resetToken == null || resetToken.UsedAt != null || resetToken.ExpiresAt < DateTime.UtcNow)
            {
                return false;
            }

            string finalPasswordHash = hashService.GetHash(newPassword);
            bool updated = authRepository.UpdatePassword(resetToken.UserId, finalPasswordHash);

            if (!updated)
            {
                return false;
            }

            resetToken.UsedAt = DateTime.UtcNow;
            authRepository.SavePasswordResetToken(resetToken);
            authRepository.InvalidateAllSessions(resetToken.UserId);

            return true;
        }

        // PRIVATE HELPERS
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

            // Lockout expired, reset and allow login attempt
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

            if (user.Preferred2FAMethod == "email")
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

        private string? ValidateRegistration(RegisterRequest request)
        {
            // There should also be client-side validation, this is last resort
            // can't trust the client
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

        private User CreateUserFromRequest(RegisterRequest request)
        {
            return new User
            {
                Email = request.Email,
                PasswordHash = hashService.GetHash(request.Password),
                FullName = request.FullName,
                PreferredLanguage = "en",
                Is2FAEnabled = false,
                IsLocked = false,
                FailedLoginAttempts = 0
            };
        }

        public bool VerifyResetToken(string token)
        {
            PasswordResetToken? resetToken = authRepository.FindPasswordResetToken(token);

            if (resetToken == null || resetToken.UsedAt != null || resetToken.ExpiresAt < DateTime.UtcNow)
            {
                return false;
            }

            return true;
        }

        public bool Logout(string token)
        {
            Session? session = authRepository.FindSessionByToken(token);
            if (session == null)
            {
                return false;
            }
            authRepository.UpdateSessionToken(session.Id);
            return true;
        }
    }
}
