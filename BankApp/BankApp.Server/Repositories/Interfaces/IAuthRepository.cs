using BankApp.Models.Entities;
namespace BankApp.Server.Repositories.Interfaces
{
    public interface IAuthRepository
    {
        User? FindUserByEmail(string email);
        bool CreateUser(User user);
        OAuthLink? FindOAuthLink(string provider, string providerUserId);
        bool CreateOAuthLink(OAuthLink link);
        Session CreateSession(int userId, string token, string? deviceInfo, string? browser, string? ip);
        Session? FindSessionByToken(string token);
        void SavePasswordResetToken(PasswordResetToken token);
        PasswordResetToken? FindPasswordResetToken(string tokenHash);
        void InvalidateAllSessions(int userId);
        User? FindUserById(int id);
        bool UpdatePassword(int userId, string newPasswordHash);

        // Additional methods for session management (could be removed if not needed)
        List<Session> FindSessionsByUserId(int userId);
        bool UpdateSessionToken(int sessionId);
        void IncrementFailedAttempts(int userId);
        void ResetFailedAttempts(int userId);
        void LockAccount(int userId, DateTime lockoutEnd);
    }
}