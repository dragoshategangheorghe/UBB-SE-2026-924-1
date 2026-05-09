using BankApp.Models.Entities;
using BankApp.Models.Enums;
using BankApp.Server.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankApp.Server.DataAccess
{
    internal class NotificationPreferenceDAO : INotificationPreferenceDAO
    {
        private AppDbContext _dbContext;

        public NotificationPreferenceDAO(AppDbContext appDbContext)
        {
            this._dbContext = appDbContext;
        }

        public bool Create(int userId, string category)
        {
            try
            {
                var user = _dbContext.Users.Local.FirstOrDefault(u => u.Id == userId) ?? _dbContext.Users.Find(userId) ?? new User { Id = userId };
                if (_dbContext.Entry(user).State == EntityState.Detached)
                {
                    _dbContext.Attach(user);
                }

                var preference = new NotificationPreference
                {
                    User = user,
                    Category = (NotificationType)Enum.Parse(typeof(NotificationType), category),
                    PushEnabled = false,
                    EmailEnabled = false,
                    SmsEnabled = false
                };

                _dbContext.NotificationPreferences.Add(preference);
                var rows = _dbContext.SaveChanges();

                return rows > 0;
            }
            catch
            {
                return false;
            }
        }

        public List<NotificationPreference> FindByUserId(int userId)
        {
            return _dbContext.NotificationPreferences
                .Include(p => p.User)
                .Where(p => p.User.Id == userId)
                .ToList();
        }

        public bool Update(int userId, List<NotificationPreference> prefs)
        {
            try
            {
                var existing = _dbContext.NotificationPreferences
                                           .Where(p => p.User.Id == userId);

                _dbContext.NotificationPreferences.RemoveRange(existing);

                var user = _dbContext.Users.Local.FirstOrDefault(u => u.Id == userId) ?? _dbContext.Users.Find(userId) ?? new User { Id = userId };
                if (_dbContext.Entry(user).State == EntityState.Detached)
                {
                    _dbContext.Attach(user);
                }

                foreach (var preference in prefs)
                {
                    _dbContext.NotificationPreferences.Add(new NotificationPreference
                    {
                        User = user,
                        Category = preference.Category,
                        PushEnabled = preference.PushEnabled,
                        EmailEnabled = preference.EmailEnabled,
                        SmsEnabled = preference.SmsEnabled,
                        MinAmountThreshold = preference.MinAmountThreshold
                    });
                }

                _dbContext.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
