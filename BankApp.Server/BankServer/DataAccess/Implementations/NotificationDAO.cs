using BankApp.Models.Entities;
using BankApp.Server.DataAccess.Interfaces;

namespace BankApp.Server.DataAccess.Implementations
{
    public class NotificationDAO : INotificationDAO
    {
        private readonly AppDbContext dbContext;
        public NotificationDAO(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public int CountUnreadByUserId(int userId)
        {
            var query = @"SELECT COUNT(*) FROM Notification WHERE UserId = @p0 and IsRead = 0";
            using var reader = dbContext.ExecuteQuery(query, new object[] { userId });
            if (reader.Read())
            {
                return reader.GetInt32(0);
            }
            return 0;
        }

        public List<Notification> FindByUserId(int userId)
        {
            var notifications = new List<Notification>();
            var query = @"SELECT * FROM Notification where UserId = @p0";
            using var reader = dbContext.ExecuteQuery(query, new object[] { userId });
            while (reader.Read())
            {
                notifications.Add(MapToNotification(reader));
            }

            return notifications;
        }

        private Notification MapToNotification(System.Data.IDataReader r)
        {
            return new Notification
            {
                Id = r.GetInt32(r.GetOrdinal("Id")),
                UserId = r.GetInt32(r.GetOrdinal("UserId")),
                Title = r.GetString(r.GetOrdinal("Title")),
                Message = r.GetString(r.GetOrdinal("Message")),
                Type = r.GetString(r.GetOrdinal("Type")),
                Channel = r.GetString(r.GetOrdinal("Channel")),
                IsRead = r.GetBoolean(r.GetOrdinal("IsRead")),
                RelatedEntityType = r.IsDBNull(r.GetOrdinal("RelatedEntityType"))
                    ? null : r.GetString(r.GetOrdinal("RelatedEntityType")),
                RelatedEntityId = r.IsDBNull(r.GetOrdinal("RelatedEntityId"))
                    ? null : r.GetInt32(r.GetOrdinal("RelatedEntityId")),
                CreatedAt = r.GetDateTime(r.GetOrdinal("CreatedAt"))
            };
        }
    }
}
