using System;
using System.Threading.Tasks; // Fixed SA1208: Tasks at the top
using BankApp.Client.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BankApp.Client.ViewModels
{
    // MUST be partial and inherit from ObservableObject
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
        private string _statusMessage;

        [ObservableProperty]
        private decimal _estimatedFee;

        [ObservableProperty]
        private decimal _totalAmount;

        public CryptoTradingViewModel(IInvestmentsService service)
        {
            _service = service;
            _ = LoadBalance();
        }

        private async Task LoadBalance()
        {
            var p = await _service.GetPortfolioAsync(1);
            CurrentBalance = p?.TotalValue ?? 0;
        }

        [RelayCommand]
        private async Task ExecuteTrade()
        {
            if (!decimal.TryParse(QuantityText, out var qty) || qty <= 0)
            {
                return; // Fixed SA1503: Added braces
            }

            StatusMessage = "Processing...";
            decimal mockPrice = SelectedTicker == "BTC" ? 65000m : 3000m;

            var success = await _service.ExecuteTradeAsync(1, SelectedTicker, ActionType, qty, mockPrice);

            if (success)
            {
                StatusMessage = "Trade Successful!";
                QuantityText = "0";
                _ = LoadBalance();
            }
            else
            {
                StatusMessage = "Trade Failed.";
            }
        }
    }
}