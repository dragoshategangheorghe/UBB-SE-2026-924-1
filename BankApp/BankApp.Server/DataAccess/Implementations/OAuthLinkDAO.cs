using BankApp.Models.Entities;
using BankApp.Server.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankApp.Server.DataAccess.Implementations
{
    public class OAuthLinkDAO : IOAuthLinkDAO
    {
        private readonly AppDbContext _dbContext;

        public OAuthLinkDAO(AppDbContext context)
        {
            this._dbContext = context;
        }

        public bool Create(int userId, string provider, string providerUserId, string? providerEmail)
        {
            var user = _dbContext.Users.Local.FirstOrDefault(u => u.Id == userId) ?? _dbContext.Users.Find(userId) ?? new User { Id = userId };
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

        public void Delete(int id)
        {
            var entity = _dbContext.OAuthLinks.FirstOrDefault(x => x.Id == id);

            if (entity == null)
                return;

            _dbContext.OAuthLinks.Remove(entity);
            _dbContext.SaveChanges();

        }

        public OAuthLink? FindByProvider(string provider, string providerUserId)
        {
            return _dbContext.OAuthLinks
                    .Include(x => x.User)
                    .FirstOrDefault(x => x.Provider == provider && x.ProviderUserId == providerUserId);
        }

        public List<OAuthLink> FindByUserId(int userId)
        {
            return _dbContext.OAuthLinks
                   .Include(x => x.User)
                   .Where(x => x.User.Id == userId)
                   .ToList();
        }
    }
}
