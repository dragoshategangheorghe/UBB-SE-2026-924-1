// <copyright file="LoansViewModel.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

namespace BankApp.Client.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using BankApp.Client.Services.Interfaces;
    using BankApp.Client.Utilities;
    using BankApp.Models.DTOs.Loans;
    using BankApp.Models.Entities;
    using BankApp.Models.Enums;
    using BankApp.Models.Features.Loans;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;

    public partial class LoansViewModel : ObservableObject
    {
        private const string CustomAmountDisplayFormat = "0.##";
        private const int ZeroCount = 0;
        private const decimal ZeroAmount = 0m;
        private const int FirstPage = 1;
        private const int DefaultPageSize = 10;

        private readonly ILoansService loanService;
        private readonly PdfExporter pdfExporter;

        [ObservableProperty]
        private ObservableCollection<AmortizationRow> amortizationRows = new ObservableCollection<AmortizationRow>();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ApplicationWasApproved))]
        private string applicationResult = string.Empty;

        [ObservableProperty]
        private bool applicationWasApproved;

        [ObservableProperty]
        private LoanEstimate currentEstimate;

        [ObservableProperty]
        private double? customAmount;

        [ObservableProperty]
        private double desiredAmount;

        [ObservableProperty]
        private string dialogActionText = "Continue";

        [ObservableProperty]
        private string dialogTitle = "Apply for Loan";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasError))]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private bool isEstimateVisible;

        [ObservableProperty]
        private bool isFormVisible = true;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private bool isReviewVisible;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FilteredLoans))]
        private ObservableCollection<LoanViewModel> loans = new ObservableCollection<LoanViewModel>();

        [ObservableProperty]
        private decimal paymentPreviewBalance;

        [ObservableProperty]
        private int paymentPreviewRemainingMonths;

        [ObservableProperty]
        private int preferredTermMonths;

        [ObservableProperty]
        private string purpose = string.Empty;

        [ObservableProperty]
        private LoanViewModel selectedLoan;

        [ObservableProperty]
        private LoanType selectedLoanType;

        [NotifyPropertyChangedFor(nameof(FilteredLoans))]
        [ObservableProperty]
        private LoanStatus? statusFilter;

        [NotifyPropertyChangedFor(nameof(FilteredLoans))]
        [ObservableProperty]
        private LoanType? typeFilter;

        [ObservableProperty]
        private User currentUser;

        public LoansViewModel(ILoansService loanService)
        {
            this.loanService = loanService ?? throw new ArgumentNullException(nameof(loanService));
            this.pdfExporter = new PdfExporter();
        }

        public IEnumerable<LoanType> LoanTypes => Enum.GetValues<LoanType>();

        public bool HasError => !string.IsNullOrEmpty(this.ErrorMessage);

        public IEnumerable<LoanViewModel> FilteredLoans =>
            this.loans.Where(loan =>
                (this.statusFilter == null || loan.Loan.LoanStatus == this.statusFilter) &&
                (this.typeFilter == null || loan.Loan.LoanType == this.typeFilter));

        [RelayCommand]
        public async Task LoadLoansAsync()
        {
            this.IsLoading = true;
            this.ErrorMessage = string.Empty;
            try
            {
                var result = await this.loanService.GetLoansByUserAsync(CurrentUser.Id);
                var loanViewModels = new List<LoanViewModel>();
                foreach (var loan in result)
                {
                    var repaymentProgress = this.loanService.GetRepaymentProgress(loan);
                    loanViewModels.Add(new LoanViewModel(loan, repaymentProgress));
                }

                this.Loans = new ObservableCollection<LoanViewModel>(loanViewModels);
            }
            catch (Exception exception)
            {
                this.ErrorMessage = exception.Message;
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task ApplyForLoanAsync()
        {
            this.IsLoading = true;
            this.ErrorMessage = string.Empty;
            try
            {
                var request = new LoanApplicationRequest
                {
                    UserId = CurrentUser.Id,
                    LoanType = this.SelectedLoanType,
                    DesiredAmount = (decimal)this.DesiredAmount,
                    PreferredTermMonths = this.PreferredTermMonths,
                    Purpose = this.Purpose,
                };

                var applicationResult = await this.loanService.SubmitLoanApplicationAsync(request);

                var applicationOutcome = await this.loanService.GetBuildApplicationOutcomeAsync(
                    applicationResult.RejectionReason);
                this.ApplicationResult = applicationOutcome?.Message ?? string.Empty;
                this.ApplicationWasApproved = applicationOutcome?.IsApproved ?? false;
                if (this.ApplicationWasApproved)
                {
                    await this.LoadLoansAsync();
                    this.OnPropertyChanged(nameof(this.FilteredLoans));
                }
            }
            catch (Exception exception)
            {
                this.ErrorMessage = exception.Message;
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task ComputeLiveEstimate()
        {
            this.ErrorMessage = string.Empty;
            try
            {
                var request = new LoanApplicationRequest
                {
                    UserId = CurrentUser.Id,
                    LoanType = this.SelectedLoanType,
                    DesiredAmount = (decimal)this.DesiredAmount,
                    PreferredTermMonths = this.PreferredTermMonths,
                    Purpose = this.Purpose,
                };
                this.CurrentEstimate = this.loanService.GetLoanEstimate(request);
            }
            catch (Exception exception)
            {
                this.ErrorMessage = exception.Message;
            }
        }

        public async Task PayInstallmentAsync()
        {
            this.IsLoading = true;
            this.ErrorMessage = string.Empty;
            try
            {
                var amount = this.CustomAmount.HasValue
                    ? (decimal?)this.CustomAmount.Value
                    : null;
                await this.loanService.PayInstallmentAsync(this.SelectedLoan.Loan.Id, amount);
                await this.LoadLoansAsync();

                this.OnPropertyChanged(nameof(this.FilteredLoans));
            }
            catch (Exception exception)
            {
                this.ErrorMessage = exception.Message;
                throw;
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        public void UpdatePaymentPreview(bool isStandardPayment, string customAmountText = "")
        {
            if (this.SelectedLoan == null)
            {
                this.PaymentPreviewBalance = ZeroAmount;
                this.PaymentPreviewRemainingMonths = ZeroCount;
                return;
            }

            decimal? customAmount = null;
            if (!isStandardPayment)
            {
                customAmount = this.loanService.ParseCustomPaymentAmount(customAmountText);
            }

            var (balance, months) = CalculatePaymentPreview(this.SelectedLoan.Loan, customAmount);
            this.PaymentPreviewBalance = balance;
            this.PaymentPreviewRemainingMonths = months;
        }

        public void SelectStandardPayment()
        {
            this.CustomAmount = null;
            this.UpdatePaymentPreview(true);
        }

        public string SelectCustomPayment()
        {
            if (this.SelectedLoan == null)
            {
                this.CustomAmount = null;
                this.UpdatePaymentPreview(false, string.Empty);
                return string.Empty;
            }

            var normalizedCustomAmount = this.loanService.NormalizeCustomPaymentAmount(
                this.SelectedLoan.Loan,
                this.CustomAmount.HasValue ? (decimal?)this.CustomAmount.Value : null);

            this.CustomAmount = (double)normalizedCustomAmount;

            var currentText = normalizedCustomAmount.ToString(CustomAmountDisplayFormat, CultureInfo.CurrentCulture);
            this.UpdatePaymentPreview(false, currentText);
            return currentText;
        }

        public void UpdateCustomPayment(string customAmountText)
        {
            var parsedAmount = this.loanService.ParseCustomPaymentAmount(customAmountText);
            this.CustomAmount = parsedAmount.HasValue ? (double)parsedAmount.Value : null;
            this.UpdatePaymentPreview(false, customAmountText);
        }

        public async Task LoadAmortizationAsync()
        {
            if (this.SelectedLoan == null)
            {
                return;
            }

            this.IsLoading = true;
            this.ErrorMessage = string.Empty;
            try
            {
                var rows = await this.loanService.GetAmortizationAsync(this.SelectedLoan.Loan.Id);
                this.AmortizationRows = new ObservableCollection<AmortizationRow>(rows);
            }
            catch (Exception exception)
            {
                this.ErrorMessage = exception.Message;
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task DownloadSchedulePdfAsync()
        {
            try
            {
                var rows = await this.loanService.GetAmortizationAsync(this.SelectedLoan.Loan.Id);
                var pdfBytes = this.pdfExporter.ExportAmortization(rows);
                var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                var fileName = $"amortization_schedule_{this.SelectedLoan.Loan.Id}.pdf";
                var filePath = Path.Combine(desktopPath, fileName);

                await File.WriteAllBytesAsync(filePath, pdfBytes);

                Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true,
                    });
            }
            catch (Exception exception)
            {
                this.ErrorMessage = exception.Message;
            }
        }

        // --- Metode de control pentru Dialog ---
        public void SwitchToReviewStage()
        {
            this.IsFormVisible = false;
            this.IsReviewVisible = true;
            this.DialogTitle = "Application Review";
            this.DialogActionText = "Submit";
        }

        public void ResetDialogState()
        {
            this.IsFormVisible = true;
            this.IsReviewVisible = false;
            this.DialogTitle = "Apply for Loan";
            this.DialogActionText = "Continue";
            this.ApplicationResult = string.Empty;
            this.ApplicationWasApproved = false;

            this.DesiredAmount = default;
            this.PreferredTermMonths = default;
            this.Purpose = string.Empty;
            this.CurrentEstimate = null;
            this.IsEstimateVisible = false;
        }

        partial void OnDesiredAmountChanged(double value)
        {
            this.TryComputeEstimate();
        }

        partial void OnPreferredTermMonthsChanged(int value)
        {
            this.TryComputeEstimate();
        }

        partial void OnSelectedLoanTypeChanged(LoanType value)
        {
            this.TryComputeEstimate();
        }

        partial void OnPurposeChanged(string value)
        {
            this.TryComputeEstimate();
        }

        private void TryComputeEstimate()
        {
            _ = this.TryComputeEstimateAsync();
        }

        private async Task TryComputeEstimateAsync()
        {
            var isFullyFilled = await this.loanService.GetShouldComputeEstimateAsync(
                this.DesiredAmount,
                this.PreferredTermMonths,
                this.Purpose);

            if (isFullyFilled)
            {
                await this.ComputeLiveEstimate();
                this.IsEstimateVisible = true;
            }
            else
            {
                this.CurrentEstimate = null;
                this.IsEstimateVisible = false;
            }
        }

        private static (decimal BalanceAfterPayment, int RemainingMonths) CalculatePaymentPreview(
            Loan loan,
            decimal? customAmount = null)
        {
            var minimumDue = Math.Min(loan.MonthlyInstallment, loan.OutstandingBalance);
            var paymentAmount = customAmount ?? minimumDue;
            var balanceAfterPayment = Math.Max(ZeroAmount, loan.OutstandingBalance - paymentAmount);

            if (balanceAfterPayment <= ZeroAmount)
            {
                return (ZeroAmount, ZeroCount);
            }

            var monthsPaid = customAmount.HasValue
                ? paymentAmount <= ZeroAmount
                    ? ZeroCount
                    : (int)Math.Floor(paymentAmount / loan.MonthlyInstallment)
                : 1;
            var newRemainingMonths = Math.Max(ZeroCount, loan.RemainingMonths - monthsPaid);
            return (balanceAfterPayment, newRemainingMonths);
        }
    }
}
