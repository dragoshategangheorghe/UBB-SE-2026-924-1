using System.Collections.Generic;
using System.Threading.Tasks;
using BankApp.Models.Features.Loans;
using BankApp.Models.DTOs.Loans;
using BankApp.Models.Enums;

namespace BankApp.Server.Repositories.Interfaces
{
    public interface ILoanRepository
    {
        /// <summary>Gets all loans.</summary>
        /// <returns>The complete loan list.</returns>
        Task<List<Loan>> GetAllLoansAsync();

        /// <summary>Gets a loan by identifier.</summary>
        /// <param name="id">The loan identifier.</param>
        /// <returns>The matching loan, if found.</returns>
        Task<Loan> GetLoanByIdAsync(int id);

        /// <summary>Gets loans for a user.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The user's loans.</returns>
        Task<List<Loan>> GetLoansByUserAsync(int userId);

        /// <summary>Gets loans by status.</summary>
        /// <param name="loanStatus">The status filter.</param>
        /// <returns>The matching loans.</returns>
        Task<List<Loan>> GetLoansByStatusAsync(LoanStatus loanStatus);

        /// <summary>Gets loans by type.</summary>
        /// <param name="loanType">The type filter.</param>
        /// <returns>The matching loans.</returns>
        Task<List<Loan>> GetLoansByTypeAsync(LoanType loanType);

        /// <summary>Saves an amortization schedule for a loan.</summary>
        /// <param name="rows">The amortization rows to persist.</param>
        /// <returns>A task that completes when saving finishes.</returns>
        Task SaveAmortizationAsync(List<AmortizationRow> rows);

        /// <summary>Gets the amortization schedule for a loan.</summary>
        /// <param name="loanId">The loan identifier.</param>
        /// <returns>The amortization rows for the loan.</returns>
        Task<List<AmortizationRow>> GetAmortizationAsync(int loanId);

        /// <summary>Creates a loan application.</summary>
        /// <param name="request">The application payload.</param>
        /// <returns>The created application identifier.</returns>
        Task<int> CreateLoanApplicationAsync(LoanApplicationRequest request);

        /// <summary>Updates loan application decision status.</summary>
        /// <param name="id">The loan application identifier.</param>
        /// <param name="loanApplicationStatus">The updated status.</param>
        /// <param name="reason">The optional rejection reason.</param>
        /// <returns>A task that completes when the update is applied.</returns>
        Task UpdateLoanApplicationStatusAsync(int id, LoanApplicationStatus loanApplicationStatus, string? reason);

        /// <summary>Creates a new approved loan record.</summary>
        /// <param name="loan">The loan payload.</param>
        /// <returns>The created loan identifier.</returns>
        Task<int> CreateLoanAsync(Loan loan);

        /// <summary>Updates balance and status after payment.</summary>
        /// <param name="id">The loan identifier.</param>
        /// <param name="newBalance">The new outstanding balance.</param>
        /// <param name="newRemainingMonths">The updated remaining months.</param>
        /// <param name="newStatus">The updated status.</param>
        /// <returns>A task that completes when the update is applied.</returns>
        Task UpdateLoanAfterPaymentAsync(int id, decimal newBalance, int newRemainingMonths, LoanStatus newStatus);
    }
}