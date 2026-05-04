using Microsoft.AspNetCore.Mvc;
using BankApp.Server.Services.Interfaces;
using BankApp.Models.DTOs.Auth;
using BankApp.Server.DataAccess;
using Microsoft.AspNetCore.Connections.Features;

namespace BankApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService authService;
        public AuthController(IAuthService authService)
        {
            this.authService = authService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            LoginResponse response = authService.Login(request);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            RegisterResponse response = authService.Register(request);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("verify-otp")]
        public IActionResult VerifyOTP([FromBody] VerifyOTPRequest request)
        {
            LoginResponse response = authService.VerifyOTP(request);
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
            authService.RequestPasswordReset(request.Email);

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

            bool isSuccess = authService.ResetPassword(request.Token, request.NewPassword);
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

            if (!authService.Logout(token))
            {
                return BadRequest(new { error = "Invalid session." });
            }

            return Ok(new { message = "Logged out successfully." });
        }

        [HttpPost("resend-otp")]
        public IActionResult ResendOTP([FromQuery] int userId, [FromQuery] string method = "email")
        {
            authService.ResendOTP(userId, method);
            return Ok(new { message = "If the user exists, a new code has been sent." });
        }

        [HttpPost("oauth-login")]
        public async Task<IActionResult> OAuthLogin([FromBody] OAuthLoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Provider) || string.IsNullOrWhiteSpace(request.ProviderToken))
            {
                return BadRequest(new { error = "Provider and ProviderToken are required." });
            }

            LoginResponse response = await authService.OAuthLoginAsync(request);

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

            bool isValid = authService.VerifyResetToken(request.Token);
            if (!isValid)
            {
                return BadRequest(new { error = "Invalid or expired token." });
            }

            return Ok(new { message = "Token is valid." });
        }
    }
}