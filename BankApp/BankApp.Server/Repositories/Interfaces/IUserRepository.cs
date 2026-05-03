using BankApp.Models.Entities;
namespace BankApp.Server.Repositories.Interfaces
{
    public interface IUserRepository
    {
        User? FindById(int userId);
        bool UpdateUser(User user);
        bool UpdatePassword(int userId, string newPasswordHash);
        List<Session> GetActiveSessions(int userId);
        void RevokeSession(int sessionId);
        List<OAuthLink> GetLinkedProviders(int userId);
        bool SaveOAuthLink(int userId, string provider, string providerUserId, string? email);
        void DeleteOAuthLink(int linkId);
        List<NotificationPreference> GetNotificationPreferences(int userId);
        bool UpdateNotificationPreferences(int userId, List<NotificationPreference> prefs);
    }
}
