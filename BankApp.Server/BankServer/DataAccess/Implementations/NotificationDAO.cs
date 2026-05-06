using BankApp.Models.Entities;
using BankApp.Server.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankApp.Server.DataAccess.Implementations
{
    public class NotificationDAO : INotificationDAO
    {
        private readonly AppDbContext appDbContext;

        public NotificationDAO(AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
        }

        public bool Create(int userId, string title, string message, string type, string channel, string? relatedEntityType, int? relatedEntityId)
        {
            var user = appDbContext.Users.Local.FirstOrDefault(u => u.Id == userId) ?? appDbContext.Users.Find(userId) ?? new User { Id = userId };
            if (appDbContext.Entry(user).State == EntityState.Detached)
            {
                appDbContext.Attach(user);
            }

            var notification = new Notification
            {
                User = user,
                Title = title,
                Message = message,
                Type = type,
                Channel = channel,
                RelatedEntityType = relatedEntityType,
                RelatedEntityId = relatedEntityId,
                CreatedAt = DateTime.UtcNow
            };

            appDbContext.Notifications.Add(notification);
            return appDbContext.SaveChanges() > 0;
        }

        public int CountUnreadByUserId(int userId)
        {
            return appDbContext.Notifications
                    .Count(n => n.User.Id == userId && !n.IsRead);
        }

        public List<Notification> FindByUserId(int userId)
        {
            return appDbContext.Notifications
                .Include(n => n.User)
                .Where(n => n.User.Id == userId)
                .ToList();
        }
    }
}
