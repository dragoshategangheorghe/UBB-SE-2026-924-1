using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using BankApp.Client.Services.Interfaces;
using BankApp.Models.Enums;
using Windows.Storage;

namespace BankApp.Client.Services.Implementations
{
    public sealed class NotificationClientService : INotificationClientService
    {
        private const string StorageKey = "bankapp.notification.preferences.v1";
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        private readonly Dictionary<NotificationType, NotificationPreferenceItem> _cache;

        public NotificationClientService()
        {
            _cache = LoadFromStorage();
        }

        public Task<IReadOnlyList<NotificationPreferenceItem>> GetPreferencesAsync()
        {
            IReadOnlyList<NotificationPreferenceItem> snapshot = _cache.Values
                .OrderBy(p => p.Type)
                .Select(p => p with { })
                .ToList();

            return Task.FromResult(snapshot);
        }

        public Task<bool> SetPreferenceAsync(NotificationType type, bool emailEnabled, bool smsEnabled, bool pushEnabled)
        {
            _cache[type] = new NotificationPreferenceItem(type, emailEnabled, smsEnabled, pushEnabled);
            return PersistAsync();
        }

        public Task<bool> SetChannelAsync(NotificationType type, NotificationChannel channel, bool isEnabled)
        {
            if (!_cache.TryGetValue(type, out NotificationPreferenceItem? current))
            {
                current = new NotificationPreferenceItem(type, true, false, true);
            }

            current = channel switch
            {
                NotificationChannel.Email => new NotificationPreferenceItem(type, isEnabled, current.SmsEnabled, current.PushEnabled),
                NotificationChannel.Sms => new NotificationPreferenceItem(type, current.EmailEnabled, isEnabled, current.PushEnabled),
                NotificationChannel.Push => new NotificationPreferenceItem(type, current.EmailEnabled, current.SmsEnabled, isEnabled),
                _ => current
            };

            _cache[type] = current;
            return PersistAsync();
        }

        public Task<bool> ResetAsync()
        {
            _cache.Clear();
            SeedDefaults();
            return PersistAsync();
        }

        private Dictionary<NotificationType, NotificationPreferenceItem> LoadFromStorage()
        {
            try
            {
                if (ApplicationData.Current.LocalSettings.Values.TryGetValue(StorageKey, out object? raw) && raw is string json && !string.IsNullOrWhiteSpace(json))
                {
                    List<NotificationPreferenceItem>? items = JsonSerializer.Deserialize<List<NotificationPreferenceItem>>(json, JsonOptions);
                    if (items != null && items.Count > 0)
                    {
                        return items.ToDictionary(p => p.Type, p => p);
                    }
                }
            }
            catch
            {
                // Ignore corrupted local data and fall back to defaults.
            }

            Dictionary<NotificationType, NotificationPreferenceItem> defaults = new Dictionary<NotificationType, NotificationPreferenceItem>();
            foreach (NotificationType type in Enum.GetValues(typeof(NotificationType)))
            {
                defaults[type] = new NotificationPreferenceItem(type, true, false, true);
            }

            return defaults;
        }

        private void SeedDefaults()
        {
            foreach (NotificationType type in Enum.GetValues(typeof(NotificationType)))
            {
                _cache[type] = new NotificationPreferenceItem(type, true, false, true);
            }
        }

        private Task<bool> PersistAsync()
        {
            try
            {
                string json = JsonSerializer.Serialize(_cache.Values.OrderBy(p => p.Type).ToList(), JsonOptions);
                ApplicationData.Current.LocalSettings.Values[StorageKey] = json;
                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }
    }
}
