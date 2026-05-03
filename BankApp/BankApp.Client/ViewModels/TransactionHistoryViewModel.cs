using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BankApp.Client.Commands;
using BankApp.Client.Services.Interfaces;
using BankApp.Client.Utilities;
using BankApp.Models.DTOs.Transactions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BankApp.Client.ViewModels
{
    public class TransactionHistoryViewModel : BaseViewModel
    {
        private readonly ITransactionApiService _transactionApiService;
        private readonly ITransactionHistorySessionState _sessionState;
        private readonly AsyncRelayCommand _refreshCommand;
        private readonly AsyncRelayCommand _resetFiltersCommand;
        private readonly AsyncRelayCommand _exportCsvCommand;
        private readonly AsyncRelayCommand _exportPdfCommand;
        private readonly AsyncRelayCommand _exportXlsxCommand;
        private readonly AsyncRelayCommand _downloadReceiptCommand;

        private bool _isLoading;
        private bool _isStatusOpen;
        private InfoBarSeverity _statusSeverity;
        private string _statusMessage = string.Empty;
        private string _searchTerm = string.Empty;
        private DateTimeOffset? _fromDate;
        private DateTimeOffset? _toDate;
        private string _minimumAmountInput = string.Empty;
        private string _maximumAmountInput = string.Empty;
        private AccountFilterOptionDto? _selectedAccount;
        private CardFilterOptionDto? _selectedCard;
        private string? _selectedTransactionType;
        private string? _selectedStatus;
        private string? _selectedDirection;
        private string _selectedSortField = TransactionSortFields.Date;
        private string _selectedSortDirection = SortDirections.Desc;
        private TransactionHistoryItemDto? _selectedTransaction;
        private string _lastExportPath = string.Empty;

        public TransactionHistoryViewModel(ITransactionApiService transactionApiService, ITransactionHistorySessionState sessionState)
        {
            _transactionApiService = transactionApiService;
            _sessionState = sessionState;

            Transactions = new ObservableCollection<TransactionHistoryItemDto>();
            AccountOptions = new ObservableCollection<AccountFilterOptionDto>();
            CardOptions = new ObservableCollection<CardFilterOptionDto>();
            TransactionTypeOptions = new ObservableCollection<string>();
            StatusOptions = new ObservableCollection<string>();
            DirectionOptions = new ObservableCollection<string>();
            SortFieldOptions = new ObservableCollection<SelectableOption>
            {
                new SelectableOption(TransactionSortFields.Date, "Date"),
                new SelectableOption(TransactionSortFields.Amount, "Amount")
            };
            SortDirectionOptions = new ObservableCollection<SelectableOption>
            {
                new SelectableOption(SortDirections.Desc, "Descending"),
                new SelectableOption(SortDirections.Asc, "Ascending")
            };

            _refreshCommand = new AsyncRelayCommand(RefreshAsync);
            _resetFiltersCommand = new AsyncRelayCommand(ResetFiltersAsync);
            _exportCsvCommand = new AsyncRelayCommand(() => ExportAsync(TransactionExportFormats.Csv));
            _exportPdfCommand = new AsyncRelayCommand(() => ExportAsync(TransactionExportFormats.Pdf));
            _exportXlsxCommand = new AsyncRelayCommand(() => ExportAsync(TransactionExportFormats.Xlsx));
            _downloadReceiptCommand = new AsyncRelayCommand(DownloadReceiptAsync, () => SelectedTransaction != null);
        }

        public ObservableCollection<TransactionHistoryItemDto> Transactions { get; }

        public ObservableCollection<AccountFilterOptionDto> AccountOptions { get; }

        public ObservableCollection<CardFilterOptionDto> CardOptions { get; }

        public ObservableCollection<string> TransactionTypeOptions { get; }

        public ObservableCollection<string> StatusOptions { get; }

        public ObservableCollection<string> DirectionOptions { get; }

        public ObservableCollection<SelectableOption> SortFieldOptions { get; }

        public ObservableCollection<SelectableOption> SortDirectionOptions { get; }

        public AsyncRelayCommand RefreshCommand => _refreshCommand;

        public AsyncRelayCommand ResetFiltersCommand => _resetFiltersCommand;

        public AsyncRelayCommand ExportCsvCommand => _exportCsvCommand;

        public AsyncRelayCommand ExportPdfCommand => _exportPdfCommand;

        public AsyncRelayCommand ExportXlsxCommand => _exportXlsxCommand;

        public AsyncRelayCommand DownloadReceiptCommand => _downloadReceiptCommand;

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

        public string SearchTerm
        {
            get => _searchTerm;
            set => SetProperty(ref _searchTerm, value);
        }

        public DateTimeOffset? FromDate
        {
            get => _fromDate;
            set => SetProperty(ref _fromDate, value);
        }

        public DateTimeOffset? ToDate
        {
            get => _toDate;
            set => SetProperty(ref _toDate, value);
        }

        public string MinimumAmountInput
        {
            get => _minimumAmountInput;
            set => SetProperty(ref _minimumAmountInput, value);
        }

        public string MaximumAmountInput
        {
            get => _maximumAmountInput;
            set => SetProperty(ref _maximumAmountInput, value);
        }

        public AccountFilterOptionDto? SelectedAccount
        {
            get => _selectedAccount;
            set => SetProperty(ref _selectedAccount, value);
        }

        public CardFilterOptionDto? SelectedCard
        {
            get => _selectedCard;
            set => SetProperty(ref _selectedCard, value);
        }

        public string? SelectedTransactionType
        {
            get => _selectedTransactionType;
            set => SetProperty(ref _selectedTransactionType, value);
        }

        public string? SelectedStatus
        {
            get => _selectedStatus;
            set => SetProperty(ref _selectedStatus, value);
        }

        public string? SelectedDirection
        {
            get => _selectedDirection;
            set => SetProperty(ref _selectedDirection, value);
        }

        public string SelectedSortField
        {
            get => _selectedSortField;
            set => SetProperty(ref _selectedSortField, value);
        }

        public string SelectedSortDirection
        {
            get => _selectedSortDirection;
            set => SetProperty(ref _selectedSortDirection, value);
        }

        public TransactionHistoryItemDto? SelectedTransaction
        {
            get => _selectedTransaction;
            set
            {
                if (SetProperty(ref _selectedTransaction, value))
                {
                    _sessionState.SelectedTransactionId = value?.Id;
                    RaiseSelectedTransactionChanged();
                }
            }
        }

        public Visibility DetailsVisibility => SelectedTransaction == null ? Visibility.Collapsed : Visibility.Visible;

        public Visibility EmptyDetailsVisibility => SelectedTransaction == null ? Visibility.Visible : Visibility.Collapsed;

        public string SelectedReferenceNumber => SelectedTransaction?.ReferenceNumber ?? string.Empty;

        public string SelectedTimestampText => FormatDateTime(SelectedTransaction?.Timestamp);

        public string SelectedTransactionTypeText => SelectedTransaction?.TransactionType ?? string.Empty;

        public string SelectedCounterpartyText => SelectedTransaction?.CounterpartyOrMerchant ?? string.Empty;

        public string SelectedDescriptionText => SelectedTransaction?.Description ?? string.Empty;

        public string SelectedAmountText => FormatDecimal(SelectedTransaction?.Amount);

        public string SelectedCurrencyText => SelectedTransaction?.Currency ?? string.Empty;

        public string SelectedDirectionText => SelectedTransaction?.Direction ?? string.Empty;

        public string SelectedStatusText => SelectedTransaction?.Status ?? string.Empty;

        public string SelectedRunningBalanceText => FormatDecimal(SelectedTransaction?.RunningBalanceAfterTransaction);

        public string SelectedFeeText => FormatDecimal(SelectedTransaction?.Fee);

        public string SelectedExchangeRateText => SelectedTransaction?.ExchangeRate?.ToString("0.000000", CultureInfo.CurrentCulture) ?? string.Empty;

        public string SelectedSourceIbanText => SelectedTransaction?.SourceAccountIban ?? string.Empty;

        public string SelectedDestinationIbanText => SelectedTransaction?.DestinationAccountIban ?? string.Empty;

        public string LastExportPath
        {
            get => _lastExportPath;
            set => SetProperty(ref _lastExportPath, value);
        }

        public async Task InitializeAsync()
        {
            await LoadMetadataAsync();
            RestoreState();
            await RefreshAsync();
        }

        public async Task RefreshAsync()
        {
            try
            {
                TransactionHistoryRequest? request = BuildRequest();
                if (request == null)
                {
                    return;
                }

                IsLoading = true;
                TransactionHistoryResponse? response = await _transactionApiService.GetHistoryAsync(request);
                if (response == null || !response.Success)
                {
                    ShowStatus("Failed to load transaction history.", InfoBarSeverity.Error);
                    return;
                }

                Transactions.Clear();
                foreach (TransactionHistoryItemDto transaction in response.Transactions)
                {
                    Transactions.Add(transaction);
                }

                SelectedTransaction = Transactions.FirstOrDefault(transaction => transaction.Id == _sessionState.SelectedTransactionId)
                    ?? Transactions.FirstOrDefault();
                PersistState(request);

                if (Transactions.Count == 0)
                {
                    ShowStatus("No transactions match the current filters.", InfoBarSeverity.Informational);
                }
            }
            catch (Exception ex)
            {
                ShowStatus($"Failed to load transactions: {ex.Message}", InfoBarSeverity.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ResetFiltersAsync()
        {
            SearchTerm = string.Empty;
            FromDate = null;
            ToDate = null;
            MinimumAmountInput = string.Empty;
            MaximumAmountInput = string.Empty;
            SelectedAccount = null;
            SelectedCard = null;
            SelectedTransactionType = null;
            SelectedStatus = null;
            SelectedDirection = null;
            SelectedSortField = TransactionSortFields.Date;
            SelectedSortDirection = SortDirections.Desc;
            SelectedTransaction = null;
            LastExportPath = string.Empty;
            _sessionState.Clear();

            await RefreshAsync();
        }

        private async Task LoadMetadataAsync()
        {
            TransactionFilterMetadataResponse? metadata = await _transactionApiService.GetFilterMetadataAsync();
            if (metadata == null || !metadata.Success)
            {
                ShowStatus("Failed to load transaction filters.", InfoBarSeverity.Warning);
                return;
            }

            ReplaceCollection(AccountOptions, metadata.Accounts);
            ReplaceCollection(CardOptions, metadata.Cards);
            ReplaceCollection(TransactionTypeOptions, metadata.AvailableTransactionTypes);
            ReplaceCollection(StatusOptions, metadata.AvailableStatuses);
            ReplaceCollection(DirectionOptions, metadata.AvailableDirections);
        }

        private async Task ExportAsync(string format)
        {
            try
            {
                TransactionHistoryRequest? baseRequest = BuildRequest();
                if (baseRequest == null)
                {
                    return;
                }

                ExportedFileResult? exportResult = await _transactionApiService.ExportTransactionsAsync(new TransactionExportRequest
                {
                    SearchTerm = baseRequest.SearchTerm,
                    FromDate = baseRequest.FromDate,
                    ToDate = baseRequest.ToDate,
                    TransactionType = baseRequest.TransactionType,
                    MinimumAmount = baseRequest.MinimumAmount,
                    MaximumAmount = baseRequest.MaximumAmount,
                    AccountId = baseRequest.AccountId,
                    CardId = baseRequest.CardId,
                    Status = baseRequest.Status,
                    Direction = baseRequest.Direction,
                    SortField = baseRequest.SortField,
                    SortDirection = baseRequest.SortDirection,
                    Format = format
                });

                if (exportResult == null)
                {
                    ShowStatus("Failed to export transaction history.", InfoBarSeverity.Error);
                    return;
                }

                LastExportPath = exportResult.FilePath;
                ShowStatus($"Export saved to {exportResult.FilePath}", InfoBarSeverity.Success);
            }
            catch (Exception ex)
            {
                ShowStatus($"Export failed: {ex.Message}", InfoBarSeverity.Error);
            }
        }

        private async Task DownloadReceiptAsync()
        {
            if (SelectedTransaction == null)
            {
                return;
            }

            try
            {
                ExportedFileResult? exportResult = await _transactionApiService.ExportReceiptAsync(SelectedTransaction.Id);
                if (exportResult == null)
                {
                    ShowStatus("Failed to download transaction receipt.", InfoBarSeverity.Error);
                    return;
                }

                LastExportPath = exportResult.FilePath;
                ShowStatus($"Receipt saved to {exportResult.FilePath}", InfoBarSeverity.Success);
            }
            catch (Exception ex)
            {
                ShowStatus($"Receipt download failed: {ex.Message}", InfoBarSeverity.Error);
            }
        }

        private TransactionHistoryRequest? BuildRequest()
        {
            if (!TryParseAmount(MinimumAmountInput, out decimal? minimumAmount) ||
                !TryParseAmount(MaximumAmountInput, out decimal? maximumAmount))
            {
                ShowStatus("Amounts must be valid decimal values.", InfoBarSeverity.Warning);
                return null;
            }

            return new TransactionHistoryRequest
            {
                SearchTerm = string.IsNullOrWhiteSpace(SearchTerm) ? null : SearchTerm.Trim(),
                FromDate = FromDate?.DateTime.Date,
                ToDate = ToDate?.DateTime.Date,
                TransactionType = SelectedTransactionType,
                MinimumAmount = minimumAmount,
                MaximumAmount = maximumAmount,
                AccountId = SelectedAccount?.Id,
                CardId = SelectedCard?.Id,
                Status = SelectedStatus,
                Direction = SelectedDirection,
                SortField = SelectedSortField,
                SortDirection = SelectedSortDirection
            };
        }

        private void RestoreState()
        {
            TransactionHistoryRequest? request = _sessionState.CurrentRequest;
            if (request == null)
            {
                return;
            }

            SearchTerm = request.SearchTerm ?? string.Empty;
            FromDate = request.FromDate.HasValue ? new DateTimeOffset(request.FromDate.Value) : null;
            ToDate = request.ToDate.HasValue ? new DateTimeOffset(request.ToDate.Value) : null;
            MinimumAmountInput = request.MinimumAmount?.ToString("0.##", CultureInfo.InvariantCulture) ?? string.Empty;
            MaximumAmountInput = request.MaximumAmount?.ToString("0.##", CultureInfo.InvariantCulture) ?? string.Empty;
            SelectedAccount = AccountOptions.FirstOrDefault(account => account.Id == request.AccountId);
            SelectedCard = CardOptions.FirstOrDefault(card => card.Id == request.CardId);
            SelectedTransactionType = request.TransactionType;
            SelectedStatus = request.Status;
            SelectedDirection = request.Direction;
            SelectedSortField = request.SortField;
            SelectedSortDirection = request.SortDirection;
        }

        private void PersistState(TransactionHistoryRequest request)
        {
            _sessionState.CurrentRequest = request;
            _sessionState.SelectedTransactionId = SelectedTransaction?.Id;
        }

        private void ShowStatus(string message, InfoBarSeverity severity)
        {
            StatusMessage = message;
            StatusSeverity = severity;
            IsStatusOpen = !string.IsNullOrWhiteSpace(message);
        }

        private static bool TryParseAmount(string input, out decimal? amount)
        {
            amount = null;
            if (string.IsNullOrWhiteSpace(input))
            {
                return true;
            }

            if (decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal parsedAmount))
            {
                amount = parsedAmount;
                return true;
            }

            return false;
        }

        private static void ReplaceCollection<T>(ObservableCollection<T> target, IEnumerable<T> source)
        {
            target.Clear();
            foreach (T item in source)
            {
                target.Add(item);
            }
        }

        private void RaiseSelectedTransactionChanged()
        {
            OnPropertyChanged(nameof(DetailsVisibility));
            OnPropertyChanged(nameof(EmptyDetailsVisibility));
            OnPropertyChanged(nameof(SelectedReferenceNumber));
            OnPropertyChanged(nameof(SelectedTimestampText));
            OnPropertyChanged(nameof(SelectedTransactionTypeText));
            OnPropertyChanged(nameof(SelectedCounterpartyText));
            OnPropertyChanged(nameof(SelectedDescriptionText));
            OnPropertyChanged(nameof(SelectedAmountText));
            OnPropertyChanged(nameof(SelectedCurrencyText));
            OnPropertyChanged(nameof(SelectedDirectionText));
            OnPropertyChanged(nameof(SelectedStatusText));
            OnPropertyChanged(nameof(SelectedRunningBalanceText));
            OnPropertyChanged(nameof(SelectedFeeText));
            OnPropertyChanged(nameof(SelectedExchangeRateText));
            OnPropertyChanged(nameof(SelectedSourceIbanText));
            OnPropertyChanged(nameof(SelectedDestinationIbanText));
            _downloadReceiptCommand.RaiseCanExecuteChanged();
        }

        private static string FormatDateTime(DateTime? value)
        {
            return value?.ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.CurrentCulture) ?? string.Empty;
        }

        private static string FormatDecimal(decimal? value)
        {
            return value?.ToString("0.00", CultureInfo.CurrentCulture) ?? string.Empty;
        }

        public override void Dispose()
        {
        }
    }
}
