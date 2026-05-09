using System;
using System.Threading.Tasks;
using BankApp.Client.Services.Interfaces;
using BankApp.Models.DTOs.Auth;
using BankApp.Models.Enums;

namespace BankApp.Client.ViewModels
{
    public class TwoFactorViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        public Observable<TwoFactorState> State { get; private set; }

        public TwoFactorViewModel(IAuthService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            State = new Observable<TwoFactorState>(TwoFactorState.Idle);
        }

        public async Task VerifyOTP(string otp)
        {
            if (string.IsNullOrWhiteSpace(otp))
            {
                SetState(State, TwoFactorState.InvalidOTP);
                return;
            }

            SetState(State, TwoFactorState.Verifying);

            try
            {
                int? userId = _authService.GetCurrentUserId();
                if (userId == null)
                {
                    SetState(State, TwoFactorState.InvalidOTP);
                    return;
                }

                var request = new VerifyOTPRequest
                {
                    UserId = userId.Value,
                    OTPCode = otp
                };

                var response = await _authService.VerifyOtpAsync(request);

                if (response != null && response.Success)
                {
                    SetState(State, TwoFactorState.Success);
                }
                else
                {
                    SetState(State, TwoFactorState.InvalidOTP);
                }
            }
            catch (Exception)
            {
                SetState(State, TwoFactorState.InvalidOTP);
            }
        }

        public async Task ResendOTP()
        {
            SetState(State, TwoFactorState.Idle);
            try
            {
                int? userId = _authService.GetCurrentUserId();
                if (userId == null) return;
                await _authService.ResendOtpAsync(userId.Value);
            }
            catch (Exception)
            {
                ;
            }
        }

        public override void Dispose()
        {
            State = null;
        }
    }
}