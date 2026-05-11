using BankApp.Models.Enums;
using BankApp.Models.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BankApp.Client.ViewModels
{
    public sealed class NotificationPreferenceItemViewModel : ObservableObject
    {
        private bool _emailEnabled;
        private bool _smsEnabled;
        private bool _pushEnabled;

        public NotificationPreferenceItemViewModel(NotificationType type, bool emailEnabled, bool smsEnabled, bool pushEnabled)
        {
            Type = type;
            DisplayName = type.ToDisplayName();
            _emailEnabled = emailEnabled;
            _smsEnabled = smsEnabled;
            _pushEnabled = pushEnabled;
        }

        public NotificationType Type { get; }

        public string DisplayName { get; }

        public bool EmailEnabled
        {
            get => _emailEnabled;
            set => SetProperty(ref _emailEnabled, value);
        }

        public bool SmsEnabled
        {
            get => _smsEnabled;
            set => SetProperty(ref _smsEnabled, value);
        }

        public bool PushEnabled
        {
            get => _pushEnabled;
            set => SetProperty(ref _pushEnabled, value);
        }
    }
}
