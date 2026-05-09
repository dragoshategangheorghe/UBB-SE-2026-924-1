using BankApp.Models.Entities;
using BankApp.Models.Extensions;
using BankApp.Server.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankApp.Server.DataAccess
{
    internal class NotificationPreferenceDAO : INotificationPreferenceDAO
    {
        private AppDbContext appDbContext;

        public NotificationPreferenceDAO(AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
        }

        public bool Create(int userId, string category)
        {
            try
            {
                var user = appDbContext.Users.Local.FirstOrDefault(u => u.Id == userId) ?? appDbContext.Users.Find(userId) ?? new User { Id = userId };
                if (appDbContext.Entry(user).State == EntityState.Detached)
                {
                    appDbContext.Attach(user);
                }

                var preference = new NotificationPreference
                {
                    User = user,
                    Category = NotificationTypeExtensions.FromString(category),
                    PushEnabled = false,
                    EmailEnabled = false,
                    SmsEnabled = false
                };

                appDbContext.NotificationPreferences.Add(preference);
                var rows = appDbContext.SaveChanges();

                return rows > 0;
            }
            catch
            {
                return false;
            }
        }

        public List<NotificationPreference> FindByUserId(int userId)
        {
            return appDbContext.NotificationPreferences
                .Include(p => p.User)
                .Where(p => p.User.Id == userId)
                .ToList();
        }

        public bool Update(int userId, List<NotificationPreference> prefs)
        {
            try
            {
                var existing = appDbContext.NotificationPreferences
                    .Where(p => p.User.Id == userId)
                    .ToList();

                appDbContext.NotificationPreferences.RemoveRange(existing);

                var user = appDbContext.Users.Local.FirstOrDefault(u => u.Id == userId) ?? appDbContext.Users.Find(userId) ?? new User { Id = userId };
                if (appDbContext.Entry(user).State == EntityState.Detached)
                {
                    appDbContext.Attach(user);
                }

                foreach (var preference in prefs)
                {
                    appDbContext.NotificationPreferences.Add(new NotificationPreference
                    {
                        User = user,
                        Category = preference.Category,
                        PushEnabled = preference.PushEnabled,
                        EmailEnabled = preference.EmailEnabled,
                        SmsEnabled = preference.SmsEnabled,
                        MinAmountThreshold = preference.MinAmountThreshold
                    });
                }

                appDbContext.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
