using BankApp.Models.Entities;
using BankApp.Server.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankApp.Server.DataAccess.Implementations
{
    public class NotificationDAO : INotificationDAO
    {
        private readonly AppDbContext _dbContext;

        public NotificationDAO(AppDbContext appDbContext)
        {
            this._dbContext = appDbContext;
        }

        public bool Create(int userId, string title, string message, string type, string channel, string? relatedEntityType, int? relatedEntityId)
        {
            var user = _dbContext.Users.Local.FirstOrDefault(u => u.Id == userId) ?? _dbContext.Users.Find(userId) ?? new User { Id = userId };
            if (_dbContext.Entry(user).State == EntityState.Detached)
            {
                _dbContext.Attach(user);
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

            _dbContext.Notifications.Add(notification);
            return _dbContext.SaveChanges() > 0;
        }

        public int CountUnreadByUserId(int userId)
        {
            return _dbContext.Notifications
                    .Count(n => n.User.Id == userId && !n.IsRead);
        }

        public List<Notification> FindByUserId(int userId)
        {
            return _dbContext.Notifications
                .Include(n => n.User)
                .Where(n => n.User.Id == userId)
                .ToList();
        }
    }
}
