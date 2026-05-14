using System;
using BankApp.Client.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BankApp.Client.Views
{
    public sealed partial class CryptoTradingView : Page
    {
        public CryptoTradingViewModel ViewModel { get; }

        public CryptoTradingView()
        {
            this.InitializeComponent();

            var app = (App)Application.Current;
            this.ViewModel = app.Services.GetService<CryptoTradingViewModel>();
            this.DataContext = this.ViewModel;
        }

        private void OnActionTypeChecked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && ViewModel != null)
            {
                ViewModel.ActionType = rb.Tag?.ToString() ?? "BUY";
            }
        }

        private void OnBackButtonClicked(object sender, RoutedEventArgs e)
        {
            // Reliable UI-level frame manipulation routing
            if (this.Frame != null && this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
            else
            {
                this.Frame?.Navigate(typeof(InvestmentsView));
            }
        }
    }
}