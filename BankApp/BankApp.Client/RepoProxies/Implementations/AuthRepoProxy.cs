using System.Threading.Tasks;
using BankApp.Client.RepoProxies;
using BankApp.Client.RepoProxies.Interfaces;
using BankApp.Models.DTOs.Auth;

namespace BankApp.Client.RepoProxies.Implementations
{
    public class AuthRepoProxy : IAuthRepoProxy
    {
        private class ApiResponse
        {
            public string? message { get; set; }

            public string? error { get; set; }
        }

        private readonly ApiService _apiService;

        public AuthRepoProxy(ApiService apiService)
        {
            _apiService = apiService;
        }

        public Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            return _apiService.PostAllowBadRequestAsync<LoginRequest, LoginResponse>("/api/auth/login", request);
        }

        public Task<LoginResponse?> OAuthLoginAsync(OAuthLoginRequest request)
        {
            return _apiService.PostAllowBadRequestAsync<OAuthLoginRequest, LoginResponse>("/api/auth/oauth-login", request);
        }

        public Task<RegisterResponse?> RegisterAsync(RegisterRequest request)
        {
            return _apiService.PostAllowBadRequestAsync<RegisterRequest, RegisterResponse>("/api/auth/register", request);
        }

        public Task<LoginResponse?> VerifyOtpAsync(VerifyOTPRequest request)
        {
            return _apiService.PostAllowBadRequestAsync<VerifyOTPRequest, LoginResponse>("/api/auth/verify-otp", request);
        }

        public Task ResendOtpAsync(int userId)
        {
            return _apiService.PostAsync<object, object>($"/api/auth/resend-otp?userId={userId}", new { });
        }

        public async Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            ApiResponse? response = await _apiService.PostAsync<ForgotPasswordRequest, ApiResponse>("/api/auth/forgot-password", request);
            return response != null && response.error == null;
        }

        public async Task<bool> VerifyResetTokenAsync(string token)
        {
            ApiResponse? response = await _apiService.PostAsync<object, ApiResponse>("/api/auth/verify-reset-token", new { Token = token });
            return response != null && response.error == null;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
        {
            ApiResponse? response = await _apiService.PostAsync<ResetPasswordRequest, ApiResponse>("/api/auth/reset-password", request);
            return response != null && response.error == null;
        }

        public async Task<bool> LogoutPostAsync()
        {
            ApiResponse? response = await _apiService.PostAsync<object, ApiResponse>("/api/auth/logout", new { });
            return response != null && response.error == null;
        }

        public void SetBearerToken(string token) => _apiService.SetToken(token);

        public void SetCurrentUserId(int userId) => _apiService.SetCurrentUserId(userId);

        public void ClearLocalSession() => _apiService.ClearToken();

        public int? GetCurrentUserId() => _apiService.GetCurrentUserId();

        public string? GetBearerToken() => _apiService.GetToken();
    }
}
