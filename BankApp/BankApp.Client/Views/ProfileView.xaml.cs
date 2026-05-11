using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using BankApp.Client.Services.Interfaces;
using BankApp.Client.Utilities;
using BankApp.Client.ViewModels;
using BankApp.Models.Entities;
using BankApp.Models.Enums;
using BankApp.Models.Extensions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

namespace BankApp.Client.Views
{
    public sealed partial class ProfileView : Page, IAppObserver<ProfileState>
    {
        private ProfileViewModel _viewModel;
        private readonly NotificationPreferencesViewModel _notificationPreferencesViewModel;

        private string _verifiedPassword = string.Empty;
        private string _pending2FAType = string.Empty;
        private bool _isChangingPasswordFlow = false;
        private bool _is2FAFlow = false;
        private bool _isPopulating = false;
        private bool _isUpdatingToggle = false;

        public ProfileView()
        {
            this.InitializeComponent();

            _viewModel = new ProfileViewModel(App.ProfileService);
            _viewModel.State.AddObserver(this);
            _notificationPreferencesViewModel = new NotificationPreferencesViewModel(App.NotificationClientService);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            ShowLoading(true);

            await _viewModel.LoadProfile();
            await _notificationPreferencesViewModel.LoadAsync();

            ShowLoading(false);

            if (_viewModel.ProfileInfo != null)
            {
                PopulateUI();
            }

            SetEditingEnabled(false);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            _viewModel?.State.RemoveObserver(this);
        }

        private void PopulateUI()
        {
            var user = _viewModel.ProfileInfo;

            if (user == null)
            {
                return;
            }

            ProfileCardName.Text = user.FullName ?? string.Empty;
            ProfileCardEmail.Text = user.Email ?? string.Empty;
            ProfileCardPhone.Text = user.PhoneNumber ?? string.Empty;
            ProfileCardAddress.Text = user.Address ?? string.Empty;

            FullNameBox.Text = user.FullName ?? string.Empty;
            EmailBox.Text = user.Email ?? string.Empty;

            PhoneBox.Text = user.PhoneNumber ?? string.Empty;
            AddressBox.Text = user.Address ?? string.Empty;

            TwoFactorPhoneDisplay.Text = user.PhoneNumber ?? string.Empty;
            TwoFactorEmailDisplay.Text = user.Email ?? string.Empty;

            _isPopulating = true;
            TwoFactorToggle.IsOn = user.Is2FAEnabled;
            _isPopulating = false;

            PopulateOAuthLinks(_viewModel.OAuthLinks);
            PopulateNotificationPreferences(_notificationPreferencesViewModel.Preferences);
            Update2FAVisuals();
        }

        private void SetEditingEnabled(bool enabled)
        {
            PhoneBox.IsEnabled = enabled;
            AddressBox.IsEnabled = enabled;
            SaveButton.IsEnabled = enabled;

            PhoneBox.IsReadOnly = !enabled;
            AddressBox.IsReadOnly = !enabled;

            PhoneBox.Opacity = enabled ? 1.0 : 0.6;
            AddressBox.Opacity = enabled ? 1.0 : 0.6;

            if (enabled)
            {
                PhoneBox.Focus(FocusState.Programmatic);
                AddressBox.Focus(FocusState.Programmatic);
            }
        }

        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            _isChangingPasswordFlow = false; // Just editing info
            _is2FAFlow = false;
            VerifyCurrentPasswordBox.Password = string.Empty;
            VerifyErrorInfoBar.IsOpen = false;
            await VerifyPasswordDialog.ShowAsync();
        }

