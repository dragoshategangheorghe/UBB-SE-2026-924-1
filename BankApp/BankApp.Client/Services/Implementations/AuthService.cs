using System.Threading.Tasks;
using BankApp.Client.RepoProxies.Interfaces;
using BankApp.Client.Services.Interfaces;
using BankApp.Models.DTOs.Auth;

namespace BankApp.Client.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IAuthApiService _authRepo;

        public AuthService(IAuthApiService authRepo)
        {
            _authRepo = authRepo;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            LoginResponse? response = await _authRepo.LoginAsync(request);
            if (response?.Success != true)
            {
                return response;
            }

            if (response.Requires2FA && response.UserId.HasValue)
            {
                _authRepo.SetCurrentUserId(response.UserId.Value);
                return response;
            }

            if (!string.IsNullOrWhiteSpace(response.Token) && response.UserId.HasValue)
            {
                _authRepo.SetBearerToken(response.Token);
                _authRepo.SetCurrentUserId(response.UserId.Value);
            }

            return response;
        }

        public async Task<LoginResponse?> OAuthLoginAsync(OAuthLoginRequest request)
        {
            LoginResponse? response = await _authRepo.OAuthLoginAsync(request);
            if (response?.Success != true)
            {
                return response;
            }

            if (response.Requires2FA && response.UserId.HasValue)
            {
                _authRepo.SetCurrentUserId(response.UserId.Value);
                return response;
            }

            if (!string.IsNullOrWhiteSpace(response.Token) && response.UserId.HasValue)
            {
                _authRepo.SetBearerToken(response.Token);
                _authRepo.SetCurrentUserId(response.UserId.Value);
            }

            return response;
        }

        public Task<RegisterResponse?> RegisterAsync(RegisterRequest request)
        {
            return _authRepo.RegisterAsync(request);
        }

        public async Task<LoginResponse?> VerifyOtpAsync(VerifyOTPRequest request)
        {
            LoginResponse? response = await _authRepo.VerifyOtpAsync(request);
            if (response?.Success == true && !string.IsNullOrWhiteSpace(response.Token))
            {
                _authRepo.SetBearerToken(response.Token);
            }

            return response;
        }

        public Task ResendOtpAsync(int userId)
        {
            return _authRepo.ResendOtpAsync(userId);
        }

        public Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            return _authRepo.ForgotPasswordAsync(request);
        }

        public Task<bool> VerifyResetTokenAsync(string token)
        {
            return _authRepo.VerifyResetTokenAsync(token);
        }

        public Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
        {
            return _authRepo.ResetPasswordAsync(request);
        }

        public async Task<bool> LogoutAsync()
        {
            string? token = _authRepo.GetBearerToken();
            if (string.IsNullOrWhiteSpace(token))
            {
                _authRepo.ClearLocalSession();
                return true;
            }

            bool ok = await _authRepo.LogoutPostAsync();
            _authRepo.ClearLocalSession();
            return ok;
        }

        public int? GetCurrentUserId() => _authRepo.GetCurrentUserId();

        public void ClearLocalSession() => _authRepo.ClearLocalSession();
    }
}
