using System.Data;
using BankApp.Models.Entities;
using BankApp.Server.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankApp.Server.DataAccess.Implementations
{
    public class PasswordResetTokenDAO : IPasswordResetTokenDAO
    {
        private readonly AppDbContext _dbContext;

        public PasswordResetTokenDAO(AppDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public PasswordResetToken Create(int userId, string tokenHash, DateTime expiresAt)
        {
            var token = new PasswordResetToken
            {
                UserId = userId,
                TokenHash = tokenHash,
                ExpiresAt = expiresAt
            };

            _dbContext.PasswordResetTokens.Add(token);

            var rows = _dbContext.SaveChanges();

            if (rows <= 0)
            {
                throw new Exception("Failed to create password reset token.");
            }

            return token;
        }

        public void DeleteExpired()
        {
            _dbContext.PasswordResetTokens
                .Where(passwordResetToken => passwordResetToken.ExpiresAt < DateTime.UtcNow)
                .ExecuteDelete();
        }

        public PasswordResetToken? FindByToken(string tokenHash)
        {
            PasswordResetToken? token = _dbContext.PasswordResetTokens
                                        .Where(passwordResetToken => passwordResetToken.TokenHash == tokenHash)
                                        .Select(passwordResetToken => new PasswordResetToken
                                        {
                                            Id = passwordResetToken.Id,
                                            UserId = passwordResetToken.UserId,
                                            TokenHash = passwordResetToken.TokenHash,
                                            ExpiresAt = passwordResetToken.ExpiresAt,
                                            UsedAt = passwordResetToken.UsedAt,
                                            CreatedAt = passwordResetToken.CreatedAt
                                        })
                                        .FirstOrDefault();
            return token;
        }

        public void MarkAsUsed(int tokenId)
        {
            _dbContext.PasswordResetTokens
                .Where(passwordResetToken => passwordResetToken.Id == tokenId)
                .ExecuteUpdate(setters => setters.SetProperty(t => t.UsedAt, DateTime.UtcNow));
        }
    }
}
