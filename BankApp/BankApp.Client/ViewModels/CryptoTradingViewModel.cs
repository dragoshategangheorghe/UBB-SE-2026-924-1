using System;
using System.Diagnostics;
using System.Threading.Tasks;
using BankApp.Client.Services.Interfaces;
using Microsoft.UI.Xaml.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BankApp.Client.ViewModels
{
    public partial class CryptoTradingViewModel : ObservableObject
    {
        private readonly IInvestmentsService _service;

        [ObservableProperty]
        private string _selectedTicker = "BTC";

        [ObservableProperty]
        private string _actionType = "BUY";

        [ObservableProperty]
        private string _quantityText = "0";

        [ObservableProperty]
        private decimal _currentBalance;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private decimal _estimatedFee;

        [ObservableProperty]
        private decimal _totalAmount;

        [ObservableProperty]
        private bool _isProcessing;

        public CryptoTradingViewModel(IInvestmentsService service)
        {
            _service = service;
            _ = LoadBalance();
        }

        private async Task LoadBalance()
        {
            try
            {
                var p = await _service.GetPortfolioForCurrentUserAsync();
                CurrentBalance = p?.TotalValue ?? 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed loading portfolio balance: {ex.Message}");
            }
        }

        // Detects when selected token switches dropdown inputs
        partial void OnSelectedTickerChanged(string value) => CalculateLiveTotals();

        // Detects when the user type numbers inside input textboxes
        partial void OnQuantityTextChanged(string value) => CalculateLiveTotals();

        private void CalculateLiveTotals()
        {
            if (!decimal.TryParse(QuantityText, out decimal qty) || qty <= 0)
            {
                EstimatedFee = 0;
                TotalAmount = 0;
                StatusMessage = string.Empty;
                return;
            }

            // Simple conditional logic for market values based on ticker selection
            decimal currentMarketPrice = SelectedTicker switch
            {
                "BTC" => 65000.00m,
                "ETH" => 2550.00m,
                "SOL" => 145.00m,
                _ => 0m
            };

            decimal principalCost = qty * currentMarketPrice;
            EstimatedFee = Math.Round(principalCost * 0.015m, 2);
            TotalAmount = Math.Round(principalCost + EstimatedFee, 2);
            StatusMessage = $"Ready to submit trade at {currentMarketPrice:N2} RON unit valuation.";
        }

        [RelayCommand]
        public async Task ExecuteTradeAsync()
        {
            if (!decimal.TryParse(QuantityText, out decimal qty) || qty <= 0)
            {
                StatusMessage = "Please insert a valid currency volume.";
                return;
            }

            IsProcessing = true;
            StatusMessage = "Processing secure network order verification...";

            try
            {
                decimal currentMarketPrice = SelectedTicker switch
                {
                    "BTC" => 65000.00m,
                    "ETH" => 2550.00m,
                    "SOL" => 145.00m,
                    _ => 0m
                };

                // Triggers API pipeline directly
                bool success = await _service.ExecuteTradeAsync(1, SelectedTicker, ActionType, qty, currentMarketPrice);

                if (success)
                {
                    StatusMessage = "Transaction verified successfully!";

                    // Unified Application Router Frame Navigation
                    if (App.MainAppWindow?.Content is Frame targetFrame && targetFrame.CanGoBack)
                    {
                        targetFrame.GoBack();
                    }
                }
                else
                {
                    StatusMessage = "Server dropped execution package. Validate account balances.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error responding: {ex.Message}";
                Debug.WriteLine($"Trade Failure Trace: {ex.Message}");
            }
            finally
            {
                IsProcessing = false;
            }
        }
    }
}