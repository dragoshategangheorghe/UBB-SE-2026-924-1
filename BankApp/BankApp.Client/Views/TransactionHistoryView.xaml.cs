using BankApp.Client.ViewModels;
using BankApp.Models.DTOs.Transactions;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BankApp.Client.Views
{
    public sealed partial class TransactionHistoryView : Page
    {
        public TransactionHistoryView()
        {
            InitializeComponent();
            ViewModel = new TransactionHistoryViewModel(App.TransactionApiService, App.TransactionHistorySessionState);
            DataContext = ViewModel;
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            Loaded += TransactionHistoryView_Loaded;
            Unloaded += TransactionHistoryView_Unloaded;
        }

        public TransactionHistoryViewModel ViewModel { get; }

        private async void TransactionHistoryView_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.InitializeAsync();
            if (TransactionsList.Items.Count > 0 && TransactionsList.SelectedItem == null)
            {
                TransactionsList.SelectedIndex = 0;
            }

            TransactionHistoryItemDto? initialTransaction =
                TransactionsList.SelectedItem as TransactionHistoryItemDto ??
                ViewModel.SelectedTransaction ??
                ViewModel.Transactions.FirstOrDefault();

            if (initialTransaction != null)
            {
                TransactionsList.SelectedItem = initialTransaction;
            }

            SetSelectedTransaction(initialTransaction);
        }

        private void TransactionHistoryView_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            ViewModel.Dispose();
        }

        private void TransactionsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListView listView)
            {
                SetSelectedTransaction(listView.SelectedItem as TransactionHistoryItemDto);
            }
        }

        private void TransactionsList_ItemClick(object sender, ItemClickEventArgs e)
        {
            SetSelectedTransaction(e.ClickedItem as TransactionHistoryItemDto);
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TransactionHistoryViewModel.SelectedTransaction))
            {
                UpdateDetailsPanel(ViewModel.SelectedTransaction);
            }
        }

        private void SetSelectedTransaction(TransactionHistoryItemDto? transaction)
        {
            ViewModel.SelectedTransaction = transaction;
            UpdateDetailsPanel(transaction);
        }

        private void UpdateDetailsPanel(TransactionHistoryItemDto? transaction)
        {
            bool hasTransaction = transaction != null;
            EmptyDetailsText.Visibility = hasTransaction ? Visibility.Collapsed : Visibility.Visible;
            DetailsContentPanel.Visibility = hasTransaction ? Visibility.Visible : Visibility.Collapsed;
            DownloadReceiptButton.Visibility = hasTransaction ? Visibility.Visible : Visibility.Collapsed;

            ReferenceNumberText.Text = transaction?.ReferenceNumber ?? string.Empty;
            TimestampText.Text = FormatDateTime(transaction?.Timestamp);
            TransactionTypeText.Text = transaction?.TransactionType ?? string.Empty;
            CounterpartyText.Text = transaction?.CounterpartyOrMerchant ?? string.Empty;
            DescriptionText.Text = transaction?.Description ?? string.Empty;
            AmountText.Text = FormatDecimal(transaction?.Amount);
            CurrencyText.Text = transaction?.Currency ?? string.Empty;
            DirectionText.Text = transaction?.Direction ?? string.Empty;
            StatusText.Text = transaction?.Status ?? string.Empty;
            RunningBalanceText.Text = FormatDecimal(transaction?.RunningBalanceAfterTransaction);
            FeeText.Text = FormatDecimal(transaction?.Fee);
            ExchangeRateText.Text = transaction?.ExchangeRate?.ToString("0.000000", CultureInfo.CurrentCulture) ?? string.Empty;
            SourceIbanText.Text = transaction?.SourceAccountIban ?? string.Empty;
            DestinationIbanText.Text = transaction?.DestinationAccountIban ?? string.Empty;
        }

        private static string FormatDateTime(DateTime? value)
        {
            return value?.ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.CurrentCulture) ?? string.Empty;
        }

        private static string FormatDecimal(decimal? value)
        {
            return value?.ToString("0.00", CultureInfo.CurrentCulture) ?? string.Empty;
        }
    }
}
