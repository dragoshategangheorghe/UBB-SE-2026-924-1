using BankApp.Models.Entities;
using BankApp.Server.DataAccess.Interfaces;

namespace BankApp.Server.DataAccess.Implementations
{
    public class UserCardPreferenceDAO : IUserCardPreferenceDAO
    {
        private readonly AppDbContext _dbContext;

        public UserCardPreferenceDAO(AppDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public UserCardPreference? FindByUserId(int userId)
        {
            UserCardPreference? preference = _dbContext.UserCardPreferences.FirstOrDefault(userCardPreference => userCardPreference.UserId == userId);
            return preference;
        }

        public bool Upsert(int userId, string sortOption)
        {
            var existing = _dbContext.UserCardPreferences
                            .FirstOrDefault(userCardPreference => userCardPreference.UserId == userId);

            if (existing != null)
            {
                existing.SortOption = sortOption;
                existing.UpdatedAt = DateTime.UtcNow;

                _dbContext.SaveChanges();
                return true;
            }
            else
            {
                _dbContext.UserCardPreferences.Add(new UserCardPreference
                {
                    UserId = userId,
                    SortOption = sortOption,
                    UpdatedAt = DateTime.UtcNow
                });

                return _dbContext.SaveChanges() > 0;
            }
        }
    }
}
