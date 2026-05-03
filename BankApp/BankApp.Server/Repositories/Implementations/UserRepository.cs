using BankApp.Models.Enums;

namespace BankApp.Server.Repositories.Implementations;

using BankApp.Models.Entities;
using BankApp.Server.DataAccess.Interfaces;
using BankApp.Server.Repositories.Interfaces;
public class UserRepository : IUserRepository
{
    private readonly IUserDAO userDao;
    private readonly ISessionDAO sessionDao;
    private readonly IOAuthLinkDAO oAuthLinkDao;
    private readonly INotificationPreferenceDAO notificationPreferenceDao;

    public UserRepository(IUserDAO userDao, ISessionDAO sessionDao, IOAuthLinkDAO oAuthLinkDao,
        INotificationPreferenceDAO notificationPreferenceDao)
    {
        this.userDao = userDao;
        this.sessionDao = sessionDao;
        this.notificationPreferenceDao = notificationPreferenceDao;
        this.oAuthLinkDao = oAuthLinkDao;
    }

    public User? FindById(int id)
    {
        return userDao.FindById(id);
    }

    public bool UpdateUser(User user)
    {
        return userDao.Update(user);
    }

    public bool UpdatePassword(int userId, string newPasswordHash)
    {
        return userDao.UpdatePassword(userId, newPasswordHash);
    }

    public List<Session> GetActiveSessions(int userId)
    {
        return sessionDao.FindByUserId(userId);
    }

    public void RevokeSession(int sessionId)
    {
        sessionDao.Revoke(sessionId);
    }

    public List<OAuthLink> GetLinkedProviders(int userId)
    {
        return oAuthLinkDao.FindByUserId(userId);
    }

    public bool SaveOAuthLink(int userId, string provider, string providerUserId, string? email)
    {
        return oAuthLinkDao.Create(userId, provider, providerUserId, email);
    }

    public void DeleteOAuthLink(int linkId)
    {
        oAuthLinkDao.Delete(linkId);
    }

    public List<NotificationPreference> GetNotificationPreferences(int userId)
    {
        return notificationPreferenceDao.FindByUserId(userId);
    }

    public bool UpdateNotificationPreferences(int userId, List<NotificationPreference> prefs)
    {
        return notificationPreferenceDao.Update(userId, prefs);
    }
}