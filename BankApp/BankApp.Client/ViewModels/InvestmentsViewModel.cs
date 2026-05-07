using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using BankApp.Models.Entities;
using BankApp.Client.Utilities;

namespace BankApp.Client.ViewModels
{
    public class InvestmentsViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        private const string RefreshPricesEventName = "refreshPrices";
        private readonly DispatcherQueue? dispatcherQueue;

        private string activeFilterType = "All";
        private ObservableCollection<InvestmentHolding> displayedHoldings;
        private bool hasLoaded;
        private bool isPortfolioLoading;
        private Portfolio userPortfolio;

        public InvestmentsViewModel()
        {
            this.dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            this.SelectFilterCommand = new RelayCommand<string>(this.ApplyFilter);

            this.userPortfolio = new Portfolio();
            this.displayedHoldings = new ObservableCollection<InvestmentHolding>();
        }

        public ICommand SelectFilterCommand { get; }
        public bool IsEmptyStateVisible => !this.IsPortfolioLoading && !this.DisplayedHoldings.Any();
        public bool IsHoldingsVisible => !this.IsEmptyStateVisible;

        public string ActiveFilterType
        {
            get => this.activeFilterType;
            set
            {
                if (this.activeFilterType == value) return;
                this.activeFilterType = value;
                this.RefreshDisplayedHoldings();
                this.OnPropertyChanged();
            }
        }

        public ObservableCollection<InvestmentHolding> DisplayedHoldings
        {
            get => this.displayedHoldings;
            private set
            {
                this.displayedHoldings = value;
                this.OnPropertyChanged();
                this.NotifyEmptyStateChanged();
            }
        }

        public Portfolio UserPortfolio
        {
            get => this.userPortfolio;
            set
            {
                this.userPortfolio = value;
                this.OnPropertyChanged();
            }
        }

        public bool IsPortfolioLoading
        {
            get => this.isPortfolioLoading;
            set
            {
                this.isPortfolioLoading = value;
                this.OnPropertyChanged();
                this.NotifyEmptyStateChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void EnsureInitialized()
        {
            if (this.hasLoaded) return;
            this.hasLoaded = true;
            this.LoadUserPortfolio();
        }

        public async void LoadUserPortfolio()
        {
            this.IsPortfolioLoading = true;

            try
            {

                var portfolio = await App.ApiService.GetAsync<Portfolio>($"/api/investments/portfolio/1");

                if (portfolio != null)
                {
                    this.UserPortfolio = portfolio;
                    this.RefreshDisplayedHoldings();
                }
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"LoadUserPortfolio API error: {exception.Message}");
            }
            finally
            {
                this.IsPortfolioLoading = false;
            }
        }

        public void ApplyFilter(string? filterType)
        {
            this.ActiveFilterType = string.IsNullOrWhiteSpace(filterType) ? "All" : filterType;
        }

        private void RefreshDisplayedHoldings()
        {
            this.DisplayedHoldings.Clear();
            var holdings = this.UserPortfolio?.Holdings ?? Enumerable.Empty<InvestmentHolding>();

            // Filters logic: if All, show all; otherwise filter by type
            var filtered = this.ActiveFilterType == "All"
                ? holdings
                : holdings.Where(h => h.AssetType.Equals(this.ActiveFilterType, StringComparison.OrdinalIgnoreCase));

            foreach (var holding in filtered)
            {
                this.DisplayedHoldings.Add(holding);
            }

            this.NotifyEmptyStateChanged();
        }

        public void StopMarketDataPolling()
        {
            // Logic to stop server polling can be implemented here if needed
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void NotifyEmptyStateChanged()
        {
            this.OnPropertyChanged(nameof(this.IsEmptyStateVisible));
            this.OnPropertyChanged(nameof(this.IsHoldingsVisible));
        }
    }
}