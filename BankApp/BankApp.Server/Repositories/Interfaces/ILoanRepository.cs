using System.Collections.Generic;
using System.Threading.Tasks;
using BankApp.App.Models.Enums;
using BankApp.Models.Features.Loans;
using BankApp.Models.DTOs.Loans;
using BankApp.Models.Enums;

namespace BankApp.Server.Repositories.Interfaces
{
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