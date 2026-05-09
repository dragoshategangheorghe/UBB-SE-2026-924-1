using System.Threading.Tasks;
using BankApp.Models.DTOs.Auth;

namespace BankApp.Client.RepoProxies.Interfaces
{
    public interface IAuthApiService
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request);

        Task<LoginResponse?> OAuthLoginAsync(OAuthLoginRequest request);

        Task<RegisterResponse?> RegisterAsync(RegisterRequest request);

        Task<LoginResponse?> VerifyOtpAsync(VerifyOTPRequest request);

        Task ResendOtpAsync(int userId);

        Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request);

        Task<bool> VerifyResetTokenAsync(string token);

        Task<bool> ResetPasswordAsync(ResetPasswordRequest request);

        Task<bool> LogoutPostAsync();

        void SetBearerToken(string token);

        void SetCurrentUserId(int userId);

        void ClearLocalSession();

        int? GetCurrentUserId();

        string? GetBearerToken();
    }
}
