using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankApp.Server.Services.Interfaces
{
    public interface ILoanService
    {
        Task<List<Loan>> GetAllLoansAsync();

        Task<Loan> GetLoanByIdAsync(int id);

        Task<List<Loan>> GetLoansByUserAsync(int userId);

        Task<List<Loan>> GetLoansByStatusAsync(LoanStatus loanStatus);

        Task<List<Loan>> GetLoansByTypeAsync(LoanType loanType);

        Task<(LoanApplicationStatus approved, string? reason)> ProcessApplicationStatusAsync(LoanApplication application);

        Task<LoanApplication> ApplyForLoanAsync(LoanApplicationRequest request);

        Task<(LoanApplicationStatus Status, string? RejectionReason)> SubmitLoanApplicationAsync(LoanApplicationRequest request);

        LoanEstimate GetLoanEstimate(LoanApplicationRequest request);

        Task<int> AddLoanAsync(LoanApplication application);

        Task PayInstallmentAsync(int loanId, decimal? amount = null);

        (decimal BalanceAfterPayment, int RemainingMonths) CalculatePaymentPreview(Loan loan, decimal? customAmount = null);

        decimal? ParseCustomPaymentAmount(string input);

        decimal NormalizeCustomPaymentAmount(Loan loan, decimal? currentCustomAmount);

        double GetRepaymentProgress(Loan loan);

        Task<List<AmortizationRow>> GetAmortizationAsync(int loanId);

        Task SaveAmortizationAsync(List<AmortizationRow> rows);

        Task GenerateAmortizationAsync(int loanId);
    }
}