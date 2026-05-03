using BankApp.Models.DTOs.Auth;
namespace BankApp.Server.Services.Interfaces
{
    public interface IAuthService
    {
        LoginResponse Login(LoginRequest request);
        Task<LoginResponse> OAuthLoginAsync(OAuthLoginRequest request);
        RegisterResponse Register(RegisterRequest request);
        RegisterResponse OAuthRegister(OAuthRegisterRequest request);
        LoginResponse VerifyOTP(VerifyOTPRequest request);
        void ResendOTP(int userId, string method);
        void RequestPasswordReset(string email);
        bool ResetPassword(string token, string newPasswordHash);
        bool Logout(string token);
        bool VerifyResetToken(string token);
    }
}