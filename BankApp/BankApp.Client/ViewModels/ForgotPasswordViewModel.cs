using System;
using System.Threading.Tasks;
using BankApp.Client.Services.Interfaces;
using BankApp.Models.DTOs.Auth;
using BankApp.Models.Enums;

namespace BankApp.Client.ViewModels
{
    public class ForgotPasswordViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        public Observable<ForgotPasswordState> State { get; private set; }

        public ForgotPasswordViewModel(IAuthService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
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
                bool ok = await _authService.ForgotPasswordAsync(request);
                if (ok)
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
                bool ok = await _authService.ResetPasswordAsync(request);
                if (ok)
                {
                    SetState(State, ForgotPasswordState.PasswordResetSuccess);
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

        public async Task VerifyToken(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                SetState(State, ForgotPasswordState.Error);
                return;
            }

            try
            {
                bool ok = await _authService.VerifyResetTokenAsync(code);
                if (ok)
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