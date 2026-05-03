using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using BankApp.Client.Commands;
using BankApp.Client.Services.Interfaces;
using BankApp.Client.Utilities;
using BankApp.Models.DTOs.Statistics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BankApp.Client.ViewModels
{
    public class StatisticsViewModel : BaseViewModel
    {
        private readonly IStatisticsApiService _statisticsApiService;
        private readonly AsyncRelayCommand _refreshCommand;

        private bool _isLoading;
        private bool _isStatusOpen;
        private InfoBarSeverity _statusSeverity;
        private string _statusMessage = string.Empty;
        private decimal _income;
        private decimal _expenses;
        private decimal _net;
        private decimal _totalSpending;
        private double _maxCategoryAmount = 1;
        private double _maxBalanceAmount = 1;
        private double _maxTopRecipientAmount = 1;

        public StatisticsViewModel(IStatisticsApiService statisticsApiService)
        {
            _statisticsApiService = statisticsApiService;
            SpendingByCategory = new ObservableCollection<CategorySpendingPointDto>();
            BalanceTrends = new ObservableCollection<BalanceTrendPointDto>();
            TopRecipients = new ObservableCollection<TopCounterpartyDto>();
            _refreshCommand = new AsyncRelayCommand(LoadAsync);
        }

        public ObservableCollection<CategorySpendingPointDto> SpendingByCategory { get; }

        public ObservableCollection<BalanceTrendPointDto> BalanceTrends { get; }

        public ObservableCollection<TopCounterpartyDto> TopRecipients { get; }

        public AsyncRelayCommand RefreshCommand => _refreshCommand;

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    OnPropertyChanged(nameof(LoadingVisibility));
                }
            }
        }

        public Visibility LoadingVisibility => IsLoading ? Visibility.Visible : Visibility.Collapsed;

        public bool IsStatusOpen
        {
            get => _isStatusOpen;
            set => SetProperty(ref _isStatusOpen, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public InfoBarSeverity StatusSeverity
        {
            get => _statusSeverity;
            set => SetProperty(ref _statusSeverity, value);
        }

        public decimal Income
        {
            get => _income;
            set => SetProperty(ref _income, value);
        }

        public decimal Expenses
        {
            get => _expenses;
            set => SetProperty(ref _expenses, value);
        }

        public decimal Net
        {
            get => _net;
            set => SetProperty(ref _net, value);
        }

        public decimal TotalSpending
        {
            get => _totalSpending;
            set => SetProperty(ref _totalSpending, value);
        }

        public double MaxCategoryAmount
        {
            get => _maxCategoryAmount;
            set => SetProperty(ref _maxCategoryAmount, value);
        }

        public double MaxBalanceAmount
        {
            get => _maxBalanceAmount;
            set => SetProperty(ref _maxBalanceAmount, value);
        }

        public double MaxTopRecipientAmount
        {
            get => _maxTopRecipientAmount;
            set => SetProperty(ref _maxTopRecipientAmount, value);
        }

        public async Task LoadAsync()
        {
            try
            {
                IsLoading = true;

                Task<SpendingByCategoryResponse?> spendingTask = _statisticsApiService.GetSpendingByCategoryAsync();
                Task<IncomeVsExpensesResponse?> incomeTask = _statisticsApiService.GetIncomeVsExpensesAsync();
                Task<BalanceTrendsResponse?> balanceTask = _statisticsApiService.GetBalanceTrendsAsync();
                Task<TopRecipientsResponse?> topRecipientsTask = _statisticsApiService.GetTopRecipientsAsync();

                await Task.WhenAll(spendingTask, incomeTask, balanceTask, topRecipientsTask);

                SpendingByCategoryResponse? spendingResponse = await spendingTask;
                IncomeVsExpensesResponse? incomeResponse = await incomeTask;
                BalanceTrendsResponse? balanceResponse = await balanceTask;
                TopRecipientsResponse? topRecipientsResponse = await topRecipientsTask;

                if (spendingResponse?.Success != true ||
                    incomeResponse?.Success != true ||
                    balanceResponse?.Success != true ||
                    topRecipientsResponse?.Success != true)
                {
                    ShowStatus("Failed to load one or more statistics sections.", InfoBarSeverity.Error);
                    return;
                }

                ReplaceCollection(SpendingByCategory, spendingResponse.Categories);
                ReplaceCollection(BalanceTrends, balanceResponse.Points);
                ReplaceCollection(TopRecipients, topRecipientsResponse.Recipients);

                TotalSpending = spendingResponse.TotalSpending;
                Income = incomeResponse.Income;
                Expenses = incomeResponse.Expenses;
                Net = incomeResponse.Net;
                MaxCategoryAmount = SpendingByCategory.Count == 0 ? 1 : (double)SpendingByCategory.Max(item => item.Amount);
                MaxBalanceAmount = BalanceTrends.Count == 0 ? 1 : (double)BalanceTrends.Max(item => item.Balance);
                MaxTopRecipientAmount = TopRecipients.Count == 0 ? 1 : (double)TopRecipients.Max(item => item.TotalAmount);
            }
            catch (Exception ex)
            {
                ShowStatus($"Failed to load statistics: {ex.Message}", InfoBarSeverity.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ShowStatus(string message, InfoBarSeverity severity)
        {
            StatusMessage = message;
            StatusSeverity = severity;
            IsStatusOpen = !string.IsNullOrWhiteSpace(message);
        }

        private static void ReplaceCollection<T>(ObservableCollection<T> target, IEnumerable<T> source)
        {
            target.Clear();
            foreach (T item in source)
            {
                target.Add(item);
            }
        }

        public override void Dispose()
        {
        }
    }
}
