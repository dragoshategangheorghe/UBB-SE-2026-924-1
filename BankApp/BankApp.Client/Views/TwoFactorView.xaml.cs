using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using BankApp.Client.Utilities;
using BankApp.Client.ViewModels;
using BankApp.Models.Enums;

namespace BankApp.Client.Views
{
    public sealed partial class TwoFactorView : Page, Observer<TwoFactorState>
    {
        private readonly TwoFactorViewModel _viewModel;
        private DispatcherTimer _countdownTimer;
        private int _secondsRemaining = 30;

        public TwoFactorView()
        {
            this.InitializeComponent();

            _viewModel = new TwoFactorViewModel(App.ApiService);
            _viewModel.State.AddObserver(this);

            _countdownTimer = new DispatcherTimer();
            _countdownTimer.Interval = TimeSpan.FromSeconds(1);
            _countdownTimer.Tick += CountdownTimer_Tick;
        }

        public void Update(TwoFactorState state)
        {
            OnStateChanged(state);
        }

        public void OnStateChanged(TwoFactorState state)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                HideLoading();
                ErrorInfoBar.IsOpen = false;

                switch (state)
                {
                    case TwoFactorState.Idle:
                        break;

                    case TwoFactorState.Verifying:
                        ShowLoading();
                        break;

                    case TwoFactorState.Success:
                        App.NavigationService.NavigateTo<NavView>();
                        break;

                    case TwoFactorState.InvalidOTP:
                        ShowError("The code you entered is incorrect.");
                        break;

                    case TwoFactorState.Expired:
                        ShowError("This code has expired. Please request a new one.");
                        break;

                    case TwoFactorState.MaxAttemptsReached:
                        ShowError("Maximum attempts reached. Your account has been locked.");
                        VerifyButton.IsEnabled = false;
                        OtpBox.IsEnabled = false;
                        break;
                }
            });
        }

        public void ShowError(string msg)
        {
            ErrorInfoBar.Message = msg;
            ErrorInfoBar.IsOpen = true;
        }

        public void ShowLoading()
        {
            LoadingRing.IsActive = true;
            LoadingRing.Visibility = Visibility.Visible;
            VerifyButton.IsEnabled = false;
            OtpBox.IsEnabled = false;
        }

        public void HideLoading()
        {
            LoadingRing.IsActive = false;
            LoadingRing.Visibility = Visibility.Collapsed;
            VerifyButton.IsEnabled = true;
            OtpBox.IsEnabled = true;
        }

        private async void VerifyButton_Click(object sender, RoutedEventArgs e)
        {
            var otp = OtpBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(otp) || otp.Length != 6)
            {
                ShowError("Please enter a valid 6-digit code.");
                return;
            }

            await _viewModel.VerifyOTP(otp);
        }

        private async void ResendButton_Click(object sender, RoutedEventArgs e)
        {
            ResendButton.IsEnabled = false;
            _secondsRemaining = 30;
            CountdownText.Text = $"Available in {_secondsRemaining}s";
            CountdownText.Visibility = Visibility.Visible;

            _countdownTimer.Start();

            await _viewModel.ResendOTP();
        }

        private void CountdownTimer_Tick(object sender, object e)
        {
            _secondsRemaining--;

            if (_secondsRemaining <= 0)
            {
                _countdownTimer.Stop();
                ResendButton.IsEnabled = true;
                CountdownText.Visibility = Visibility.Collapsed;
            }
            else
            {
                CountdownText.Text = $"Available in {_secondsRemaining}s";
            }
        }

        private void BackToLoginButton_Click(object sender, RoutedEventArgs e)
        {
            App.ApiService.ClearToken();
            App.NavigationService.NavigateTo<LoginView>();
        }
    }
}