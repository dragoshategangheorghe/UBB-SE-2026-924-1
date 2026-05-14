using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using BankApp.Client.Commands;
using BankApp.Client.Services.Interfaces;
using BankApp.Client.Utilities;
using BankApp.Models.Entities;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BankApp.Client.ViewModels
{
    public class InvestmentsViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        private readonly DispatcherQueue? dispatcherQueue;
        private readonly IInvestmentsService investmentsService;

        private string activeFilterType = "All";
        private ObservableCollection<InvestmentHolding> displayedHoldings;
        private bool hasLoaded;
        private bool isPortfolioLoading;
        private Portfolio userPortfolio;

        public InvestmentsViewModel(IInvestmentsService investmentsService)
        {
            this.dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            this.investmentsService = investmentsService;

            // Commands
            this.SelectFilterCommand = new RelayCommand<string>(this.ApplyFilter);
            this.OpenTradeDialogCommand = new RelayCommand(async () => await this.HandleTradeAsync());

            this.userPortfolio = new Portfolio();
            this.displayedHoldings = new ObservableCollection<InvestmentHolding>();
        }

        // --- Commands ---
        public ICommand SelectFilterCommand { get; }
        public ICommand OpenTradeDialogCommand { get; }

        // --- Properties ---
        public bool IsEmptyStateVisible => !this.IsPortfolioLoading && !this.DisplayedHoldings.Any();
        public bool IsHoldingsVisible => !this.IsEmptyStateVisible;

        public string ActiveFilterType
        {
            get => this.activeFilterType;
            set
            {
                if (this.activeFilterType == value)
                {
                    return;
                }

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

        // --- Logic ---
        public void EnsureInitialized()
        {
            if (this.hasLoaded)
            {
                return;
            }

            this.hasLoaded = true;
            this.LoadUserPortfolio();
        }

        public async void LoadUserPortfolio()
        {
            this.IsPortfolioLoading = true;
            try
            {
                var portfolio = await investmentsService.GetPortfolioForCurrentUserAsync();

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

        private async Task HandleTradeAsync()
        {
            // This is much simpler and avoids that double-negative logic
            if (App.MainAppWindow?.Content is Frame rootFrame)
            {
                rootFrame.Navigate(typeof(BankApp.Client.Views.CryptoTradingView));
            }
            else
            {
                Debug.WriteLine("Navigation Error: Root content is not a Frame.");
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

            // Fixed the plural/singular logic (Filter says "Stocks", DB says "Stock")
            var filtered = this.ActiveFilterType switch
            {
                "All" => holdings,
                "Stocks" => holdings.Where(h => h.AssetType.Equals("Stock", StringComparison.OrdinalIgnoreCase)),
                "Crypto" => holdings.Where(h => h.AssetType.Equals("Crypto", StringComparison.OrdinalIgnoreCase)),
                _ => holdings.Where(h => h.AssetType.Equals(this.ActiveFilterType, StringComparison.OrdinalIgnoreCase))
            };

            foreach (var holding in filtered)
            {
                this.DisplayedHoldings.Add(holding);
            }

            this.NotifyEmptyStateChanged();
        }

        public void StopMarketDataPolling()
        {
        }

        public event PropertyChangedEventHandler? PropertyChanged;
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