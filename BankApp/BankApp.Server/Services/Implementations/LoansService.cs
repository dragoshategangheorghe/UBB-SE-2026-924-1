using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BankApp.Models.DTOs.Loans;
using BankApp.Models.Enums;
using BankApp.Models.Features.Loans;
using BankApp.Server.Repositories.Interfaces;
using BankApp.Server.Services.Interfaces;
using BankApp.Server.Utilities;

namespace BankApp.Server.Services.Implementations
{
    public class LoanService : ILoanService
    {
        private const int MinimumIdExclusive = 0;
        private const decimal ZeroAmount = 0m;
        private const int NoRowsCount = 0;
        private const int MaxActiveLoans = 5;
        private const decimal TotalDebtLimit = 200000m;
        private const decimal PersonalLoanRate = 8.5m;
        private const decimal MortgageLoanRate = 4.5m;
        private const decimal StudentLoanRate = 3.0m;
        private const decimal AutoLoanRate = 6.5m;

        private readonly ILoanRepository _loanRepository;
        private readonly LoanApplicationValidator _validator;
        private readonly PaymentCalculationService _paymentCalculationService;

        public LoanService(ILoanRepository loanRepository)
        {
            this._loanRepository = loanRepository;
            this._validator = new LoanApplicationValidator();
            this._paymentCalculationService = new PaymentCalculationService();
        }

        public async Task<List<Loan>> GetAllLoansAsync()
        {
            return await this._loanRepository.GetAllLoansAsync();
        }

        public async Task<Loan> GetLoanByIdAsync(int id)
        {
            if (id <= MinimumIdExclusive)
            {
                return new Loan();
            }

            return await this._loanRepository.GetLoanByIdAsync(id);
        }

        public async Task<List<Loan>> GetLoansByUserAsync(int userId)
        {
            if (userId <= MinimumIdExclusive)
            {
                return new List<Loan>();
            }

            return await this._loanRepository.GetLoansByUserAsync(userId);
        }

        public async Task<List<Loan>> GetLoansByStatusAsync(LoanStatus loanStatus)
        {
            return await this._loanRepository.GetLoansByStatusAsync(loanStatus);
        }

        public async Task<List<Loan>> GetLoansByTypeAsync(LoanType loanType)
        {
            return await this._loanRepository.GetLoansByTypeAsync(loanType);
        }

        public async Task<LoanApplication> ApplyForLoanAsync(LoanApplicationRequest request)
        {
            this._validator.Validate(request);

            var application = new LoanApplication
            {
                UserId = request.UserId,
                LoanType = request.LoanType,
                DesiredAmount = request.DesiredAmount,
                PreferredTermMonths = request.PreferredTermMonths,
                Purpose = request.Purpose,
                ApplicationStatus = LoanApplicationStatus.Pending,
                RejectionReason = string.Empty,
            };

            var appId = await this._loanRepository.CreateLoanApplicationAsync(request);
            application.UserId = appId;

            return application;
        }

        public async Task<(LoanApplicationStatus Status, string? RejectionReason)> SubmitLoanApplicationAsync(LoanApplicationRequest request)
        {
            var newApplication = await this.ApplyForLoanAsync(request);
            var (status, rejectionReason) = await this.ProcessApplicationStatusAsync(newApplication);

            if (status == LoanApplicationStatus.Approved)
            {
                var loanId = await this.AddLoanAsync(newApplication);
                await this.GenerateAmortizationAsync(loanId);
            }

            return (status, rejectionReason);
        }

        public async Task<(LoanApplicationStatus approved, string? reason)> ProcessApplicationStatusAsync(LoanApplication application)
        {
            var (status, reason) = await this.EvaluateApplicationAsync(application);

            await this._loanRepository.UpdateLoanApplicationStatusAsync(application.UserId, status, reason);

            return (status, reason);
        }

        public async Task<int> AddLoanAsync(LoanApplication application)
        {
            var rate = this.GetInterestRateForType(application.LoanType);
            var estimate = AmortizationCalculator.ComputeEstimate(
                application.DesiredAmount,
                rate,
                application.PreferredTermMonths);

            var loan = new Loan
            {
                UserId = application.UserId,
                LoanType = application.LoanType,
                Principal = application.DesiredAmount,
                OutstandingBalance = application.DesiredAmount,
                InterestRate = rate,
                MonthlyInstallment = estimate.MonthlyInstallment,
                RemainingMonths = application.PreferredTermMonths,
                LoanStatus = LoanStatus.Active,
                TermInMonths = application.PreferredTermMonths,
                StartDate = DateTime.Now,
            };

            return await this._loanRepository.CreateLoanAsync(loan);
        }

        public LoanEstimate GetLoanEstimate(LoanApplicationRequest request)
        {
            this._validator.Validate(request);

            var rate = this.GetInterestRateForType(request.LoanType);

            return AmortizationCalculator.ComputeEstimate(
                request.DesiredAmount,
                rate,
                request.PreferredTermMonths);
        }

