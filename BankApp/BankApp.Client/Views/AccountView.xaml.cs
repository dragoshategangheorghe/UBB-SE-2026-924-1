using System;
using BankApp.Client.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace BankApp.Client.Views
{
    /// <summary>
    /// View for displaying bank accounts.
    /// </summary>
    public sealed partial class AccountView : Page
    {
        public AccountView()
        {
            InitializeComponent();
            ViewModel = new AccountViewModel(App.AccountService, App.AuthService);
            DataContext = ViewModel;
            Loaded += AccountView_Loaded;
            Unloaded += AccountView_Unloaded;
        }

        public AccountViewModel ViewModel { get; }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await ViewModel.LoadAsync();
        }

        private async void AccountView_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Accounts.Count == 0)
            {
                await ViewModel.LoadAsync();
            }
        }

        private void AccountView_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.Dispose();
        }
    }
}
