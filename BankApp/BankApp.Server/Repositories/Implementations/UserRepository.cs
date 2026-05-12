namespace BankApp.Server.Repositories.Implementations;

using BankApp.Models.Entities;
using BankApp.Server.DataAccess.Interfaces;
using BankApp.Server.Repositories.Interfaces;
public class UserRepository : IUserRepository
{
    private readonly IUserDAO _userDao;
    private readonly ISessionDAO _sessionDao;
    private readonly IOAuthLinkDAO _oAuthLinkDao;
    private readonly INotificationPreferenceDAO _notificationPreferenceDao;

    public UserRepository(IUserDAO userDao, ISessionDAO sessionDao, IOAuthLinkDAO oAuthLinkDao,
        INotificationPreferenceDAO notificationPreferenceDao)
    {
        this._userDao = userDao;
        this._sessionDao = sessionDao;
        this._notificationPreferenceDao = notificationPreferenceDao;
        this._oAuthLinkDao = oAuthLinkDao;
    }

    public User? FindById(int userId)
    {
        return _userDao.FindById(userId);
    }

    public bool UpdateUser(User user)
    {
        return _userDao.Update(user);
    }

    public bool UpdatePassword(int userId, string newPasswordHash)
    {
        return _userDao.UpdatePassword(userId, newPasswordHash);
    }

    public List<Session> GetActiveSessions(int userId)
    {
        return _sessionDao.FindByUserId(userId);
    }

    public void RevokeSession(int sessionId)
    {
        _sessionDao.Revoke(sessionId);
    }

    public List<OAuthLink> GetLinkedProviders(int userId)
    {
        return _oAuthLinkDao.FindByUserId(userId);
    }

    public bool SaveOAuthLink(int userId, string provider, string providerUserId, string? email)
    {
        return _oAuthLinkDao.Create(userId, provider, providerUserId, email);
    }

    public void DeleteOAuthLink(int linkId)
    {
        _oAuthLinkDao.Delete(linkId);
    }

    public List<NotificationPreference> GetNotificationPreferences(int userId)
    {
        return _notificationPreferenceDao.FindByUserId(userId);
    }

    public bool UpdateNotificationPreferences(int userId, List<NotificationPreference> prefs)
    {
        return _notificationPreferenceDao.Update(userId, prefs);
    }
}