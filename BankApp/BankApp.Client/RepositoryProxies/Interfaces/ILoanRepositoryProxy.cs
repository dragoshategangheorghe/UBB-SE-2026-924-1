using BankApp.Models.Enums;
using BankApp.Models.Features.Loans;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApp.Client.RepositoryProxies.Interfaces
{
    public interface ILoanRepositoryProxy
    {
        Task<List<Loan>?> GetAllLoansAsync();

        Task<Loan?> GetLoanByIdAsync(int id);

        Task<List<Loan>?> GetLoansByUserAsync(int userId);

        Task<List<Loan>?> GetLoansByStatusAsync(LoanStatus loanStatus);

        Task<List<Loan>?> GetLoansByTypeAsync(LoanType loanType);

        Task SaveAmortizationAsync(List<AmortizationRow> amortizationRows);

        Task UpdateLoanApplicationStatusAsync(int loanId, LoanApplicationStatus loanApplicationStatus, string? reason);

        // I don't think the return type is correct in the LoansController
        // Task<int> SubmitLoanApplicationAsync ?

        Task<int> CreateLoanAsync(Loan loan);

        Task<IActionResult?> UpdateLoanAfterPayment(int loanId, decimal newBalance, int newRemainingMonths, LoanStatus newLoanStatus);

        Task<List<AmortizationRow>?> GetAmortizationAsync(int loanId);
    }
}
