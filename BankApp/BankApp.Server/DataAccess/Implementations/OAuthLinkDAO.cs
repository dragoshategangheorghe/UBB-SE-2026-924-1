using BankApp.Models.Entities;
using BankApp.Server.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankApp.Server.DataAccess.Implementations
{
    public class OAuthLinkDAO : IOAuthLinkDAO
    {
        private readonly AppDbContext _dbContext;
        public OAuthLinkDAO(AppDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public bool Create(int userId, string provider, string providerUserId, string? providerEmail)
        {
            var user = _dbContext.Users.Local.FirstOrDefault(user => user.Id == userId) ?? _dbContext.Users.Find(userId) ?? new User { Id = userId };
            if (_dbContext.Entry(user).State == EntityState.Detached)
            {
                _dbContext.Attach(user);
            }

            var link = new OAuthLink
            {
                User = user,
                Provider = provider,
                ProviderUserId = providerUserId,
                ProviderEmail = providerEmail
            };

            _dbContext.OAuthLinks.Add(link);
            return _dbContext.SaveChanges() > 0;
        }

        public void Delete(int oAuthLinkId)
        {
            var oAuthLink = _dbContext.OAuthLinks.FirstOrDefault(oAuthLink => oAuthLink.Id == oAuthLinkId);

            if (oAuthLink == null)
            {
                return;
            }

            _dbContext.OAuthLinks.Remove(oAuthLink);
            _dbContext.SaveChanges();
        }

        public OAuthLink? FindByProvider(string provider, string providerUserId)
        {
            return _dbContext.OAuthLinks
                    .Include(oAuthLink => oAuthLink.User)
                    .FirstOrDefault(oAuthLink => oAuthLink.Provider == provider && oAuthLink.ProviderUserId == providerUserId);
        }

        public List<OAuthLink> FindByUserId(int userId)
        {
            return _dbContext.OAuthLinks
                   .Include(oAuthLink => oAuthLink.User)
                   .Where(oAuthLink => oAuthLink.User.Id == userId)
                   .ToList();
        }
    }
}