        private async void VerifyPasswordDialog_PrimaryButtonClick(
            ContentDialog sender,
            ContentDialogButtonClickEventArgs args)
        {
            var deferral = args.GetDeferral();

            try
            {
                if (string.IsNullOrWhiteSpace(VerifyCurrentPasswordBox.Password))
                {
                    VerifyErrorInfoBar.Message = "Enter your password.";
                    VerifyErrorInfoBar.IsOpen = true;
                    args.Cancel = true;
                    return;
                }

                bool verified = await _viewModel.VerifyPassword(VerifyCurrentPasswordBox.Password);

                if (!verified)
                {
                    VerifyErrorInfoBar.Message = "Incorrect password.";
                    VerifyErrorInfoBar.IsOpen = true;
                    args.Cancel = true;
                    return;
                }

                _verifiedPassword = VerifyCurrentPasswordBox.Password;
                VerifyErrorInfoBar.IsOpen = false;

                if (_isChangingPasswordFlow)
                {
                    DispatcherQueue.TryEnqueue(async () =>
                    {
                        NewPasswordBox.Password = string.Empty;
                        ConfirmPasswordBox.Password = string.Empty;
                        NewPasswordErrorInfoBar.IsOpen = false;
                        await NewPasswordDialog.ShowAsync();
                    });
                }
                else if (!_is2FAFlow)
                {
                    SetEditingEnabled(true);
                    ShowSuccess("You can now edit your profile.");
                }
            }
            catch (Exception ex)
            {
                VerifyErrorInfoBar.Message = $"Verification failed: {ex.Message}";
                VerifyErrorInfoBar.IsOpen = true;
                args.Cancel = true;
            }
            finally
            {
                deferral.Complete();
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            ShowLoading(true);

            bool success = await _viewModel.UpdatePersonalInfo(
                PhoneBox.Text,
                AddressBox.Text,
                _verifiedPassword);

            ShowLoading(false);

            if (success)
            {
                ProfileCardPhone.Text = PhoneBox.Text.Trim();
                ProfileCardAddress.Text = AddressBox.Text.Trim();

                _verifiedPassword = string.Empty;
                SetEditingEnabled(false);

                ShowSuccess("Profile updated successfully.");
            }
            else
            {
                ShowError("Failed to update profile.");
            }
        }

        private async void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            _isChangingPasswordFlow = true; // Password change flow
            _is2FAFlow = false;
            VerifyCurrentPasswordBox.Password = string.Empty;
            VerifyErrorInfoBar.IsOpen = false;
            await VerifyPasswordDialog.ShowAsync();
        }

        private async void NewPasswordDialog_PrimaryButtonClick(
            ContentDialog sender,
            ContentDialogButtonClickEventArgs args)
        {
            var deferral = args.GetDeferral();

            try
            {
                string newPwd = NewPasswordBox.Password;
                string confirmPwd = ConfirmPasswordBox.Password;

                if (newPwd.Length < 8)
                {
                    NewPasswordErrorInfoBar.Message = "Minimum 8 characters required.";
                    NewPasswordErrorInfoBar.IsOpen = true;
                    args.Cancel = true;
                    return;
                }

                if (newPwd != confirmPwd)
                {
                    NewPasswordErrorInfoBar.Message = "Passwords do not match.";
                    NewPasswordErrorInfoBar.IsOpen = true;
                    args.Cancel = true;
                    return;
                }

                bool success = await _viewModel.ChangePassword(_verifiedPassword, newPwd);

                if (success)
                {
                    _verifiedPassword = string.Empty;
                    NewPasswordErrorInfoBar.IsOpen = false;
                    ShowSuccess("Your password has been changed successfully.");
                }
                else
                {
                    NewPasswordErrorInfoBar.Message = "The password change was rejected.";
                    NewPasswordErrorInfoBar.IsOpen = true;
                    args.Cancel = true;
                }
            }
            catch (Exception ex)
            {
                NewPasswordErrorInfoBar.Message = $"Password change failed: {ex.Message}";
                NewPasswordErrorInfoBar.IsOpen = true;
                args.Cancel = true;
            }
            finally
            {
                deferral.Complete();
            }
        }

