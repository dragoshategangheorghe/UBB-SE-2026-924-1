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

            // --- CLIENT-SIDE PROTECTION: BUY ORDER WALLET LIMIT ---
            if (ActionType == "BUY" && TotalAmount > CurrentBalance)
            {
                StatusMessage = $" Insufficient Funds: Total cost ({TotalAmount:N2} RON) exceeds your wallet balance ({CurrentBalance:N2} RON).";
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

                // Get the ID from the App's global Auth Service
                int? userId = App.AuthService.GetCurrentUserId();

                if (!userId.HasValue)
                {
                    StatusMessage = " Session expired. Please log in again.";
                    return;
                }

                bool success = await _service.ExecuteTradeAsync(userId.Value, SelectedTicker, ActionType, qty, currentMarketPrice);

                if (success)
                {
                    StatusMessage = "Transaction verified successfully!";

                    if (App.MainAppWindow?.Content is Frame targetFrame && targetFrame.CanGoBack)
                    {
                        targetFrame.GoBack();
                    }
                }
            }
            catch (Exception ex)
            {
                // --- CLEAN JSON STRING PARSING FOR ERROR DUMPS ---
                string rawError = ex.Message;
                if (rawError.Contains("Body: "))
                {
                    try
                    {
                        int bodyStartIndex = rawError.IndexOf("Body: ") + 6;
                        string jsonBody = rawError.Substring(bodyStartIndex);

                        if (jsonBody.Contains("\"detail\":\""))
                        {
                            int detailStart = jsonBody.IndexOf("\"detail\":\"") + 10;
                            int detailEnd = jsonBody.IndexOf("\"", detailStart);
                            StatusMessage = $" {jsonBody.Substring(detailStart, detailEnd - detailStart)}";
                        }
                        else
                        {
                            StatusMessage = " Transaction rejected by server validations.";
                        }
                    }
                    catch
                    {
                        StatusMessage = " Validation processing failure.";
                    }
                }
                else
                {
                    StatusMessage = $" Connection Error: {ex.Message}";
                }

                Debug.WriteLine($"Trade Failure Trace: {ex.Message}");
            }
            finally
            {
                IsProcessing = false;
            }
        }
    }
}