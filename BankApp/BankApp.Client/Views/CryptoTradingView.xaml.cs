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

            // Cast App.Current so we can see the Services property
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
    }
}