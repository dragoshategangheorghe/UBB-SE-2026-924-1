using System.Data;
using BankApp.Models.Entities;
using BankApp.Server.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

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
            var token = new PasswordResetToken
            {
                UserId = userId,
                TokenHash = tokenHash,
                ExpiresAt = expiresAt
            };

            context.PasswordResetTokens.Add(token);

            var rows = context.SaveChanges();

            if (rows <= 0)
            {
                throw new Exception("Failed to create password reset token.");
            }

            return token;
        }

        public void DeleteExpired()
        {
            context.PasswordResetTokens
                .Where(t => t.ExpiresAt < DateTime.UtcNow)
                .ExecuteDelete();
        }

        public PasswordResetToken? FindByToken(string tokenHash)
        {
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
            context.PasswordResetTokens
                .Where(t => t.Id == tokenId)
                .ExecuteUpdate(s => s.SetProperty(t => t.UsedAt, DateTime.UtcNow));
        }
    }
}
