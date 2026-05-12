using BankApp.Models.Entities;
using BankApp.Models.Extensions;
using BankApp.Server.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankApp.Server.DataAccess
{
    internal class NotificationPreferenceDAO : INotificationPreferenceDAO
    {
        private AppDbContext _dbContext;

        public NotificationPreferenceDAO(AppDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public bool Create(int userId, string category)
        {
            try
            {
                var user = _dbContext.Users.Local.FirstOrDefault(user => user.Id == userId) ?? _dbContext.Users.Find(userId) ?? new User { Id = userId };
                if (_dbContext.Entry(user).State == EntityState.Detached)
                {
                    _dbContext.Attach(user);
                }

                var preference = new NotificationPreference
                {
                    User = user,
                    Category = NotificationTypeExtensions.FromString(category),
                    PushEnabled = false,
                    EmailEnabled = false,
                    SmsEnabled = false
                };

                _dbContext.NotificationPreferences.Add(preference);
                var rows = _dbContext.SaveChanges();

                return rows > 0;
            }
            catch (Exception exception) when (
            exception is DbUpdateConcurrencyException
            || exception is DbUpdateException)
            {
                return false;
            }
        }

        public List<NotificationPreference> FindByUserId(int userId)
        {
            return _dbContext.NotificationPreferences
                .Include(notificationPreference => notificationPreference.User)
                .Where(notificationPreference => notificationPreference.User.Id == userId)
                .ToList();
        }

        public bool Update(int userId, List<NotificationPreference> notificationPreferences)
        {
            try
            {
                var existing = _dbContext.NotificationPreferences
                    .Where(notificationPreference => notificationPreference.User.Id == userId)
                    .ToList();

                _dbContext.NotificationPreferences.RemoveRange(existing);

                var user = _dbContext.Users.Local.FirstOrDefault(user => user.Id == userId) ?? _dbContext.Users.Find(userId) ?? new User { Id = userId };
                if (_dbContext.Entry(user).State == EntityState.Detached)
                {
                    _dbContext.Attach(user);
                }

                foreach (var preference in notificationPreferences)
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
            catch (Exception exception) when (
            exception is DbUpdateException
            || exception is DbUpdateConcurrencyException)
            {
                return false;
            }
        }
    }
}