        public async Task PayInstallmentAsync(int loanId, decimal? customAmount)
        {
            var loan = await this._loanRepository.GetLoanByIdAsync(loanId);

            if (loan == null)
            {
                throw new InvalidOperationException("Loan not found.");
            }

            if (loan.RemainingMonths <= MinimumIdExclusive || loan.LoanStatus == LoanStatus.Passed)
            {
                throw new InvalidOperationException("This loan is already closed.");
            }

            var paymentAmount = customAmount ?? loan.MonthlyInstallment;

            if (paymentAmount <= ZeroAmount)
            {
                throw new ArgumentException("Payment amount must be greater than zero.");
            }

            if (customAmount.HasValue && paymentAmount < loan.MonthlyInstallment)
            {
                throw new InvalidOperationException("Payment amount must be at least the minimum installment.");
            }

            if (paymentAmount > loan.OutstandingBalance)
            {
                throw new InvalidOperationException("Payment amount exceeds the outstanding balance.");
            }

            var (newBalance, newRemainingMonths) = this.CalculatePaymentPreview(loan, customAmount);

            var newStatus = newBalance <= ZeroAmount || newRemainingMonths == MinimumIdExclusive
                ? LoanStatus.Passed
                : loan.LoanStatus;

            await this._loanRepository.UpdateLoanAfterPaymentAsync(loan.UserId, newBalance, newRemainingMonths, newStatus);
        }

        public (decimal BalanceAfterPayment, int RemainingMonths) CalculatePaymentPreview(Loan loan, decimal? customAmount = null)
        {
            var isStandardPayment = !customAmount.HasValue;
            var customPaymentAmount = customAmount ?? ZeroAmount;

            return this._paymentCalculationService.CalculatePaymentPreview(
                loan.MonthlyInstallment,
                loan.OutstandingBalance,
                loan.RemainingMonths,
                isStandardPayment,
                customPaymentAmount);
        }

        public decimal? ParseCustomPaymentAmount(string input)
        {
            var (success, amount) = this._paymentCalculationService.ParsePaymentAmount(input);
            return success ? amount : null;
        }

        public decimal NormalizeCustomPaymentAmount(Loan loan, decimal? currentCustomAmount)
        {
            return this._paymentCalculationService.GetInitialCustomAmount(
                loan.MonthlyInstallment,
                loan.OutstandingBalance,
                currentCustomAmount.HasValue ? (double?)currentCustomAmount.Value : null);
        }

        public double GetRepaymentProgress(Loan loan)
        {
            return (double)AmortizationCalculator.ComputeRepaymentProgress(loan.Principal, loan.OutstandingBalance);
        }

        public async Task<List<AmortizationRow>> GetAmortizationAsync(int loanId)
        {
            var rows = await this._loanRepository.GetAmortizationAsync(loanId);

            if (rows == null || rows.Count == NoRowsCount)
            {
                await this.GenerateAmortizationAsync(loanId);
                rows = await this._loanRepository.GetAmortizationAsync(loanId);
            }

            var isCurrentSet = false;
            foreach (var row in rows)
            {
                if (!isCurrentSet && row.DueDate.Date >= DateTime.Today)
                {
                    row.IsCurrent = true;
                    isCurrentSet = true;
                }
                else
                {
                    row.IsCurrent = false;
                }
            }

            return rows;
        }

        public async Task SaveAmortizationAsync(List<AmortizationRow> rows)
        {
            await this._loanRepository.SaveAmortizationAsync(rows);
        }

        public async Task GenerateAmortizationAsync(int loanId)
        {
            var loan = await this._loanRepository.GetLoanByIdAsync(loanId);
            var rows = AmortizationCalculator.Generate(loan);
            await this._loanRepository.SaveAmortizationAsync(rows);
        }

        private async Task<(LoanApplicationStatus approved, string? reason)> EvaluateApplicationAsync(LoanApplication application)
        {
            var currentLoans = await this._loanRepository.GetLoansByUserAsync(application.UserId);

            var totalOutstanding = currentLoans.Sum(loan => loan.OutstandingBalance);
            var activeLoansCount = currentLoans.Count(loan => loan.LoanStatus == LoanStatus.Active);

            if (activeLoansCount >= MaxActiveLoans)
            {
                return (LoanApplicationStatus.Rejected, "Maximum number of active loans reached.");
            }

            if (totalOutstanding + application.DesiredAmount >= TotalDebtLimit)
            {
                return (LoanApplicationStatus.Rejected, "Total debt limit exceeded.");
            }

            return (LoanApplicationStatus.Approved, null);
        }

        private decimal GetInterestRateForType(LoanType loanType)
        {
            return loanType switch
            {
                LoanType.Personal => PersonalLoanRate,
                LoanType.Mortgage => MortgageLoanRate,
                LoanType.Student => StudentLoanRate,
                LoanType.Auto => AutoLoanRate,
            };
        }
    }
}