using BankApp.Models.Entities;

namespace BankApp.Server.DataAccess.Interfaces
{
    public interface IUserCardPreferenceDAO
    {
        UserCardPreference? FindByUserId(int userId);
        bool Upsert(int userId, string sortOption);
    }
}
