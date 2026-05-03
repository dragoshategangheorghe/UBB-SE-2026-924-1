using BankApp.Client.Utilities;
using BankApp.Client.ViewModels;
using BankApp.Models.Entities;
using BankApp.Models.Enums;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;
using BankApp.Models.Extensions;

namespace BankApp.Client.Views
{
    public sealed partial class ProfileView : Page, Observer<ProfileState>
    {
        private ProfileViewModel _viewModel;


        private string _verifiedPassword = string.Empty;
        private string _pending2FAType = "";
        private bool _isChangingPasswordFlow = false;
        private bool _is2FAFlow = false;
        private bool _isPopulating = false;
        private bool _isUpdatingToggle = false;


        public ProfileView()
        {
            this.InitializeComponent();

            _viewModel = new ProfileViewModel(App.ApiService);
            _viewModel.State.AddObserver(this);
        }

        // ─── Navigation ─────────────────────────────────────────

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            ShowLoading(true);

            await _viewModel.LoadProfile();

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

        // ─── UI Setup ─────────────────────────────────────────

        private void PopulateUI()
        {
            var user = _viewModel.ProfileInfo;

            ProfileCardName.Text = user.FullName ?? "";
            ProfileCardEmail.Text = user.Email ?? "";
            ProfileCardPhone.Text = user.PhoneNumber ?? "";
            ProfileCardAddress.Text = user.Address ?? "";

            FullNameBox.Text = user.FullName ?? "";
            EmailBox.Text = user.Email ?? "";

            PhoneBox.Text = user.PhoneNumber ?? "";
            AddressBox.Text = user.Address ?? "";

            TwoFactorPhoneDisplay.Text = user.PhoneNumber ?? "";
            TwoFactorEmailDisplay.Text = user.Email ?? "";

            _isPopulating = true;
            TwoFactorToggle.IsOn = user.Is2FAEnabled;
            _isPopulating = false;

            PopulateOAuthLinks(_viewModel.OAuthLinks);
            PopulateNotificationPreferences(_viewModel.NotificationPreferences);
            Update2FAVisuals();
        }

        private void SetEditingEnabled(bool enabled)
        {
            //FullNameBox.IsEnabled = enabled;
            PhoneBox.IsEnabled = enabled;
            AddressBox.IsEnabled = enabled;
            SaveButton.IsEnabled = enabled;

            
            PhoneBox.IsReadOnly = !enabled;
            AddressBox.IsReadOnly = !enabled;
            //FullNameBox.IsReadOnly = !enabled;
          
            PhoneBox.Opacity = enabled ? 1.0 : 0.6;
            AddressBox.Opacity = enabled ? 1.0 : 0.6;
           // FullNameBox.Opacity = !enabled ? 1.0 : 0.6;
            
            if (enabled)
            {
                PhoneBox.Focus(FocusState.Programmatic);
                AddressBox.Focus(FocusState.Programmatic);
               // FullNameBox.Focus(FocusState.Programmatic);
            }
        }

        // ─── PERSONAL INFO FLOW ───────────────────────────────

        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            _isChangingPasswordFlow = false; // Just editing info
            _is2FAFlow = false;
            VerifyCurrentPasswordBox.Password = "";
            VerifyErrorInfoBar.IsOpen = false;
            await VerifyPasswordDialog.ShowAsync();
        }

