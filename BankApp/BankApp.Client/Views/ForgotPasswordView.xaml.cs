using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using BankApp.Client.Utilities;
using BankApp.Client.ViewModels;
using BankApp.Models.Enums;

namespace BankApp.Client.Views
{
    public sealed partial class ForgotPasswordView : Page, Observer<ForgotPasswordState>
    {
        private readonly ForgotPasswordViewModel _viewModel;

        public ForgotPasswordView()
        {
            this.InitializeComponent();

            _viewModel = new ForgotPasswordViewModel(App.ApiService);
            _viewModel.State.AddObserver(this);
        }

        public void Update(ForgotPasswordState state)
        {
            OnStateChanged(state);
        }

        public void OnStateChanged(ForgotPasswordState state)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                HideLoading();

                switch (state)
                {
                    case ForgotPasswordState.Idle:
                        Step1Panel.Visibility = Visibility.Visible;
                        Step2Panel.Visibility = Visibility.Collapsed;
                        break;

                    case ForgotPasswordState.EmailSent:
                        ShowMessage("A recovery code has been sent to your email.", InfoBarSeverity.Success);
                        InstructionText.Text = "Please paste the code from your email to continue.";
                        Step1Panel.Visibility = Visibility.Collapsed;
                        Step2Panel.Visibility = Visibility.Visible;
                        Step3Panel.Visibility = Visibility.Collapsed;
                        VerifyTokenButton.Visibility = Visibility.Visible;
                        ResendPanel.Visibility = Visibility.Visible;
                        TokenBox.IsEnabled = true;
                        TokenBox.Text = "";
                        break;

                    case ForgotPasswordState.PasswordResetSuccess:
                        ShowMessage("Your password has been reset successfully! You can now log in.", InfoBarSeverity.Success);
                        Step1Panel.Visibility = Visibility.Collapsed;
                        Step2Panel.Visibility = Visibility.Collapsed;
                        InstructionText.Text = "Account recovered successfully.";
                        break;

                    case ForgotPasswordState.TokenValid:
                        ShowMessage("Code verified! You can now set a new password.", InfoBarSeverity.Success);
                        VerifyTokenButton.Visibility = Visibility.Collapsed;
                        ResendPanel.Visibility = Visibility.Collapsed;
                        TokenBox.IsEnabled = false;
                        Step3Panel.Visibility = Visibility.Visible;
                        break;

                    case ForgotPasswordState.TokenExpired:
                        ShowMessage("The recovery code has expired. Please request a new one.", InfoBarSeverity.Error);
                        break;

                    case ForgotPasswordState.TokenAlreadyUsed:
                        ShowMessage("This recovery code has already been used.", InfoBarSeverity.Error);
                        break;

                    case ForgotPasswordState.Error:
                        ShowMessage("An error occurred. Please try again.", InfoBarSeverity.Error);
                        break;
                }
            });
        }

        private async void SendCodeButton_Click(object sender, RoutedEventArgs e)
        {
            StatusInfoBar.IsOpen = false;
            var email = EmailBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                ShowMessage("Please enter your email address.", InfoBarSeverity.Warning);
                return;
            }

            ShowLoading();
            await _viewModel.ForgotPassword(email);
        }

        private async void ResetPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            StatusInfoBar.IsOpen = false;
            var code = TokenBox.Text.Trim();
            var newPassword = NewPasswordBox.Password;
            var confirmPassword = ConfirmPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(newPassword))
            {
                ShowMessage("Please fill in all fields.", InfoBarSeverity.Warning);
                return;
            }

            if (newPassword != confirmPassword)
            {
                ShowMessage("Passwords do not match.", InfoBarSeverity.Warning);
                return;
            }

            if (newPassword.Length < 8 ||
                !System.Linq.Enumerable.Any(newPassword, char.IsUpper) ||
                !System.Linq.Enumerable.Any(newPassword, char.IsLower) ||
                !System.Linq.Enumerable.Any(newPassword, char.IsDigit) ||
                !System.Linq.Enumerable.Any(newPassword, ch => !char.IsLetterOrDigit(ch)))
            {
                ShowMessage("Password must be at least 8 characters with uppercase, lowercase, a digit, and a special character.", InfoBarSeverity.Warning);
                return;
            }

            ShowLoading();
            await _viewModel.ResetPassword(EmailBox.Text.Trim(), newPassword, code);
        }

        private async void VerifyTokenButton_Click(object sender, RoutedEventArgs e)
        {
            StatusInfoBar.IsOpen = false;
            var code = TokenBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(code))
            {
                ShowMessage("Please paste the recovery code first.", InfoBarSeverity.Warning);
                return;
            }

            ShowLoading();
            await _viewModel.VerifyToken(code);
        }

        private async void ResendCodeButton_Click(object sender, RoutedEventArgs e)
        {
            StatusInfoBar.IsOpen = false;
            var email = EmailBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                ShowMessage("Email is missing. Please go back to login and try again.", InfoBarSeverity.Error);
                return;
            }

            ShowLoading();

            await _viewModel.ForgotPassword(email);
        }

        private void BackToLoginButton_Click(object sender, RoutedEventArgs e)
        {
            App.NavigationService.NavigateTo<LoginView>();
        }

        public void ShowMessage(string msg, InfoBarSeverity severity)
        {
            StatusInfoBar.Message = msg;
            StatusInfoBar.Severity = severity;
            StatusInfoBar.IsOpen = true;
        }

        public void ShowLoading()
        {
            LoadingRing.IsActive = true;
            LoadingRing.Visibility = Visibility.Visible;
            SendCodeButton.IsEnabled = false;
            ResetPasswordButton.IsEnabled = false;
        }

        public void HideLoading()
        {
            LoadingRing.IsActive = false;
            LoadingRing.Visibility = Visibility.Collapsed;
            SendCodeButton.IsEnabled = true;
            ResetPasswordButton.IsEnabled = true;
        }
    }
}