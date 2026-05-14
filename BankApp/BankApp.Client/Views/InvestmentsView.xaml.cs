using BankApp.Client.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace BankApp.Client.Views
{
    public sealed partial class InvestmentsView : Page
    {
        public InvestmentsView()
        {
            this.InitializeComponent();

            // Link the ViewModel (Using the Service from your App.xaml.cs)
            this.ViewModel = new InvestmentsViewModel(App.InvestmentsService);
            this.DataContext = this.ViewModel;

            this.Loaded += this.OnPageLoaded;
            this.Unloaded += this.OnPageUnloaded;
        }

        public InvestmentsViewModel ViewModel { get; }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            // Triggers the data fetch from the Server
            this.ViewModel.EnsureInitialized();
        }

        private void OnPageUnloaded(object sender, RoutedEventArgs e)
        {
            this.ViewModel.StopMarketDataPolling();
            // Important: Remove handlers to prevent memory leaks
            this.Loaded -= this.OnPageLoaded;
            this.Unloaded -= this.OnPageUnloaded;
        }
    }
}