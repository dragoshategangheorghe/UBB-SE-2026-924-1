using BankApp.Models.Entities;
namespace BankApp.Server.DataAccess.Interfaces
{
    public interface IPasswordResetTokenDAO
    {
        PasswordResetToken Create(int userId, string tokenHash, DateTime expiresAt);
        PasswordResetToken? FindByToken(string tokenHash);
        void MarkAsUsed(int tokenId);
        void DeleteExpired();
    }
}