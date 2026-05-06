namespace BankApp.Client.Views
{
    using System;
    using BankApp.Client.ViewModels;
    using BankApp.Models.Features.Loans;
    using Microsoft.UI;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Media;
    using Microsoft.UI.Xaml.Navigation;

    public sealed partial class AmortizationScheduleView : Page
    {
        private const string LoanHeaderFormat = "{0} · {1} months · {2:0.##}%";
        private const byte CurrentRowHighlightAlpha = 40;
        private const byte CurrentRowHighlightRed = 0;
        private const byte CurrentRowHighlightGreen = 120;
        private const byte CurrentRowHighlightBlue = 215;

        private static readonly SolidColorBrush CurrentRowHighlightBrush = new SolidColorBrush(
            ColorHelper.FromArgb(
                CurrentRowHighlightAlpha,
                CurrentRowHighlightRed,
                CurrentRowHighlightGreen,
                CurrentRowHighlightBlue));

        private Loan? loan;

        public AmortizationScheduleView(LoansViewModel loansViewModel)
        {
            this.InitializeComponent();

            this.ViewModel = loansViewModel;
            this.DataContext = this.ViewModel;

            // Highlight the current installment row after containers are created.
            this.AmortizationListView.ContainerContentChanging += this.OnRowContainerContentChanging;
        }

        private LoansViewModel ViewModel { get; }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is Loan loan)
            {
                this.loan = loan;
                this.PopulateStaticLabels(loan);

                this.ViewModel.SelectedLoan = new LoanViewModel(loan, this.GetRepaymentProgress(loan));
                await this.ViewModel.LoadAmortizationAsync();
            }
        }

        private void PopulateStaticLabels(Loan loan)
        {
            this.LoanSubHeaderText.Text =
                string.Format(LoanHeaderFormat, loan.LoanType, loan.TermInMonths, loan.InterestRate);

            var loanViewModel = new LoanViewModel(loan, this.GetRepaymentProgress(loan));
            this.TotalInstallmentsText.Text = loan.TermInMonths.ToString();
            this.PaidInstallmentsText.Text = loanViewModel.PaidInstallments.ToString();
            this.RemainingInstallmentsText.Text = loan.RemainingMonths.ToString();
        }

        private double GetRepaymentProgress(Loan loan)
        {
            if (loan.Principal <= 0)
            {
                return 0;
            }

            var paidAmount = Math.Max(0m, loan.Principal - loan.OutstandingBalance);
            var progress = paidAmount / loan.Principal;
            return (double)Math.Clamp(progress, 0m, 1m);
        }

        private void OnRowContainerContentChanging(
            ListViewBase sender,
            ContainerContentChangingEventArgs args)
        {
            if (args.Item is AmortizationRow row && args.ItemContainer is ListViewItem container)
            {
                container.Background = row.IsCurrent
                    ? CurrentRowHighlightBrush
                    : null;
            }
        }

        private void OnBackClicked(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }

        private async void OnDownloadPdfClicked(object sender, RoutedEventArgs e)
        {
            if (this.loan != null)
            {
                await this.ViewModel.DownloadSchedulePdfAsync();
            }
        }
    }
}
