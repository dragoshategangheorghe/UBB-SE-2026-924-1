using BankApp.Models.Entities;
using BankApp.Server.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Data;

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
            //string sql = @"
            //    INSERT INTO PasswordResetToken (UserId, TokenHash, ExpiresAt) 
            //    OUTPUT INSERTED.Id, INSERTED.UserId, INSERTED.TokenHash, INSERTED.ExpiresAt, INSERTED.UsedAt, INSERTED.CreatedAt
            //    VALUES (@p0, @p1, @p2)";

            //using var reader = context.ExecuteQuery(sql, new object[] { userId, tokenHash, expiresAt });

            //if (reader.Read())
            //{
            //    return MapToPRT(reader);
            //}

            //throw new Exception("Failed to create password reset token.");

            var token = new PasswordResetToken
            {
                UserId = userId,
                TokenHash = tokenHash,
                ExpiresAt = expiresAt
            };

            context.PasswordResetTokens.Add(token);

            var rows = context.SaveChanges();

            if (rows <= 0)
                throw new Exception("Failed to create password reset token.");

            return token;
        }

        public void DeleteExpired()
        {
            //string sql = "DELETE FROM PasswordResetToken WHERE ExpiresAt < GETUTCDATE()";
            //context.ExecuteNonQuery(sql, Array.Empty<object>());

            context.PasswordResetTokens
                .Where(t => t.ExpiresAt < DateTime.UtcNow)
                .ExecuteDelete();
        }

        public PasswordResetToken? FindByToken(string tokenHash)
        {
            //string sql = "SELECT Id, UserId, TokenHash, ExpiresAt, UsedAt, CreatedAt FROM PasswordResetToken WHERE TokenHash = @p0";
            //using var reader = context.ExecuteQuery(sql, new object[] { tokenHash });

            //if (reader.Read())
            //{
            //    return MapToPRT(reader);
            //}
            //return null;

            PasswordResetToken? token = context.PasswordResetTokens
                                        .Where(t => t.TokenHash == tokenHash)
                                        .Select(t => new PasswordResetToken
                                        {
                                            Id = t.Id,
                                            UserId = t.UserId,
                                            TokenHash = t.TokenHash,
                                            ExpiresAt = t.ExpiresAt,
                                            UsedAt = t.UsedAt,
                                            CreatedAt = t.CreatedAt
                                        })
                                        .FirstOrDefault();
            return token;
        }

        public void MarkAsUsed(int tokenId)
        {
            //string sql = "UPDATE PasswordResetToken SET UsedAt = GETUTCDATE() WHERE Id = @p0";
            //context.ExecuteNonQuery(sql, new object[] { tokenId });

            context.PasswordResetTokens
                .Where(t => t.Id == tokenId)
                .ExecuteUpdate(s => s.SetProperty(t => t.UsedAt, DateTime.UtcNow));
        }

        //private PasswordResetToken MapToPRT(IDataReader reader)
        //{
        //    return new PasswordResetToken
        //    {
        //        Id = reader.GetInt32(0),
        //        UserId = reader.GetInt32(1),
        //        TokenHash = reader.GetString(2),
        //        ExpiresAt = reader.GetDateTime(3),
        //        UsedAt = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
        //        CreatedAt = reader.GetDateTime(5)
        //    };
        //}
    }
}