        private async void VerifyPasswordDialog_PrimaryButtonClick(
     ContentDialog sender,
     ContentDialogButtonClickEventArgs args)
        {
            var deferral = args.GetDeferral();

            if (string.IsNullOrWhiteSpace(VerifyCurrentPasswordBox.Password))
            {
                VerifyErrorInfoBar.Message = "Enter your password.";
                VerifyErrorInfoBar.IsOpen = true;
                args.Cancel = true;
                deferral.Complete();
                return;
            }

            bool verified = await _viewModel.VerifyPassword(VerifyCurrentPasswordBox.Password);

            if (!verified)
            {
                VerifyErrorInfoBar.Message = "Incorrect password.";
                VerifyErrorInfoBar.IsOpen = true;
                args.Cancel = true;
                deferral.Complete();
                return;
            }

            // Success logic
            _verifiedPassword = VerifyCurrentPasswordBox.Password;
            VerifyErrorInfoBar.IsOpen = false;

            // Complete the deferral so the FIRST dialog closes
            deferral.Complete();

            // Now, trigger the NEXT step based on the flow
            if (_isChangingPasswordFlow)
            {
                // We MUST use the Dispatcher to wait until the first dialog is gone
                DispatcherQueue.TryEnqueue(async () =>
                {
                    NewPasswordBox.Password = "";
                    ConfirmPasswordBox.Password = "";
                    NewPasswordErrorInfoBar.IsOpen = false;
                    await NewPasswordDialog.ShowAsync();
                });
            }
            else if (!_is2FAFlow)
            {
                // Normal profile edit flow
                SetEditingEnabled(true);
                ShowSuccess("You can now edit your profile.");
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

                _verifiedPassword = "";
                SetEditingEnabled(false);

                ShowSuccess("Profile updated successfully.");
            }
            else
            {
                ShowError("Failed to update profile.");
            }
        }

        // ─── PASSWORD CHANGE ───────────────────────────────

