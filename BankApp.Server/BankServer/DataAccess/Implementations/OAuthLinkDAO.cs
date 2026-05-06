using BankApp.Models.Entities;
using BankApp.Server.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankApp.Server.DataAccess.Implementations
{
    public class OAuthLinkDAO : IOAuthLinkDAO
    {
        private readonly AppDbContext context;
        public OAuthLinkDAO(AppDbContext context)
        {
            this.context = context;
        }

        public bool Create(int userId, string provider, string providerUserId, string? providerEmail)
        {
            var user = context.Users.Local.FirstOrDefault(u => u.Id == userId) ?? context.Users.Find(userId) ?? new User { Id = userId };
            if (context.Entry(user).State == EntityState.Detached)
            {
                context.Attach(user);
            }

            var link = new OAuthLink
            {
                User = user,
                Provider = provider,
                ProviderUserId = providerUserId,
                ProviderEmail = providerEmail
            };

            context.OAuthLinks.Add(link);
            return context.SaveChanges() > 0;
        }

        public void Delete(int id)
        {
            var entity = context.OAuthLinks.FirstOrDefault(x => x.Id == id);

            if (entity == null)
                return;

            context.OAuthLinks.Remove(entity);
            context.SaveChanges();

        }

        public OAuthLink? FindByProvider(string provider, string providerUserId)
        {
            return context.OAuthLinks
                    .Include(x => x.User)
                    .FirstOrDefault(x => x.Provider == provider && x.ProviderUserId == providerUserId);
        }

        public List<OAuthLink> FindByUserId(int userId)
        {
            return context.OAuthLinks
                   .Include(x => x.User)
                   .Where(x => x.User.Id == userId)
                   .ToList();
        }
    }
}
