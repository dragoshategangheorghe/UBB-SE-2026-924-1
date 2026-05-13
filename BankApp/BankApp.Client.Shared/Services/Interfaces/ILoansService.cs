using System.Collections.Generic;
using System.Threading.Tasks;
using BankApp.Models.DTOs.Loans;
using BankApp.Models.Enums;
using BankApp.Models.Features.Loans;

namespace BankApp.Client.Services.Interfaces
{
    /// <summary>
    /// Desktop business service for Loans.
    /// Implements business rules locally and uses a RepoProxy for persistence.
    /// </summary>
    public interface ILoansService
    {
        Task<List<Loan>> GetLoansByUserAsync(int userId);

        Task<LoanApplicationResult> SubmitLoanApplicationAsync(LoanApplicationRequest request);

        LoanEstimate GetLoanEstimate(LoanApplicationRequest request);

        Task PayInstallmentAsync(int loanId, decimal? customAmount);

        decimal? ParseCustomPaymentAmount(string input);

        decimal NormalizeCustomPaymentAmount(Loan loan, decimal? currentCustomAmount);

        double GetRepaymentProgress(Loan loan);

        Task<List<AmortizationRow>> GetAmortizationAsync(int loanId);

        Task<BuildApplicationOutcomeResponse?> GetBuildApplicationOutcomeAsync(string? rejectionReason);

        Task<bool> GetShouldComputeEstimateAsync(double desiredAmount, int preferredTermMonths, string purpose);
    }

    public class LoanApplicationResult
    {
        public LoanApplicationStatus Status { get; set; }
        public string? RejectionReason { get; set; }
    }
}

