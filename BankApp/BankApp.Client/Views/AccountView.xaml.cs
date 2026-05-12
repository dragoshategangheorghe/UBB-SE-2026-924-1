namespace BankApp.Client.Views
{
    using BankApp.Client.ViewModels;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;

    /// <summary>
    /// View for displaying bank accounts.
    /// </summary>
    public sealed partial class AccountView : Page
    {
        public AccountView()
        {
            this.InitializeComponent();
            this.ViewModel = new AccountViewModel();
            this.DataContext = this.ViewModel;
        }

        public AccountViewModel ViewModel { get; }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            int? userId = App.AuthService.GetCurrentUserId();
            if (userId == null)
            {
                return;
            }

            await this.ViewModel.LoadAccountsAsync(userId.Value);
        }
    }
}
