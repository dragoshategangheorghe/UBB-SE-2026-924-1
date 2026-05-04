namespace BankApp.Client.Views
{
    using System;
    using System.Collections.Generic;
    using BankApp.Client.Utilities;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Input;

    public sealed partial class NavView : Page
    {
        private readonly List<Button> _navButtons;
        private Button _activeNavButton;

        public NavView()
        {
            this.InitializeComponent();
            this._navButtons = new List<Button>
            {
                NavDashboard, NavTransfers, NavBillPayments, NavCards,
                NavTransferHistory, NavCurrencyExchange, NavSavings,
                NavInvestments, NavStatistics, NavSupport, NavProfile
            };
            App.NavigationService.SetContentFrame(ContentFrame);
            App.NavigationService.NavigateToContent<DashboardView>();
        }

        public void UpdateNotificationBadge(int count)
        {
            if (count <= 0)
            {
                NotificationBadge.Visibility = Visibility.Collapsed;
                return;
            }

            NotificationBadgeText.Text = count > 99 ? "99+" : count.ToString();
            NotificationBadge.Visibility = Visibility.Visible;
        }

        private void SetActiveNav(Button selected)
        {
            foreach (Button btn in this._navButtons)
            {
                btn.Style = (Style)Resources["NavItemStyle"];
            }

            selected.Style = (Style)Resources["NavItemActiveStyle"];
            this._activeNavButton = selected;
        }

        private void NavDashboard_Click(object sender, RoutedEventArgs e)
        {
            this.SetActiveNav(NavDashboard);
            App.NavigationService.NavigateToContent<DashboardView>();
        }

        private void NavProfile_Click(object sender, RoutedEventArgs e)
        {
            this.SetActiveNav(NavProfile);
            App.NavigationService.NavigateToContent<ProfileView>();
        }

        private void NavCards_Click(object sender, RoutedEventArgs e)
        {
            this.SetActiveNav(NavCards);
            App.NavigationService.NavigateToContent<CardManagementView>();
        }

        private void NavTransferHistory_Click(object sender, RoutedEventArgs e)
        {
            this.SetActiveNav(NavTransferHistory);
            App.NavigationService.NavigateToContent<TransactionHistoryView>();
        }

        private void NavInvestments_Click(object sender, RoutedEventArgs e)
        {
            // Update the UI style to show this tab is active
            this.SetActiveNav(NavInvestments);

            // Navigate the central frame to your merged view using the project's NavigationService[cite: 1]
            App.NavigationService.NavigateToContent<InvestmentsView>();
        }

        private void NavStatistics_Click(object sender, RoutedEventArgs e)
        {
            this.SetActiveNav(NavStatistics);
            App.NavigationService.NavigateToContent<StatisticsView>();
        }

        // All other nav items show a coming soon alert
        private async void NavTransfers_Click(object sender, RoutedEventArgs e) =>
            await this.ShowComingSoonAsync("Transfers");

        private async void NavBillPayments_Click(object sender, RoutedEventArgs e) =>
            await this.ShowComingSoonAsync("Bill Payments");

        private async void NavCurrencyExchange_Click(object sender, RoutedEventArgs e) =>
            await this.ShowComingSoonAsync("Currency Exchange");

        private async void NavSavings_Click(object sender, RoutedEventArgs e) =>
            await this.ShowComingSoonAsync("Savings & Loans");

        private async void NavSupport_Click(object sender, RoutedEventArgs e) =>
            await this.ShowComingSoonAsync("Support");

        private async System.Threading.Tasks.Task ShowComingSoonAsync(string feature)
        {
            var dialog = new ContentDialog
            {
                Title = feature,
                Content = $"{feature} is coming soon.",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }

        private void NotificationBell_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            // TODO: show notifications panel
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            App.ApiService.ClearToken();
            App.NavigationService.NavigateTo<LoginView>();
        }
    }
}