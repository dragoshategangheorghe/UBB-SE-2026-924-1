using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using BankApp.Client.Utilities;
using BankApp.Client.ViewModels;
using BankApp.Models.Enums;

namespace BankApp.Client.Views
{
    public sealed partial class RegisterView : Page, Observer<RegisterState>
    {
        private readonly RegisterViewModel _viewModel;

        public RegisterView()
        {
            this.InitializeComponent();

            _viewModel = new RegisterViewModel(App.ApiService);
            _viewModel.State.AddObserver(this);
        }

        public void Update(RegisterState state)
        {
            OnStateChanged(state);
        }

        public void OnStateChanged(RegisterState state)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                HideLoading();
                ErrorInfoBar.IsOpen = false;
                SuccessInfoBar.IsOpen = false;

                switch (state)
                {
                    case RegisterState.Idle:
                        break;

                    case RegisterState.Loading:
                        ShowLoading();
                        break;

                    case RegisterState.Success:
                        SuccessInfoBar.IsOpen = true;
                        ClearForm();
                        break;

                    case RegisterState.AutoLoggedIn:
                        App.NavigationService.NavigateTo<NavView>();
                        break;

                    case RegisterState.EmailAlreadyExists:
                        ShowError("This email is already registered.");
                        break;

                    case RegisterState.InvalidEmail:
                        ShowError("Please enter a valid email address.");
                        break;

                    case RegisterState.WeakPassword:
                        ShowError("Password must be at least 8 characters with uppercase, lowercase, a digit and a special character.");
                        break;

                    case RegisterState.PasswordMismatch:
                        ShowError("Passwords do not match.");
                        break;

                    case RegisterState.Error:
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
            RegisterButton.IsEnabled = false;
        }

        public void HideLoading()
        {
            LoadingRing.IsActive = false;
            LoadingRing.Visibility = Visibility.Collapsed;
            RegisterButton.IsEnabled = true;
        }

        private void ClearForm()
        {
            FullNameBox.Text = "";
            EmailBox.Text = "";
            PasswordBox.Password = "";
            ConfirmPasswordBox.Password = "";
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var fullName = FullNameBox.Text.Trim();
            var email = EmailBox.Text.Trim();
            var password = PasswordBox.Password;
            var confirmPassword = ConfirmPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email)
                || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                ShowError("Please fill in all fields.");
                return;
            }

            _viewModel.Register(email, password, confirmPassword, fullName);
        }

        private void GoogleRegisterButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.OAuthRegister(EmailBox.Text.Trim(), "Google");
        }

        private void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            App.NavigationService.NavigateTo<LoginView>();
        }
    }
}