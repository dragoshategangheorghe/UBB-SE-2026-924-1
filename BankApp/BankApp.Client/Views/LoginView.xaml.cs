using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using BankApp.Client.Utilities;
using BankApp.Client.ViewModels;
using BankApp.Models.Enums;

namespace BankApp.Client.Views
{
    public sealed partial class LoginView : Page, Observer<LoginState>
    {
        private readonly LoginViewModel _viewModel;

        public LoginView()
        {
            this.InitializeComponent();

            _viewModel = new LoginViewModel(App.ApiService);
            _viewModel.State.AddObserver(this);
        }

        public void Update(LoginState state)
        {
            OnStateChanged(state);
        }

        public void OnStateChanged(LoginState state)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                HideLoading();
                ErrorInfoBar.IsOpen = false;

                switch (state)
                {
                    case LoginState.Idle:
                        break;

                    case LoginState.Loading:
                        ShowLoading();
                        break;

                    case LoginState.Success:
                        App.NavigationService.NavigateTo<NavView>(); // goes to NavView
                        break;

                    case LoginState.Require2FA:
                        App.NavigationService.NavigateTo<TwoFactorView>();
                        break;

                    case LoginState.InvalidCredentials:
                        ShowError("Invalid email or password.");
                        break;

                    case LoginState.AccountLocked:
                        ShowError("Account is locked. Try again later.");
                        break;

                    case LoginState.Error:
                        ShowError("Something went wrong. Please try again.");
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
            SignInButton.IsEnabled = false;
        }

        public void HideLoading()
        {
            LoadingRing.IsActive = false;
            LoadingRing.Visibility = Visibility.Collapsed;
            SignInButton.IsEnabled = true;
        }

        private void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            var email = EmailBox.Text.Trim();
            var password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ShowError("Please enter email and password.");
                return;
            }

            _viewModel.Login(email, password);
        }

        private void GoogleLoginButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.OAuthLogin(EmailBox.Text.Trim(), "Google");
        }

        private void ForgotPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            App.NavigationService.NavigateTo<ForgotPasswordView>();
        }

        private void CreateAccountButton_Click(object sender, RoutedEventArgs e)
        {
            App.NavigationService.NavigateTo<RegisterView>();
        }
    }
}