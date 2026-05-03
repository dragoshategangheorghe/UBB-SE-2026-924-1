using System;
using System.Threading.Tasks;
using BankApp.Client.Utilities;
using BankApp.Models.DTOs.Auth;
using BankApp.Models.Enums;

namespace BankApp.Client.ViewModels
{
    public class TwoFactorViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        public Observable<TwoFactorState> State { get; private set; }

        public TwoFactorViewModel(ApiService apiService)
        {
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
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
                int? userId = _apiService.GetCurrentUserId();
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

                var response = await _apiService.PostAsync<VerifyOTPRequest, LoginResponse>("/api/auth/verify-otp", request);

                if (response != null && response.Success)
                {
                    _apiService.SetToken(response.Token!);
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
                int? userId = _apiService.GetCurrentUserId();
                if (userId == null) return;
                await _apiService.PostAsync<object, object>($"/api/auth/resend-otp?userId={userId.Value}", null);
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