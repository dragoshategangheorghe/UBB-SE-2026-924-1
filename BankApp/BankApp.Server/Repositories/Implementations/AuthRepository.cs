using BankApp.Models.Entities;
using BankApp.Models.Enums;
using BankApp.Models.Extensions;
using BankApp.Server.DataAccess.Interfaces;
using BankApp.Server.Repositories.Interfaces;

namespace BankApp.Server.Repositories.Implementations
{
    public class AuthRepository : IAuthRepository
    {
        private readonly IUserDAO _userDao;
        private readonly ISessionDAO _sessionDao;
        private readonly IOAuthLinkDAO _oAuthLinkDao;
        private readonly IPasswordResetTokenDAO _passwordResetTokenDao;
        private readonly INotificationPreferenceDAO _notificationPreferenceDao;

        public AuthRepository(IUserDAO userDao, ISessionDAO sessionDao, IOAuthLinkDAO oAuthLinkDao,
            IPasswordResetTokenDAO passwordResetTokenDao, INotificationPreferenceDAO notificationPreferenceDao)
        {
            this._userDao = userDao;
            this._sessionDao = sessionDao;
            this._oAuthLinkDao = oAuthLinkDao;
            this._passwordResetTokenDao = passwordResetTokenDao;
            this._notificationPreferenceDao = notificationPreferenceDao;
        }

        // USER
        public User? FindUserByEmail(string email)
        {
            return _userDao.FindByEmail(email);
        }

        public bool CreateUser(User user)
        {
            bool success = _userDao.Create(user);
            if (!success)
            {
                return false;
            }

            User? createdUser = _userDao.FindByEmail(user.Email);
            if (createdUser == null)
            {
                return false;
            }

            foreach (NotificationType type in Enum.GetValues(typeof(NotificationType)))
            {
                success = _notificationPreferenceDao.Create(createdUser.Id, NotificationTypeExtensions.ToDisplayName(type));
                if (!success)
                {
                    return false;
                }
            }

            return true;
        }

        // OAUTH
        public OAuthLink? FindOAuthLink(string provider, string providerUserId)
        {
            return _oAuthLinkDao.FindByProvider(provider, providerUserId);
        }

        public bool CreateOAuthLink(OAuthLink link)
        {
            return _oAuthLinkDao.Create(link.UserId, link.Provider, link.ProviderUserId, link.ProviderEmail);
        }

        // SESSIONS
        public Session CreateSession(int userId, string token, string? deviceInfo, string? browser, string? ip)
        {
            return _sessionDao.Create(userId, token, deviceInfo, browser, ip);
        }

        public Session? FindSessionByToken(string token)
        {
            return _sessionDao.FindByToken(token);
        }

        public void InvalidateAllSessions(int userId)
        {
            _sessionDao.RevokeAll(userId);
        }

        public List<Session> FindSessionsByUserId(int userId)
        {
            return _sessionDao.FindByUserId(userId);
        }

        public bool UpdateSessionToken(int sessionId)
        {
            // Revoke the old session
            // the service layer will create a new one
            _sessionDao.Revoke(sessionId);
            return true;
        }

        public void SavePasswordResetToken(PasswordResetToken token)
        {
            _passwordResetTokenDao.Create(token.Id, token.TokenHash, token.ExpiresAt);
        }

        public PasswordResetToken? FindPasswordResetToken(string tokenHash)
        {
            return _passwordResetTokenDao.FindByToken(tokenHash);
        }

        // ACCOUNT SECURITY
        public void IncrementFailedAttempts(int userId)
        {
            _userDao.IncrementFailedAttempts(userId);
        }

        public void ResetFailedAttempts(int userId)
        {
            _userDao.ResetFailedAttempts(userId);
        }

        public void LockAccount(int userId, DateTime lockoutEnd)
        {
            _userDao.LockAccount(userId, lockoutEnd);
        }

        public User? FindUserById(int id)
        {
            return _userDao.FindById(id);
        }

        public bool UpdatePassword(int userId, string newPasswordHash)
        {
            return _userDao.UpdatePassword(userId, newPasswordHash);
        }
    }
}
