using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankApp.Client.Commands;
using BankApp.Client.Services.Interfaces;
using BankApp.Models.Enums;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BankApp.Client.ViewModels
{
    public sealed class NotificationPreferencesViewModel : BaseViewModel
    {
        private readonly INotificationClientService _notificationClientService;
        private readonly AsyncRelayCommand _loadCommand;
        private readonly AsyncRelayCommand _saveCommand;
        private readonly AsyncRelayCommand _resetCommand;

        private bool _isBusy;
        private bool _isError;
        private string _statusMessage = string.Empty;

        public NotificationPreferencesViewModel(INotificationClientService notificationClientService)
        {
            _notificationClientService = notificationClientService ?? throw new ArgumentNullException(nameof(notificationClientService));

            Preferences = new ObservableCollection<NotificationPreferenceItemViewModel>();
            _loadCommand = new AsyncRelayCommand(LoadAsync);
            _saveCommand = new AsyncRelayCommand(SaveAsync, () => !IsBusy && Preferences.Count > 0);
            _resetCommand = new AsyncRelayCommand(ResetAsync, () => !IsBusy);
        }

        public ObservableCollection<NotificationPreferenceItemViewModel> Preferences { get; }

        public AsyncRelayCommand LoadCommand => _loadCommand;

        public AsyncRelayCommand SaveCommand => _saveCommand;

        public AsyncRelayCommand ResetCommand => _resetCommand;

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (SetProperty(ref _isBusy, value))
                {
                    _loadCommand.RaiseCanExecuteChanged();
                    _saveCommand.RaiseCanExecuteChanged();
                    _resetCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsError
        {
            get => _isError;
            private set => SetProperty(ref _isError, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public async Task LoadAsync()
        {
            try
            {
                IsBusy = true;
                IsError = false;
                StatusMessage = string.Empty;

                IReadOnlyList<NotificationPreferenceItem> preferences = await _notificationClientService.GetPreferencesAsync();

                Preferences.Clear();

                foreach (NotificationPreferenceItem preference in preferences)
                {
                    Preferences.Add(new NotificationPreferenceItemViewModel(
                        preference.Type,
                        preference.EmailEnabled,
                        preference.SmsEnabled,
                        preference.PushEnabled));
                }

                StatusMessage = "Notification preferences loaded.";
            }
            catch (Exception ex)
            {
                IsError = true;
                StatusMessage = $"Unable to load notification preferences: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task<bool> UpdatePreferenceAsync(NotificationPreferenceItemViewModel preference, NotificationChannel channel, bool isEnabled)
        {
            if (preference == null)
            {
                return false;
            }

            try
            {
                IsBusy = true;
                IsError = false;

                switch (channel)
                {
                    case NotificationChannel.Email:
                        preference.EmailEnabled = isEnabled;
                        break;
                    case NotificationChannel.Sms:
                        preference.SmsEnabled = isEnabled;
                        break;
                    case NotificationChannel.Push:
                        preference.PushEnabled = isEnabled;
                        break;
                }

                bool saved = await _notificationClientService.SetPreferenceAsync(
                    preference.Type,
                    preference.EmailEnabled,
                    preference.SmsEnabled,
                    preference.PushEnabled);

                if (saved)
                {
                    StatusMessage = $"{preference.DisplayName} updated.";
                }
                else
                {
                    IsError = true;
                    StatusMessage = "Unable to save notification preference.";
                }

                return saved;
            }
            catch (Exception ex)
            {
                IsError = true;
                StatusMessage = $"Unable to update notification preference: {ex.Message}";
                return false;
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task<bool> SaveAsync()
        {
            try
            {
                IsBusy = true;
                IsError = false;

                bool saved = true;
                foreach (NotificationPreferenceItemViewModel preference in Preferences)
                {
                    saved &= await _notificationClientService.SetPreferenceAsync(
                        preference.Type,
                        preference.EmailEnabled,
                        preference.SmsEnabled,
                        preference.PushEnabled);
                }

                if (saved)
                {
                    StatusMessage = "Notification preferences saved locally.";
                }
                else
                {
                    IsError = true;
                    StatusMessage = "One or more notification preferences could not be saved.";
                }

                return saved;
            }
            catch (Exception ex)
            {
                IsError = true;
                StatusMessage = $"Unable to save notification preferences: {ex.Message}";
                return false;
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task<bool> ResetAsync()
        {
            try
            {
                IsBusy = true;
                IsError = false;

                if (!await _notificationClientService.ResetAsync())
                {
                    IsError = true;
                    StatusMessage = "Unable to reset notification preferences.";
                    return false;
                }

                await LoadAsync();
                return true;
            }
            catch (Exception ex)
            {
                IsError = true;
                StatusMessage = $"Unable to reset notification preferences: {ex.Message}";
                return false;
            }
            finally
            {
                IsBusy = false;
            }
        }

        public override void Dispose()
        {
        }
    }
}
