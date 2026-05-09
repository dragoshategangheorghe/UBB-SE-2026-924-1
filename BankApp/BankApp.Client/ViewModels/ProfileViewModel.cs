using BankApp.Models.DTOs.Profile;
using BankApp.Models.Entities;
using BankApp.Models.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Text.Core;
using BankApp.Client.Services.Interfaces;

namespace BankApp.Client.ViewModels
{
    public class ProfileViewModel : BaseViewModel
    {
        private readonly IProfileService _profileService;
        private bool _disposed;
        
        public Observable<ProfileState> State { get; private set; }
        public ProfileInfo ProfileInfo { get; private set; }
        public List<OAuthLink> OAuthLinks { get; private set; }
        public List<Session> ActiveSessions { get; private set; }
        public List<NotificationPreference> NotificationPreferences { get; private set; }

        public ProfileViewModel(IProfileService profileService)
        {
            _profileService = profileService;
            State = new Observable<ProfileState>(ProfileState.Idle);
        }

        public async Task<bool> LoadProfile()
        {
            try
            {
                State.SetValue(ProfileState.Loading);

                GetProfileResponse? profileResponse = await _profileService.GetProfileAsync();

                if (profileResponse == null || !profileResponse.Success || profileResponse.ProfileInfo == null)
                {
                    State.SetValue(ProfileState.Error);
                    return false;
                }

                ProfileInfo = profileResponse.ProfileInfo;

                List<OAuthLink>? oauthResponse = await _profileService.GetOAuthLinksAsync();

                if (oauthResponse == null)
                {
                    State.SetValue(ProfileState.Error);
                    return false;
                }

                OAuthLinks = oauthResponse;

                List<NotificationPreference>? prefsResponse = await _profileService.GetNotificationPreferencesAsync();

                if (prefsResponse == null)
                {
                    State.SetValue(ProfileState.Error);
                    return false;
                }

                NotificationPreferences = prefsResponse;

                State.SetValue(ProfileState.UpdateSuccess);
                return true;
            }
            catch (Exception ex)
            {
                State.SetValue(ProfileState.Error);
                LogError(nameof(UpdatePersonalInfo), ex);
                return false;
            }
        }

        public async Task<bool> UpdatePersonalInfo(string phone, string address, string password)
        {
            try
            {
                State.SetValue(ProfileState.Loading);

                if (ProfileInfo == null || ProfileInfo.UserId == null)
                {
                    State.SetValue(ProfileState.Error);
                    return false;
                }

                phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
                address = string.IsNullOrWhiteSpace(address) ? null : address.Trim();

                UpdateProfileRequest request = new UpdateProfileRequest(ProfileInfo.UserId, phone, address);
                
                UpdateProfileResponse? response = await _profileService.UpdateProfileAsync(request);

                if (response == null)
                {
                    State.SetValue(ProfileState.Error);
                    return false;
                }

                if (response.Success)
                {
                    ProfileInfo.PhoneNumber = (phone == null) ? null : phone.Trim();
                    ProfileInfo.Address = (address == null) ? null : address.Trim();
                    State.SetValue(ProfileState.UpdateSuccess);
                }
                else
                {
                    State.SetValue(ProfileState.Error);
                }

                return response.Success;
            }
            catch (Exception ex)
            {
                State.SetValue(ProfileState.Error);
                LogError(nameof(UpdatePersonalInfo), ex);
                return false;
            }
        }