        private async void Handle2FAAction_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn)
            {
                return;
            }

            _pending2FAType = btn.Tag as string ?? string.Empty;

            if (string.Equals(btn.Content?.ToString(), "Remove", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _is2FAFlow = true;
            VerifyCurrentPasswordBox.Password = string.Empty;
            await VerifyPasswordDialog.ShowAsync();
        }

        private async void SaveTwoFactorSettings_Click(object sender, RoutedEventArgs e)
        {
            // intentionally left blank; security settings are controlled above
            await Task.CompletedTask;
        }

        private async void TwoFactorToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (_isPopulating)
            {
                return;
            }

            bool success;

            if (TwoFactorToggle.IsOn)
            {
                success = await _viewModel.EnableTwoFactor(TwoFactorMethod.Email);
            }
            else
            {
                success = await _viewModel.DisableTwoFactor();
            }

            if (!success)
            {
                _isPopulating = true;
                TwoFactorToggle.IsOn = !TwoFactorToggle.IsOn;
                _isPopulating = false;
                ShowError("Failed to update 2FA settings");
            }
        }

        private async void TwoFactorEmailToggle_Toggled(object sender, RoutedEventArgs e)
        {
            await Task.CompletedTask;
        }

        private async void RemoveConnectedAccount_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is OAuthLink link)
            {
                bool success = await _viewModel.UnlinkOAuth(link.Provider);

                if (success)
                {
                    PopulateOAuthLinks(_viewModel.OAuthLinks);
                }
                else
                {
                    ShowError("Failed to remove account.");
                }
            }
        }

        private void ManageDevicesButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private async void NotificationToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (_isPopulating)
            {
                return;
            }

            if (sender is not ToggleSwitch toggle || toggle.Tag is not NotificationPreferenceItemViewModel pref)
            {
                return;
            }

            _isUpdatingToggle = true;

            try
            {
                bool success = await _notificationPreferencesViewModel.UpdatePreferenceAsync(
                    pref,
                    NotificationChannel.Email,
                    toggle.IsOn);

                if (!success)
                {
                    _isPopulating = true;
                    toggle.IsOn = !toggle.IsOn;
                    _isPopulating = false;
                    ShowError("Failed to save notification preferences.");
                }
            }
            catch (Exception ex)
            {
                _isPopulating = true;
                toggle.IsOn = !toggle.IsOn;
                _isPopulating = false;
                ShowError($"Failed to save notification preferences: {ex.Message}");
            }
            finally
            {
                _isUpdatingToggle = false;
            }
        }

        private void DashboardNavButton_Click(object sender, RoutedEventArgs e)
        {
            App.NavigationService.NavigateTo<DashboardView>();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            App.NavigationService.NavigateTo<LoginView>();
        }

        private void Update2FAVisuals()
        {
            var user = _viewModel.ProfileInfo;

            if (user == null)
            {
                return;
            }

            // Check Phone Status
            if (string.IsNullOrEmpty(user.PhoneNumber))
            {
                TwoFactorPhoneDisplay.Text = "No phone number set";
                ConfigureActionButton(ActionPhoneBtn, PhoneStatusBadge, PhoneStatusText, "Add", "#F1F5F9", "#64748B", "Disabled");
            }
            else
            {
                TwoFactorPhoneDisplay.Text = user.PhoneNumber;
                ConfigureActionButton(ActionPhoneBtn, PhoneStatusBadge, PhoneStatusText, "Verify", "#FFF7ED", "#C2410C", "Unverified");
            }
        }

        private void ConfigureActionButton(Button btn, Border badge, TextBlock statusTxt, string action, string badgeBg, string textCol, string status)
        {
            btn.Content = action;
            statusTxt.Text = status;
        }

        private void ShowLoading(bool visible)
        {
            LoadingPanel.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            LoadingRing.IsActive = visible;
            ErrorInfoBar.IsOpen = false;
            SuccessInfoBar.IsOpen = false;
        }

        private void ShowError(string message)
        {
            ErrorInfoBar.Message = message;
            ErrorInfoBar.IsOpen = true;
            SuccessInfoBar.IsOpen = false;
        }

        private void ShowSuccess(string message)
        {
            SuccessInfoBar.Message = message;
            SuccessInfoBar.IsOpen = true;
            ErrorInfoBar.IsOpen = false;
        }

        public void Update(ProfileState state)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                if (_isUpdatingToggle)
                {
                    if (state == ProfileState.Error)
                    {
                        ShowError("Failed to save notification preferences.");
                    }

                    return;
                }

                switch (state)
                {
                    case ProfileState.Loading:
                        ShowLoading(true);
                        break;

                    case ProfileState.UpdateSuccess:
                        ShowLoading(false);
                        PopulateUI();
                        break;

                    case ProfileState.Error:
                        ShowLoading(false);
                        ShowError("Operation failed.");
                        break;
                }
            });
        }

        private void TabPersonalBtn_Click(object sender, RoutedEventArgs e)
        {
            PanelPersonal.Visibility = Visibility.Visible;
            PanelSecurity.Visibility = Visibility.Collapsed;
            PanelNotifications.Visibility = Visibility.Collapsed;

            TabPersonalBtn.Style = (Style)Resources["TabButtonActiveStyle"];
            TabSecurityBtn.Style = (Style)Resources["TabButtonStyle"];
            TabNotificationsBtn.Style = (Style)Resources["TabButtonStyle"];
        }

        private void TabSecurityBtn_Click(object sender, RoutedEventArgs e)
        {
            PanelPersonal.Visibility = Visibility.Collapsed;
            PanelSecurity.Visibility = Visibility.Visible;
            PanelNotifications.Visibility = Visibility.Collapsed;

            TabPersonalBtn.Style = (Style)Resources["TabButtonStyle"];
            TabSecurityBtn.Style = (Style)Resources["TabButtonActiveStyle"];
            TabNotificationsBtn.Style = (Style)Resources["TabButtonStyle"];
        }

        private void TabNotificationsBtn_Click(object sender, RoutedEventArgs e)
        {
            PanelPersonal.Visibility = Visibility.Collapsed;
            PanelSecurity.Visibility = Visibility.Collapsed;
            PanelNotifications.Visibility = Visibility.Visible;

            TabPersonalBtn.Style = (Style)Resources["TabButtonStyle"];
            TabSecurityBtn.Style = (Style)Resources["TabButtonStyle"];
            TabNotificationsBtn.Style = (Style)Resources["TabButtonActiveStyle"];
        }

        private void PopulateOAuthLinks(List<OAuthLink> links)
        {
            OAuthLinksPanel.Children.Clear();

            if (links == null)
            {
                return;
            }

            foreach (var link in links)
            {
                var btn = new Button
                {
                    Content = link.ProviderEmail ?? link.Provider,
                    Tag = link
                };

                btn.Click += RemoveConnectedAccount_Click;
                OAuthLinksPanel.Children.Add(btn);
            }
        }

        private void PopulateNotificationPreferences(IEnumerable<NotificationPreferenceItemViewModel> prefs)
        {
            _isPopulating = true;

            NotificationPreferencesPanel.Children.Clear();

            if (prefs == null)
            {
                _isPopulating = false;
                return;
            }

            foreach (var pref in prefs)
            {
                var row = new Grid
                {
                    Margin = new Thickness(0, 6, 0, 6)
                };

                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var text = new TextBlock
                {
                    Text = pref.DisplayName,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 13,
                    Foreground = (Brush)this.Resources["TextPrimary"]
                };

                var emailToggle = BuildNotificationToggle(pref, "Email", pref.EmailEnabled, 1);
                var smsToggle = BuildNotificationToggle(pref, "SMS", pref.SmsEnabled, 2);
                var pushToggle = BuildNotificationToggle(pref, "Push", pref.PushEnabled, 3);

                Grid.SetColumn(text, 0);

                row.Children.Add(text);
                row.Children.Add(emailToggle);
                row.Children.Add(smsToggle);
                row.Children.Add(pushToggle);

                NotificationPreferencesPanel.Children.Add(row);
            }

            _isPopulating = false;
        }

        private ToggleSwitch BuildNotificationToggle(NotificationPreferenceItemViewModel pref, string label, bool isOn, int column)
        {
            var toggle = new ToggleSwitch
            {
                IsOn = isOn,
                Tag = pref,
                OnContent = label,
                OffContent = label,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0)
            };

            toggle.Toggled += async (_, __) =>
            {
                if (_isPopulating)
                {
                    return;
                }

                NotificationChannel channel = label switch
                {
                    "SMS" => NotificationChannel.Sms,
                    "Push" => NotificationChannel.Push,
                    _ => NotificationChannel.Email
                };

                await _notificationPreferencesViewModel.UpdatePreferenceAsync(pref, channel, toggle.IsOn);
            };

            Grid.SetColumn(toggle, column);
            return toggle;
        }
    }
}
