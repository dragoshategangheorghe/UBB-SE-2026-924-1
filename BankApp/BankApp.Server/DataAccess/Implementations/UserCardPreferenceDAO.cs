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
            //const string query = @"
            //    SELECT UserId, SortOption, UpdatedAt
            //    FROM UserCardPreference
            //    WHERE UserId = @p0";

            //using var reader = dbContext.ExecuteQuery(query, new object[] { userId });
            //return reader.Read() ? MapPreference(reader) : null;

            UserCardPreference? preference = dbContext.UserCardPreferences.FirstOrDefault(p => p.UserId == userId);
            return preference;
        }

        public bool Upsert(int userId, string sortOption)
        {
            //const string updateQuery = @"
            //    UPDATE UserCardPreference
            //    SET SortOption = @p1,
            //        UpdatedAt = GETUTCDATE()
            //    WHERE UserId = @p0";

            //int updatedRows = dbContext.ExecuteNonQuery(updateQuery, new object[] { userId, sortOption });
            //if (updatedRows > 0)
            //{
            //    return true;
            //}

            //const string insertQuery = @"
            //    INSERT INTO UserCardPreference (UserId, SortOption)
            //    VALUES (@p0, @p1)";

            //return dbContext.ExecuteNonQuery(insertQuery, new object[] { userId, sortOption }) > 0;

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

        //private static UserCardPreference MapPreference(IDataReader reader)
        //{
        //    return new UserCardPreference
        //    {
        //        UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
        //        SortOption = reader.GetString(reader.GetOrdinal("SortOption")),
        //        UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
        //    };
        //}
    }
}
