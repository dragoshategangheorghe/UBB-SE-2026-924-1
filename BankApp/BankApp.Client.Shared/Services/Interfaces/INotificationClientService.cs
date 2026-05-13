using System.Collections.Generic;
using System.Threading.Tasks;
using BankApp.Models.Enums;

namespace BankApp.Client.Services.Interfaces
{
    public interface INotificationClientService
    {
        Task<IReadOnlyList<NotificationPreferenceItem>> GetPreferencesAsync();
        Task<bool> SetPreferenceAsync(NotificationType type, bool emailEnabled, bool smsEnabled, bool pushEnabled);
        Task<bool> SetChannelAsync(NotificationType type, NotificationChannel channel, bool isEnabled);
        Task<bool> ResetAsync();
    }

    public sealed record NotificationPreferenceItem
    {
        public NotificationPreferenceItem(NotificationType type, bool emailEnabled, bool smsEnabled, bool pushEnabled)
        {
            Type = type;
            EmailEnabled = emailEnabled;
            SmsEnabled = smsEnabled;
            PushEnabled = pushEnabled;
        }

        public NotificationType Type { get; }
        public bool EmailEnabled { get; }
        public bool SmsEnabled { get; }
        public bool PushEnabled { get; }
    }

    public enum NotificationChannel
    {
        Email,
        Sms,
        Push
    }
}
