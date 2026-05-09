using BankApp.Client.Services.Interfaces;
using BankApp.Client.Utilities;
using BankApp.Models.DTOs.Profile;
using BankApp.Models.Entities;
using BankApp.Models.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankApp.Client.Services.Implementations
{
    public class ProfileService : IProfileService
    {
        private readonly ApiService _apiService;

        public ProfileService(ApiService apiService)
        {
            _apiService = apiService;
        }

        public Task<GetProfileResponse?> GetProfileAsync()
        {
            return _apiService.GetAsync<GetProfileResponse>("/api/profile");
        }

        public Task<List<OAuthLink>?> GetOAuthLinksAsync()
        {
            return _apiService.GetAsync<List<OAuthLink>>("/api/profile/oauthlinks");
        }

        public Task<List<NotificationPreference>?> GetNotificationPreferencesAsync()
        {
            return _apiService.GetAsync<List<NotificationPreference>>("/api/profile/notifications/preferences");
        }

        public Task<UpdateProfileResponse?> UpdateProfileAsync(UpdateProfileRequest request)
        {
            return _apiService.PutAsync<UpdateProfileRequest, UpdateProfileResponse>("/api/profile", request);
        }

        public Task<ChangePasswordResponse?> ChangePasswordAsync(ChangePasswordRequest request)
        {
            return _apiService.PutAsync<ChangePasswordRequest, ChangePasswordResponse>("/api/profile/password", request);
        }

        public Task<Toggle2FAResponse?> Enable2FAAsync(TwoFactorMethod method)
        {
            return _apiService.PutAsync<object, Toggle2FAResponse>("/api/profile/2fa/enable", new { Method = method });
        }

        public Task<Toggle2FAResponse?> Disable2FAAsync()
        {
            return _apiService.PutAsync<object, Toggle2FAResponse>("/api/profile/2fa/disable", new { });
        }

        public async Task<bool> VerifyPasswordAsync(string password)
        {
            return await _apiService.PostAsync<string, bool>("/api/profile/verify-password", password);
        }

        public async Task<bool> UpdateNotificationPreferencesAsync(List<NotificationPreference> prefs)
        {
            return await _apiService.PutAsync<List<NotificationPreference>, bool>("/api/profile/notifications/preferences", prefs);
        }
    }
}

