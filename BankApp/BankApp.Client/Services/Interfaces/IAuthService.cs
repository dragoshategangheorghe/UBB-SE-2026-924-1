using System.Threading.Tasks;
using BankApp.Models.DTOs.Auth;

namespace BankApp.Client.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request);
        Task<LoginResponse?> OAuthLoginAsync(OAuthLoginRequest request);
        Task<RegisterResponse?> RegisterAsync(RegisterRequest request);
        Task<LoginResponse?> VerifyOtpAsync(VerifyOTPRequest request);
        Task ResendOtpAsync(int userId);

        Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request);
        Task<bool> VerifyResetTokenAsync(string token);
        Task<bool> ResetPasswordAsync(ResetPasswordRequest request);

        Task<bool> LogoutAsync();

        /// <summary>
        /// Session context stored locally after login / OAuth / OTP (used before JWT is fully established).
        /// </summary>
        int? GetCurrentUserId();

        /// <summary>
        /// Clears JWT and cached user id without calling the API (e.g. abandon login flow).
        /// </summary>
        void ClearLocalSession();
    }
}

