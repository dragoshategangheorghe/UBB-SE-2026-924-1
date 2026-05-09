using System.Collections.Generic;
using System.Threading.Tasks;
using BankApp.Client.RepoProxies;
using BankApp.Client.RepoProxies.Interfaces;
using BankApp.Models.DTOs.Profile;
using BankApp.Models.Entities;
using BankApp.Models.Enums;

namespace BankApp.Client.RepoProxies.Implementations
{
    public class ProfileRepoProxy : IProfileRepoProxy
    {
        private readonly ApiService _apiService;

        public ProfileRepoProxy(ApiService apiService)
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

        public Task<bool> VerifyPasswordAsync(string password)
        {
            return _apiService.PostAsync<string, bool>("/api/profile/verify-password", password);
        }

        public Task<bool> UpdateNotificationPreferencesAsync(List<NotificationPreference> prefs)
        {
            return _apiService.PutAsync<List<NotificationPreference>, bool>("/api/profile/notifications/preferences", prefs);
        }
    }
}
