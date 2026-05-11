namespace BankApp.Client.Views
{
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;
    using BankApp.Client.ViewModels;

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
            await this.ViewModel.LoadAccountsAsync(1);
        }
    }
}