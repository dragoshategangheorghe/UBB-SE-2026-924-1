using BankApp.Models.Enums;

namespace BankApp.Models.Entities
{
    public class NotificationPreference
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public NotificationType Category { get; set; }
        // public string Category { get; set; } = string.Empty;
        public bool PushEnabled { get; set; } = true;
        public bool EmailEnabled { get; set; } = true;
        public bool SmsEnabled { get; set; }
        public decimal? MinAmountThreshold { get; set; }

        public override bool Equals(object? obj)
        {
            NotificationPreference other = obj as NotificationPreference;
            return other != null &&
                   Id == other.Id &&
                   UserId == other.UserId &&
                   Category == other.Category &&
                   PushEnabled == other.PushEnabled &&
                   EmailEnabled == other.EmailEnabled &&
                   SmsEnabled == other.SmsEnabled &&
                   MinAmountThreshold == other.MinAmountThreshold;
        }
    }
}