using BankApp.Client.Services.Interfaces;
using BankApp.Client.Utilities;
using BankApp.Models.DTOs.Auth;
using System.Threading.Tasks;

namespace BankApp.Client.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private class ApiResponse
        {
            public string? message { get; set; }
            public string? error { get; set; }
        }

        private readonly ApiService _apiService;

        public AuthService(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            LoginResponse? response = await _apiService.PostAsync<LoginRequest, LoginResponse>("/api/auth/login", request);
            if (response?.Success != true)
            {
                return response;
            }

            if (response.Requires2FA && response.UserId.HasValue)
            {
                _apiService.SetCurrentUserId(response.UserId.Value);
                return response;
            }

            if (!string.IsNullOrWhiteSpace(response.Token) && response.UserId.HasValue)
            {
                _apiService.SetToken(response.Token);
                _apiService.SetCurrentUserId(response.UserId.Value);
            }

            return response;
        }

        public async Task<LoginResponse?> OAuthLoginAsync(OAuthLoginRequest request)
        {
            LoginResponse? response = await _apiService.PostAsync<OAuthLoginRequest, LoginResponse>("/api/auth/oauth-login", request);
            if (response?.Success != true)
            {
                return response;
            }

            if (response.Requires2FA && response.UserId.HasValue)
            {
                _apiService.SetCurrentUserId(response.UserId.Value);
                return response;
            }

            if (!string.IsNullOrWhiteSpace(response.Token) && response.UserId.HasValue)
            {
                _apiService.SetToken(response.Token);
                _apiService.SetCurrentUserId(response.UserId.Value);
            }

            return response;
        }

        public Task<RegisterResponse?> RegisterAsync(RegisterRequest request)
        {
            return _apiService.PostAsync<RegisterRequest, RegisterResponse>("/api/auth/register", request);
        }

        public async Task<LoginResponse?> VerifyOtpAsync(VerifyOTPRequest request)
        {
            LoginResponse? response = await _apiService.PostAsync<VerifyOTPRequest, LoginResponse>("/api/auth/verify-otp", request);
            if (response?.Success == true && !string.IsNullOrWhiteSpace(response.Token))
            {
                _apiService.SetToken(response.Token);
            }

            return response;
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

        public async Task<bool> LogoutAsync()
        {
            string? token = _apiService.GetToken();
            if (string.IsNullOrWhiteSpace(token))
            {
                _apiService.ClearToken();
                return true;
            }

            ApiResponse? response = await _apiService.PostAsync<object, ApiResponse>("/api/auth/logout", new { });
            _apiService.ClearToken();
            return response != null && response.error == null;
        }
    }
}

