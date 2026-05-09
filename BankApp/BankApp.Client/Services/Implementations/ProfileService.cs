using System.Collections.Generic;
using System.Threading.Tasks;
using BankApp.Client.RepoProxies.Interfaces;
using BankApp.Client.Services.Interfaces;
using BankApp.Models.DTOs.Profile;
using BankApp.Models.Entities;
using BankApp.Models.Enums;

namespace BankApp.Client.Services.Implementations
{
    public class ProfileService : IProfileService
    {
        private readonly IProfileApiService _profileRepo;

        public ProfileService(IProfileApiService profileRepo)
        {
            _profileRepo = profileRepo;
        }

        public Task<GetProfileResponse?> GetProfileAsync()
        {
            return _profileRepo.GetProfileAsync();
        }

        public Task<List<OAuthLink>?> GetOAuthLinksAsync()
        {
            return _profileRepo.GetOAuthLinksAsync();
        }

        public Task<List<NotificationPreference>?> GetNotificationPreferencesAsync()
        {
            return _profileRepo.GetNotificationPreferencesAsync();
        }

        public Task<UpdateProfileResponse?> UpdateProfileAsync(UpdateProfileRequest request)
        {
            return _profileRepo.UpdateProfileAsync(request);
        }

        public Task<ChangePasswordResponse?> ChangePasswordAsync(ChangePasswordRequest request)
        {
            return _profileRepo.ChangePasswordAsync(request);
        }

        public Task<Toggle2FAResponse?> Enable2FAAsync(TwoFactorMethod method)
        {
            return _profileRepo.Enable2FAAsync(method);
        }

        public Task<Toggle2FAResponse?> Disable2FAAsync()
        {
            return _profileRepo.Disable2FAAsync();
        }

        public Task<bool> VerifyPasswordAsync(string password)
        {
            return _profileRepo.VerifyPasswordAsync(password);
        }

        public Task<bool> UpdateNotificationPreferencesAsync(List<NotificationPreference> prefs)
        {
            return _profileRepo.UpdateNotificationPreferencesAsync(prefs);
        }
    }
}
