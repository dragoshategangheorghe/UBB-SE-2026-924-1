using BankApp.Models.DTOs.Auth;
using System.Threading.Tasks;

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
    }
}

