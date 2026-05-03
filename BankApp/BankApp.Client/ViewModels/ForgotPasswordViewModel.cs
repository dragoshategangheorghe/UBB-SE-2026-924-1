using System;
using System.Threading.Tasks;
using BankApp.Client.Utilities;
using BankApp.Models.DTOs.Auth;
using BankApp.Models.Enums;

namespace BankApp.Client.ViewModels
{
    public class ApiResponse
    {
        public string? message { get; set; }
        public string? error { get; set; }
    }

    public class ForgotPasswordViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        public Observable<ForgotPasswordState> State { get; private set; }

        public ForgotPasswordViewModel(ApiService apiService)
        {
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            State = new Observable<ForgotPasswordState>(ForgotPasswordState.Idle);
        }

        public async Task ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                SetState(State, ForgotPasswordState.Error);
                return;
            }

            try
            {
                var request = new ForgotPasswordRequest { Email = email };
                var response = await _apiService.PostAsync<ForgotPasswordRequest, ApiResponse>("/api/auth/forgot-password", request);
                if (response != null && response.error == null)
                {
                    SetState(State, ForgotPasswordState.EmailSent);
                }
                else
                {
                    SetState(State, ForgotPasswordState.Error);
                }
            }
            catch (Exception)
            {
                SetState(State, ForgotPasswordState.Error);
            }
        }

        public async Task ResetPassword(string email, string newPassword, string code)
        {
            if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(code))
            {
                SetState(State, ForgotPasswordState.Error);
                return;
            }

            try
            {
                var request = new ResetPasswordRequest
                {
                    Token = code,
                    NewPassword = newPassword
                };
                var response = await _apiService.PostAsync<ResetPasswordRequest, ApiResponse>("/api/auth/reset-password", request);
                if (response != null && response.error == null)
                {
                    SetState(State, ForgotPasswordState.PasswordResetSuccess);
                }
                else
                {
                    if (response?.error != null && response.error.Contains("expired", StringComparison.OrdinalIgnoreCase))
                    {
                        SetState(State, ForgotPasswordState.TokenExpired);
                    }
                    else
                    {
                        SetState(State, ForgotPasswordState.Error);
                    }
                }
            }
            catch (Exception)
            {
                SetState(State, ForgotPasswordState.Error);
            }
        }

        public async Task VerifyToken(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                SetState(State, ForgotPasswordState.Error);
                return;
            }

            try
            {
                var response = await _apiService.PostAsync<object, ApiResponse>("/api/auth/verify-reset-token", new { Token = code });

                if (response != null && response.error == null)
                {
                    SetState(State, ForgotPasswordState.TokenValid);
                }
                else
                {
                    SetState(State, ForgotPasswordState.TokenExpired);
                }
            }
            catch (Exception)
            {
                SetState(State, ForgotPasswordState.Error);
            }
        }


        public override void Dispose()
        {
            State = null;
        }
    }
}