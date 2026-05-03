using BankApp.Client.ViewModels;
using System.ComponentModel;
using System.Globalization;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BankApp.Client.Views
{
    public sealed partial class StatisticsView : Page
    {
        public StatisticsView()
        {
            InitializeComponent();
            ViewModel = new StatisticsViewModel(App.StatisticsApiService);
            DataContext = ViewModel;
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            Loaded += StatisticsView_Loaded;
            Unloaded += StatisticsView_Unloaded;
        }

        public StatisticsViewModel ViewModel { get; }

        private async void StatisticsView_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.LoadAsync();
            UpdateSummaryValues();
        }

        private void StatisticsView_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            ViewModel.Dispose();
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(StatisticsViewModel.Income) ||
                e.PropertyName == nameof(StatisticsViewModel.Expenses) ||
                e.PropertyName == nameof(StatisticsViewModel.Net) ||
                e.PropertyName == nameof(StatisticsViewModel.TotalSpending))
            {
                UpdateSummaryValues();
            }
        }

        private void UpdateSummaryValues()
        {
            IncomeValueText.Text = FormatCurrency(ViewModel.Income);
            ExpensesValueText.Text = FormatCurrency(ViewModel.Expenses);
            NetValueText.Text = FormatCurrency(ViewModel.Net);
            TotalSpendingText.Text = $"Total spending: {FormatCurrency(ViewModel.TotalSpending)}";
        }

        private static string FormatCurrency(decimal value)
        {
            return value.ToString("0.00", CultureInfo.CurrentCulture);
        }
    }
}
