using System.Collections.Generic;
using System.Threading.Tasks;
using BankApp.Models.DTOs.Profile;
using BankApp.Models.Entities;
using BankApp.Models.Enums;

namespace BankApp.Client.RepoProxies.Interfaces
{
    public interface IProfileRepoProxy
    {
        Task<GetProfileResponse?> GetProfileAsync();

        Task<List<OAuthLink>?> GetOAuthLinksAsync();

        Task<List<NotificationPreference>?> GetNotificationPreferencesAsync();

        Task<UpdateProfileResponse?> UpdateProfileAsync(UpdateProfileRequest request);

        Task<ChangePasswordResponse?> ChangePasswordAsync(ChangePasswordRequest request);

        Task<Toggle2FAResponse?> Enable2FAAsync(TwoFactorMethod method);

        Task<Toggle2FAResponse?> Disable2FAAsync();

        Task<bool> VerifyPasswordAsync(string password);

        Task<bool> UpdateNotificationPreferencesAsync(List<NotificationPreference> prefs);
    }
}