        private async void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            _isChangingPasswordFlow = true; // Password change flow
            _is2FAFlow = false;
            VerifyCurrentPasswordBox.Password = "";
            VerifyErrorInfoBar.IsOpen = false;
            await VerifyPasswordDialog.ShowAsync();
        }

        private async void NewPasswordDialog_PrimaryButtonClick(
     ContentDialog sender,
     ContentDialogButtonClickEventArgs args)
        {
            var deferral = args.GetDeferral();

            string newPwd = NewPasswordBox.Password;
            string confirmPwd = ConfirmPasswordBox.Password;

            // 1. Basic Validation
            if (newPwd.Length < 8)
            {
                NewPasswordErrorInfoBar.Message = "Minimum 8 characters required.";
                NewPasswordErrorInfoBar.IsOpen = true;
                args.Cancel = true;
                deferral.Complete();
                return;
            }

            if (newPwd != confirmPwd)
            {
                NewPasswordErrorInfoBar.Message = "Passwords do not match.";
                NewPasswordErrorInfoBar.IsOpen = true;
                args.Cancel = true;
                deferral.Complete();
                return;
            }

            // 2. Call ViewModel
            // Note: We use the _verifiedPassword we saved from Dialog 1 as the 'old' password
            bool success = await _viewModel.ChangePassword(_verifiedPassword, newPwd);

            if (success)
            {
                _verifiedPassword = ""; // Clear security sensitive data
                NewPasswordErrorInfoBar.IsOpen = false;

                deferral.Complete();
                ShowSuccess("Your password has been changed successfully.");
            }
            else
            {
                NewPasswordErrorInfoBar.Message = "The server rejected the change. Please check your connection.";
                NewPasswordErrorInfoBar.IsOpen = true;
                args.Cancel = true;
                deferral.Complete();
            }
        }
       
        // ─── 2FA ─────────────────────────────────────────


        private async void Handle2FAAction_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            _pending2FAType = btn.Tag.ToString(); // "Phone" or "Email"

            if (btn.Content.ToString() == "Remove")
            {
                // Logic for removal
            }
            else
            {
                // Logic for Add/Verify
                _is2FAFlow = true;
                VerifyCurrentPasswordBox.Password = "";
                await VerifyPasswordDialog.ShowAsync();
            }
        }



        private async void SaveTwoFactorSettings_Click(object sender, RoutedEventArgs e)
        {
            //bool success = await _viewModel.UpdateTwoFactorContacts(
            //    TwoFactorPhoneBox.Text.Trim(),
            //    TwoFactorEmailBox.Text.Trim());

            //if (success)
            //    ShowSuccess("2FA settings saved.");
            //else
            //    ShowError("Failed to save 2FA settings.");
        }
        private async void TwoFactorToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (_isPopulating) return;

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
            //bool success = TwoFactorEmailToggle.IsOn
            //    ? await _viewModel.EnableTwoFactor(TwoFactorMethod.Email)
            //    : await _viewModel.DisableTwoFactor(TwoFactorMethod.Email);

            //if (!success)
            //    ShowError("2FA email update failed.");
        }

        // ─── OAuth ─────────────────────────────────────────

        private async void RemoveConnectedAccount_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is OAuthLink link)
            {
                bool success = await _viewModel.UnlinkOAuth(link.Provider);

                if (success)
                    PopulateOAuthLinks(_viewModel.OAuthLinks);
                else
                    ShowError("Failed to remove account.");
            }
        }

        private void ManageDevicesButton_Click(object sender, RoutedEventArgs e)
        {
        }

        // ─── Notifications ─────────────────────────

        private async void NotificationToggle_Toggled(object sender, RoutedEventArgs e)
        {
            // 1. Ignore the event if we are just drawing the UI
            if (_isPopulating) return;

            if (sender is ToggleSwitch toggle && toggle.Tag is NotificationPreference pref)
            {
                pref.EmailEnabled = toggle.IsOn;

                // 2. Set the flag to block the UI from fully refreshing
                _isUpdatingToggle = true;

                bool success = await _viewModel.UpdateNotificationPreferences(_viewModel.NotificationPreferences);

                // 3. Clear the flag when the API call finishes
                _isUpdatingToggle = false;

                if (!success)
                {
                    // Optional: If the API fails, visually flip the switch back to its old state
                    _isPopulating = true;
                    toggle.IsOn = !toggle.IsOn;
                    pref.EmailEnabled = toggle.IsOn;
                    _isPopulating = false;
                }
                else
                {
                    toggle.IsOn = pref.EmailEnabled; // force sync (important)
                }
            }
        }
        // ─── Navigation ─────────────────────────

        private void DashboardNavButton_Click(object sender, RoutedEventArgs e)
        {
            App.NavigationService.NavigateTo<DashboardView>();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            App.NavigationService.NavigateTo<LoginView>();
        }

        // ─── Helpers ─────────────────────────

        private void Update2FAVisuals()
        {
            var user = _viewModel.ProfileInfo;

            // Check Phone Status
            if (string.IsNullOrEmpty(user.PhoneNumber))
            {
                TwoFactorPhoneDisplay.Text = "No phone number set";
                ConfigureActionButton(ActionPhoneBtn, PhoneStatusBadge, PhoneStatusText, "Add", "#F1F5F9", "#64748B", "Disabled");
            }
            else if (true/*!user.IsPhoneVerified*/)
            { // You need this property in your Model
                TwoFactorPhoneDisplay.Text = user.PhoneNumber;
                ConfigureActionButton(ActionPhoneBtn, PhoneStatusBadge, PhoneStatusText, "Verify", "#FFF7ED", "#C2410C", "Unverified");
            }
            else
            {
                TwoFactorPhoneDisplay.Text = user.PhoneNumber;
                ConfigureActionButton(ActionPhoneBtn, PhoneStatusBadge, PhoneStatusText, "Remove", "#F0FDF4", "#15803D", "Active");
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

        // ─── Observer ─────────────────────────

        public void Update(ProfileState state)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                // --- INTERCEPTOR: Block full-page reloads if we are just toggling a switch ---
                if (_isUpdatingToggle)
                {
                    if (state == ProfileState.Error)
                    {
                        ShowError("Failed to save notification preferences.");
                    }
                    // Ignore Loading and UpdateSuccess so the screen doesn't wipe and redraw!
                    return;
                }
                // ----------------------------------------------------------------------------

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

        // ─── EXISTING HELPERS (unchanged) ─────────────────────────

        // ─── TAB SWITCHING ─────────────────────────

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

            if (links == null) return;

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

        private void PopulateNotificationPreferences(List<NotificationPreference> prefs)
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

                var text = new TextBlock
                {
                    Text = NotificationTypeExtensions.ToDisplayName(pref.Category),
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 13,
                    Foreground = (Brush)this.Resources["TextPrimary"]
                };

                var toggle = new ToggleSwitch
                {
                    IsOn = pref.EmailEnabled,
                    Tag = pref,
                    VerticalAlignment = VerticalAlignment.Center
                };

                toggle.Toggled += NotificationToggle_Toggled;

                Grid.SetColumn(text, 0);
                Grid.SetColumn(toggle, 1);

                row.Children.Add(text);
                row.Children.Add(toggle);

                NotificationPreferencesPanel.Children.Add(row);
            }

            _isPopulating = false;
        }



    }
}