        public async Task<bool> ChangePassword(string currentPassword, string newPassword)
        {
            try
            {
                State.SetValue(ProfileState.Loading);

                if (ProfileInfo == null || ProfileInfo.UserId == null)
                {
                    State.SetValue(ProfileState.Error);
                    return false;
                }

                ChangePasswordRequest request = new ChangePasswordRequest(ProfileInfo.UserId.Value, currentPassword, newPassword);

                ChangePasswordResponse? result = await _profileService.ChangePasswordAsync(request);

                if (result == null || !result.Success)
                {
                    State.SetValue(ProfileState.Error);
                    return false;
                }

                State.SetValue(ProfileState.UpdateSuccess);
                return result.Success;
            }
            catch (Exception ex)
            {
                State.SetValue(ProfileState.Error);
                LogError(nameof(ChangePassword), ex);
                return false;
            }
        }
        public async Task<bool> EnableTwoFactor(TwoFactorMethod method)
        {
            try
            {
                State.SetValue(ProfileState.Loading);

                var result = await _profileService.Enable2FAAsync(method);

                if (result?.Success == true)
                {
                    ProfileInfo.Is2FAEnabled = true;
                    State.SetValue(ProfileState.UpdateSuccess);
                    return true;
                }

                State.SetValue(ProfileState.Error);
                return false;
            }
            catch (Exception ex)
            {
                State.SetValue(ProfileState.Error);
                LogError(nameof(EnableTwoFactor), ex);
                return false;
            }
            return false;

        }

        public async Task<bool> DisableTwoFactor()
        {
            try
            {
                State.SetValue(ProfileState.Loading);

                var result = await _profileService.Disable2FAAsync();

                if (result?.Success == true)
                {
                    ProfileInfo.Is2FAEnabled = false;
                    State.SetValue(ProfileState.UpdateSuccess);
                    return true;
                }

                State.SetValue(ProfileState.Error);
                return false;
            }
            catch (Exception ex)
            {
                State.SetValue(ProfileState.Error);
                LogError(nameof(DisableTwoFactor), ex);
                return false;
            }
        }


        public Task<bool> LinkOAuth(string provider)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(provider))
                    return Task.FromResult(false);

                var alreadyLinked = OAuthLinks.Exists(o =>
                    string.Equals(o.Provider, provider, StringComparison.OrdinalIgnoreCase));

                if (alreadyLinked)
                    return Task.FromResult(false);

                State.SetValue(ProfileState.Loading);

                // OAuth linking is not yet refactored into the new client-service + repo-proxy layering.
                // Keep UI responsive but report the feature as unavailable for now.
                State.SetValue(ProfileState.Error);
                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                State.SetValue(ProfileState.Error);
                LogError(nameof(LinkOAuth), ex);
                return Task.FromResult(false);
            }
        }


        public Task<bool> UnlinkOAuth(string provider)
        {
            try
            {
                State.SetValue(ProfileState.Error);
                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                State.SetValue(ProfileState.Error);
                LogError(nameof(UnlinkOAuth), ex);
                return Task.FromResult(false);
            }
        }


        public async Task<bool> UpdateNotificationPreferences(List<NotificationPreference> preferences)
        {
            try
            {
                if (preferences == null || preferences.Count == 0)
                    return false;

                State.SetValue(ProfileState.Loading);

                bool result = await _profileService.UpdateNotificationPreferencesAsync(preferences);

                if (result)
                {
                    NotificationPreferences = preferences;
                    State.SetValue(ProfileState.UpdateSuccess);
                }
                else
                {
                    State.SetValue(ProfileState.Error);
                }

                return result;
            }
            catch (Exception ex)
            {
                State.SetValue(ProfileState.Error);
                LogError(nameof(UpdateNotificationPreferences), ex);
                return false;
            }
        }

        public async Task<bool> VerifyPassword(string password)
        {
            try
            {
                State.SetValue(ProfileState.Loading);

                if (ProfileInfo == null || ProfileInfo.UserId == null)
                {
                    State.SetValue(ProfileState.Error);
                    return false;
                }

                bool result = await _profileService.VerifyPasswordAsync(password);

                if (!result)
                {
                    State.SetValue(ProfileState.Error);
                    return false;
                }

                State.SetValue(ProfileState.UpdateSuccess);
                return result;
            }
            catch (Exception ex)
            {
                State.SetValue(ProfileState.Error);
                LogError(nameof(VerifyPassword), ex);
                return false;
            }
        }

        public override void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        private void LogError(string method, Exception ex) =>
            Console.Error.WriteLine($"[ProfileViewModel] {method}: {ex.Message}");
    }
}