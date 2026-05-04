using BankApp.Models.Entities;
namespace BankApp.Server.DataAccess.Interfaces
{
    public interface ISessionDAO
    {
        Session Create(int userId, string token, string? deviceInfo, string? browser, string? ip);
        Session? FindByToken(string token);
        List<Session> FindByUserId(int userId);
        void Revoke(int sessionId);
        void RevokeAll(int userId);
    }
}