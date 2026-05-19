using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using BankApp.Client.Commands;
using BankApp.Client.Services.Interfaces;
using BankApp.Models.DTOs.Statistics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BankApp.Client.ViewModels
{
    public class StatisticsViewModel : BaseViewModel
    {
        private readonly IStatisticsService _statisticsService;
        private readonly IAuthService _authService;
        private readonly AsyncRelayCommand _refreshCommand;

        private bool _isLoading;
        private string _statusMessage = string.Empty;
        private bool _isStatusOpen;
        private InfoBarSeverity _statusSeverity = InfoBarSeverity.Informational;
        private decimal _income;
        private decimal _expenses;
        private decimal _net;
        private decimal _totalSpending;
        private double _maxCategoryAmount = 1;
        private double _maxBalanceAmount = 1;
        private double _maxTopRecipientAmount = 1;

        public StatisticsViewModel(IStatisticsService statisticsService, IAuthService authService)
        {
            _statisticsService = statisticsService;
            _authService = authService;
            SpendingByCategory = new ObservableCollection<CategorySpendingPointDto>();
            BalanceTrends = new ObservableCollection<BalanceTrendPointDto>();
            TopRecipients = new ObservableCollection<TopCounterpartyDto>();
            _refreshCommand = new AsyncRelayCommand(LoadAsync, () => !_isLoading);
        }

        public ObservableCollection<CategorySpendingPointDto> SpendingByCategory { get; }

        public ObservableCollection<BalanceTrendPointDto> BalanceTrends { get; }

        public ObservableCollection<TopCounterpartyDto> TopRecipients { get; }

        public AsyncRelayCommand RefreshCommand => _refreshCommand;

        public bool IsLoading
        {
            get => _isLoading;
            private set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    _refreshCommand.RaiseCanExecuteChanged();
                    OnPropertyChanged(nameof(LoadingVisibility));
                }
            }
        }

        public Visibility LoadingVisibility => IsLoading ? Visibility.Visible : Visibility.Collapsed;

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public bool IsStatusOpen
        {
            get => _isStatusOpen;
            private set => SetProperty(ref _isStatusOpen, value);
        }

        public InfoBarSeverity StatusSeverity
        {
            get => _statusSeverity;
            private set => SetProperty(ref _statusSeverity, value);
        }

        public decimal Income
        {
            get => _income;
            private set => SetProperty(ref _income, value);
        }

        public decimal Expenses
        {
            get => _expenses;
            private set => SetProperty(ref _expenses, value);
        }

        public decimal Net
        {
            get => _net;
            private set => SetProperty(ref _net, value);
        }

        public decimal TotalSpending
        {
            get => _totalSpending;
            private set
            {
                if (SetProperty(ref _totalSpending, value))
                {
                    OnPropertyChanged(nameof(FormattedTotalSpendingLabel));
                }
            }
        }

        public string FormattedTotalSpendingLabel => $"Total spending: {TotalSpending:C2}";

        public double MaxCategoryAmount
        {
            get => _maxCategoryAmount;
            private set => SetProperty(ref _maxCategoryAmount, value);
        }

        public double MaxBalanceAmount
        {
            get => _maxBalanceAmount;
            private set => SetProperty(ref _maxBalanceAmount, value);
        }

        public double MaxTopRecipientAmount
        {
            get => _maxTopRecipientAmount;
            private set => SetProperty(ref _maxTopRecipientAmount, value);
        }

        public bool HasData => SpendingByCategory.Count > 0 || BalanceTrends.Count > 0 || TopRecipients.Count > 0;

        public async Task LoadAsync()
        {
            if (!_authService.IsAuthenticated())
            {
                ShowStatus("You must sign in to view statistics.", InfoBarSeverity.Warning);
                return;
            }

            try
            {
                IsLoading = true;
                ShowStatus(string.Empty, InfoBarSeverity.Informational);

                Task<SpendingByCategoryResponse?> spendingTask = _statisticsService.GetSpendingByCategoryAsync();
                Task<IncomeVsExpensesResponse?> incomeTask = _statisticsService.GetIncomeVsExpensesAsync();
                Task<BalanceTrendsResponse?> balanceTask = _statisticsService.GetBalanceTrendsAsync();
                Task<TopRecipientsResponse?> topRecipientsTask = _statisticsService.GetTopRecipientsAsync();

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

                ShowStatus("Statistics refreshed successfully.", InfoBarSeverity.Success);
                OnPropertyChanged(nameof(HasData));
            }
            catch (UnauthorizedAccessException)
            {
                ShowStatus("Your session expired. Please sign in again.", InfoBarSeverity.Warning);
            }
            catch (Exception ex)
            {
                ShowStatus($"Failed to load statistics: {ex.Message}", InfoBarSeverity.Error);
            }
            finally
            {
                IsLoading = false;
                OnPropertyChanged(nameof(HasData));
            }
        }

        private void ShowStatus(string message, InfoBarSeverity severity)
        {
            StatusMessage = message;
            StatusSeverity = severity;
            IsStatusOpen = !string.IsNullOrWhiteSpace(message);
        }

        private static void ReplaceCollection<T>(ObservableCollection<T> target, System.Collections.Generic.IEnumerable<T> source)
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
