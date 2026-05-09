using BankApp.Models.Entities;
using BankApp.Server.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace BankApp.Server.DataAccess.Implementations
{
    public class PasswordResetTokenDAO : IPasswordResetTokenDAO
    {
        private readonly AppDbContext _dbContext;

        public PasswordResetTokenDAO(AppDbContext context)
        {
            this._dbContext = context;
        }

        public PasswordResetToken Create(int userId, string tokenHash, DateTime expiresAt)
        {
            var token = new PasswordResetToken
            {
                Id = userId,
                TokenHash = tokenHash,
                ExpiresAt = expiresAt
            };

            _dbContext.PasswordResetTokens.Add(token);

            var rows = _dbContext.SaveChanges();

            if (rows <= 0)
                throw new Exception("Failed to create password reset token.");

            return token;
        }

        public void DeleteExpired()
        {
            _dbContext.PasswordResetTokens
                .Where(t => t.ExpiresAt < DateTime.UtcNow)
                .ExecuteDelete();
        }

        public PasswordResetToken? FindByToken(string tokenHash)
        {
            PasswordResetToken? token = _dbContext.PasswordResetTokens
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
            _dbContext.PasswordResetTokens
                .Where(t => t.Id == tokenId)
                .ExecuteUpdate(s => s.SetProperty(t => t.UsedAt, DateTime.UtcNow));
        }
    }
}
