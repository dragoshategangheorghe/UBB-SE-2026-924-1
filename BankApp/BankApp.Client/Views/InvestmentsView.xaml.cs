using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using BankApp.Client.ViewModels;

namespace BankApp.Client.Views 
{
    public sealed partial class InvestmentsView : Page
    {
        public InvestmentsView()
        {
            this.InitializeComponent();

            // Unified GUI: Initialize the ViewModel and link DataContext
            this.ViewModel = new InvestmentsViewModel(App.InvestmentsService);
            this.DataContext = this.ViewModel;

            this.Loaded += this.OnPageLoaded;
            this.Unloaded += this.OnPageUnloaded;
        }

        public InvestmentsViewModel ViewModel { get; }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            this.ViewModel.EnsureInitialized();
        }

        private void OnPageUnloaded(object sender, RoutedEventArgs e)
        {
            this.ViewModel.StopMarketDataPolling();
            this.Loaded -= this.OnPageLoaded;
            this.Unloaded -= this.OnPageUnloaded;
        }
    }
}