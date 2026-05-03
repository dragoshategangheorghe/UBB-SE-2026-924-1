using BankApp.Client.Utilities;

using BankApp.Models.DTOs.Profile;
using BankApp.Models.Entities;
using BankApp.Models.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Text.Core;

namespace BankApp.Client.ViewModels
{
    public class ProfileViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private bool _disposed;
        
        public Observable<ProfileState> State { get; private set; }
        public ProfileInfo ProfileInfo { get; private set; }
        public List<OAuthLink> OAuthLinks { get; private set; }
        public List<Session> ActiveSessions { get; private set; }
        public List<NotificationPreference> NotificationPreferences { get; private set; }

        public ProfileViewModel(ApiService apiService)
        {
            _apiService = apiService;
            State = new Observable<ProfileState>(ProfileState.Idle);
        }

        public async Task<bool> LoadProfile()
        {
            try
            {
                State.SetValue(ProfileState.Loading);

                GetProfileResponse? profileResponse = await _apiService.GetAsync<GetProfileResponse>(
                    $"api/profile/");

                if (profileResponse == null || !profileResponse.Success || profileResponse.ProfileInfo == null)
                {
                    State.SetValue(ProfileState.Error);
                    return false;
                }

                ProfileInfo = profileResponse.ProfileInfo;

                List<OAuthLink>? oauthResponse = await _apiService.GetAsync<List<OAuthLink>>(
                    $"api/profile/oauthlinks");

                if (oauthResponse == null)
                {
                    State.SetValue(ProfileState.Error);
                    return false;
                }

                OAuthLinks = oauthResponse;

                List<NotificationPreference>? prefsResponse = await _apiService.GetAsync<List<NotificationPreference>>(
                    $"api/profile/notifications/preferences");

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
                
                UpdateProfileResponse? response = await _apiService.PutAsync<UpdateProfileRequest, UpdateProfileResponse>(
                    $"api/profile/", request);

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

                ChangePasswordResponse? result = await _apiService.PutAsync<ChangePasswordRequest, ChangePasswordResponse>(
                    $"api/profile/password", request);

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

                var request = new { Method = method };

                var result = await _apiService.PutAsync<object, Toggle2FAResponse>(
                    $"api/profile/2fa/enable", request);

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

                var result = await _apiService.PutAsync<object, Toggle2FAResponse>(
                    $"api/profile/2fa/disable", new { });

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


        public async Task<bool> LinkOAuth(string provider)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(provider))
                    return false;

                var alreadyLinked = OAuthLinks.Exists(o =>
                    string.Equals(o.Provider, provider, StringComparison.OrdinalIgnoreCase));

                if (alreadyLinked)
                    return false;

                State.SetValue(ProfileState.Loading);

                var request = new { Provider = provider.Trim() };

                var result = await _apiService.PostAsync<object, bool>(
                    $"api/profile/oauth/link", request);

                if (result)
                {
                    /*
                    OAuthLinks.Add(new OAuthLink { Provider = provider, UserId = ProfileInfo.UserId });*/
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
                LogError(nameof(LinkOAuth), ex);
                return false;
            }
        }


        public async Task<bool> UnlinkOAuth(string provider)
        {
            try
            {
                /*
                if (string.IsNullOrWhiteSpace(provider))
                    return false;

                var existing = OAuthLinks.Find(o =>
                    string.Equals(o.Provider, provider, StringComparison.OrdinalIgnoreCase));

                if (existing == null)
                    return false;

                State.SetValue(ProfileState.Loading);

                var request = new { Provider = provider.Trim() };

                var result = await _apiService.PostAsync<object, bool>(
                    $"api/profile/{CurrentUser.Id}/oauth/unlink", request);

                if (result)
                {
                    OAuthLinks.Remove(existing);
                    State.SetValue(ProfileState.UpdateSuccess);
                }
                else
                {
                    State.SetValue(ProfileState.Error);
                }

                return result;*/
                return true;
            }
            catch (Exception ex)
            {
                State.SetValue(ProfileState.Error);
                LogError(nameof(UnlinkOAuth), ex);
                return false;
            }
        }


        public async Task<bool> UpdateNotificationPreferences(List<NotificationPreference> preferences)
        {
            try
            {
                if (preferences == null || preferences.Count == 0)
                    return false;

                State.SetValue(ProfileState.Loading);

                var result = await _apiService.PutAsync<List<NotificationPreference>, bool>(
                    $"api/profile/notifications/preferences", preferences);

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

                bool? response = await _apiService.PostAsync<string, bool>(
                    $"api/profile/verify-password", password);

                bool result = response ?? false;

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