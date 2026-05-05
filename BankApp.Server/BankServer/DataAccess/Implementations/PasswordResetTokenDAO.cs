using System.Data;
using BankApp.Models.Entities;
using BankApp.Server.DataAccess.Interfaces;

namespace BankApp.Server.DataAccess.Implementations
{
    public class PasswordResetTokenDAO : IPasswordResetTokenDAO
    {
        private readonly AppDbContext context;
        public PasswordResetTokenDAO(AppDbContext context)
        {
            this.context = context;
        }

        public PasswordResetToken Create(int userId, string tokenHash, DateTime expiresAt)
        {
            string sql = @"
                INSERT INTO PasswordResetToken (UserId, TokenHash, ExpiresAt) 
                OUTPUT INSERTED.Id, INSERTED.UserId, INSERTED.TokenHash, INSERTED.ExpiresAt, INSERTED.UsedAt, INSERTED.CreatedAt
                VALUES (@p0, @p1, @p2)";

            using var reader = context.ExecuteQuery(sql, new object[] { userId, tokenHash, expiresAt });

            if (reader.Read())
            {
                return MapToPRT(reader);
            }

            throw new Exception("Failed to create password reset token.");
        }

        public void DeleteExpired()
        {
            string sql = "DELETE FROM PasswordResetToken WHERE ExpiresAt < GETUTCDATE()";
            context.ExecuteNonQuery(sql, Array.Empty<object>());
        }

        public PasswordResetToken? FindByToken(string tokenHash)
        {
            string sql = "SELECT Id, UserId, TokenHash, ExpiresAt, UsedAt, CreatedAt FROM PasswordResetToken WHERE TokenHash = @p0";
            using var reader = context.ExecuteQuery(sql, new object[] { tokenHash });

            if (reader.Read())
            {
                return MapToPRT(reader);
            }
            return null;
        }

        public void MarkAsUsed(int tokenId)
        {
            string sql = "UPDATE PasswordResetToken SET UsedAt = GETUTCDATE() WHERE Id = @p0";
            context.ExecuteNonQuery(sql, new object[] { tokenId });
        }

        private PasswordResetToken MapToPRT(IDataReader reader)
        {
            return new PasswordResetToken
            {
                Id = reader.GetInt32(0),
                UserId = reader.GetInt32(1),
                TokenHash = reader.GetString(2),
                ExpiresAt = reader.GetDateTime(3),
                UsedAt = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                CreatedAt = reader.GetDateTime(5)
            };
        }
    }
}
