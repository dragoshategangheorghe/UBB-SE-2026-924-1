using BankApp.Models.Entities;
using BankApp.Server.DataAccess.Interfaces;
using System.Data;

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
            //string sql = "INSERT INTO OAuthLink (UserId, Provider, ProviderUserId, ProviderEmail) VALUES (@p0, @p1, @p2, @p3)";
            //int rowsAffected = context.ExecuteNonQuery(sql, new object[] { userId, provider, providerUserId, (object?)providerEmail ?? DBNull.Value });
            //return rowsAffected > 0;

            var link = new OAuthLink
            {
                UserId = userId,
                Provider = provider,
                ProviderUserId = providerUserId,
                ProviderEmail = providerEmail
            };

            context.OAuthLinks.Add(link);
            return context.SaveChanges() > 0;
        }

        public void Delete(int id)
        {
            //string sql = "DELETE FROM OAuthLink WHERE Id = @p0";
            //context.ExecuteNonQuery(sql, new object[] { id });
            var entity = context.OAuthLinks.FirstOrDefault(x => x.Id == id);

            if (entity == null)
                return;

            context.OAuthLinks.Remove(entity);
            context.SaveChanges();

        }

        public OAuthLink? FindByProvider(string provider, string providerUserId)
        {
            //string sql = "SELECT Id, UserId, Provider, ProviderUserId, ProviderEmail FROM OAuthLink WHERE Provider = @p0 AND ProviderUserId = @p1";
            //using IDataReader reader = context.ExecuteQuery(sql, new object[] { provider, providerUserId });

            //if (reader.Read())
            //{
            //    return MapToOAuthLink(reader);
            //}
            //return null;

            return context.OAuthLinks
                    .FirstOrDefault(x => x.Provider == provider && x.ProviderUserId == providerUserId);
        }

        public List<OAuthLink> FindByUserId(int userId)
        {
            //string sql = "SELECT Id, UserId, Provider, ProviderUserId, ProviderEmail FROM OAuthLink WHERE UserId = @p0";
            //List<OAuthLink> links = new List<OAuthLink>();
            //using IDataReader reader = context.ExecuteQuery(sql, new object[] { userId });

            //while (reader.Read())
            //{
            //    links.Add(MapToOAuthLink(reader));
            //}
            //return links;

            List<OAuthLink> links = context.OAuthLinks
                                           .Where(x => x.UserId == userId)
                                           .ToList();
            return links;
        }

        //private OAuthLink MapToOAuthLink(IDataReader reader)
        //{
        //    return new OAuthLink
        //    {
        //        Id = reader.GetInt32(0),
        //        UserId = reader.GetInt32(1),
        //        Provider = reader.GetString(2),
        //        ProviderUserId = reader.GetString(3),
        //        ProviderEmail = reader.IsDBNull(4) ? null : reader.GetString(4)
        //    };
        //}
    }
}
