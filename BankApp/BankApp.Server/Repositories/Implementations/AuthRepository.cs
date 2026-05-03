using BankApp.Server.Repositories.Interfaces;
using BankApp.Server.DataAccess.Interfaces;
using BankApp.Models.Entities;
using BankApp.Models.Enums;
using BankApp.Models.Extensions;

namespace BankApp.Server.Repositories.Implementations
{
    public class AuthRepository : IAuthRepository
    {
        private readonly IUserDAO userDao;
        private readonly ISessionDAO sessionDao;
        private readonly IOAuthLinkDAO oAuthLinkDao;
        private readonly IPasswordResetTokenDAO passwordResetTokenDao;
        private readonly INotificationPreferenceDAO notificationPreferenceDao;

        public AuthRepository(IUserDAO userDao, ISessionDAO sessionDao, IOAuthLinkDAO oAuthLinkDao,
            IPasswordResetTokenDAO passwordResetTokenDao, INotificationPreferenceDAO notificationPreferenceDao)
        {
            this.userDao = userDao;
            this.sessionDao = sessionDao;
            this.oAuthLinkDao = oAuthLinkDao;
            this.passwordResetTokenDao = passwordResetTokenDao;
            this.notificationPreferenceDao = notificationPreferenceDao;
        }

        // USER
        public User? FindUserByEmail(string email)
        {
            return userDao.FindByEmail(email);
        }

        public bool CreateUser(User user)
        {
            bool success = userDao.Create(user);
            if (!success)
            {
                return false;
            }

            User? createdUser = userDao.FindByEmail(user.Email);
            if (createdUser == null)
            {
                return false;
            }

            foreach (NotificationType type in Enum.GetValues(typeof(NotificationType)))
            {
                success = notificationPreferenceDao.Create(createdUser.Id, NotificationTypeExtensions.ToDisplayName(type));
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
            return oAuthLinkDao.FindByProvider(provider, providerUserId);
        }

        public bool CreateOAuthLink(OAuthLink link)
        {
            return oAuthLinkDao.Create(link.UserId, link.Provider, link.ProviderUserId, link.ProviderEmail);
        }

        // SESSIONS
        public Session CreateSession(int userId, string token, string? deviceInfo, string? browser, string? ip)
        {
            return sessionDao.Create(userId, token, deviceInfo, browser, ip);
        }

        public Session? FindSessionByToken(string token)
        {
            return sessionDao.FindByToken(token);
        }

        public void InvalidateAllSessions(int userId)
        {
            sessionDao.RevokeAll(userId);
        }

        public List<Session> FindSessionsByUserId(int userId)
        {
            return sessionDao.FindByUserId(userId);
        }

        public bool UpdateSessionToken(int sessionId)
        {
            // Revoke the old session
            // the service layer will create a new one
            sessionDao.Revoke(sessionId);
            return true;
        }

        public void SavePasswordResetToken(PasswordResetToken token)
        {
            passwordResetTokenDao.Create(token.UserId, token.TokenHash, token.ExpiresAt);
        }

        public PasswordResetToken? FindPasswordResetToken(string tokenHash)
        {
            return passwordResetTokenDao.FindByToken(tokenHash);
        }

        // ACCOUNT SECURITY
        public void IncrementFailedAttempts(int userId)
        {
            userDao.IncrementFailedAttempts(userId);
        }

        public void ResetFailedAttempts(int userId)
        {
            userDao.ResetFailedAttempts(userId);
        }

        public void LockAccount(int userId, DateTime lockoutEnd)
        {
            userDao.LockAccount(userId, lockoutEnd);
        }

        public User? FindUserById(int id)
        {
            return userDao.FindById(id);
        }

        public bool UpdatePassword(int userId, string newPasswordHash)
        {
            return userDao.UpdatePassword(userId, newPasswordHash);
        }
    }
}
