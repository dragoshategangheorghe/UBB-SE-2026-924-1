using BankApp.Models.Entities;
namespace BankApp.Server.DataAccess.Interfaces
{
    public interface IOAuthLinkDAO
    {
        OAuthLink? FindByProvider(string provider, string providerUserId);
        List<OAuthLink> FindByUserId(int userId);
        bool Create(int userId, string provider, string providerUserId, string? providerEmail);
        void Delete(int id);
    }
}