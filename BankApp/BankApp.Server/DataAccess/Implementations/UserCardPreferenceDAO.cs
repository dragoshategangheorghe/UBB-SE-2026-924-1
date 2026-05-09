using BankApp.Models.Entities;
using BankApp.Server.DataAccess.Interfaces;

namespace BankApp.Server.DataAccess.Implementations
{
    public class UserCardPreferenceDAO : IUserCardPreferenceDAO
    {
        private readonly AppDbContext dbContext;

        public UserCardPreferenceDAO(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public UserCardPreference? FindByUserId(int userId)
        {
            UserCardPreference? preference = dbContext.UserCardPreferences.FirstOrDefault(p => p.UserId == userId);
            return preference;
        }

        public bool Upsert(int userId, string sortOption)
        {
            var existing = dbContext.UserCardPreferences
                            .FirstOrDefault(p => p.UserId == userId);

            if (existing != null)
            {
                existing.SortOption = sortOption;
                existing.UpdatedAt = DateTime.UtcNow;

                dbContext.SaveChanges();
                return true;
            }
            else
            {
                dbContext.UserCardPreferences.Add(new UserCardPreference
                {
                    UserId = userId,
                    SortOption = sortOption,
                    UpdatedAt = DateTime.UtcNow
                });

                return dbContext.SaveChanges() > 0;
            }
        }
    }
}
