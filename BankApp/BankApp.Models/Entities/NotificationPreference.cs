using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using BankApp.Models.Enums;

namespace BankApp.Models.Entities
{
    public class NotificationPreference
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Not serialized on API round-trip — avoids object cycles (User aggregates NotificationPreferences).
        /// </summary>
        [JsonIgnore]
        public virtual User User { get; set; } = null!;
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
                   User == other.User &&
                   Category == other.Category &&
                   PushEnabled == other.PushEnabled &&
                   EmailEnabled == other.EmailEnabled &&
                   SmsEnabled == other.SmsEnabled &&
                   MinAmountThreshold == other.MinAmountThreshold;
        }
    }
}