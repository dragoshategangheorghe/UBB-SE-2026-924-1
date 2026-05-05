using BankApp.Models.DTOs.Loans;
using BankApp.Models.Enums;
using BankApp.Models.Features.Loans;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApp.Client.Services.Interfaces
{
    public interface ILoansApiService
    {
        Task<List<Loan>> GetAllLoansAsync();

        Task<Loan> GetLoanByIdAsync(int id);

        Task<List<Loan>> GetLoansByUserAsync(int userId);

        Task<List<Loan>> GetLoansByStatusAsync(LoanStatus loanStatus);

        Task<List<Loan>> GetLoansByTypeAsync(LoanType loanType);

        Task<LoanApplicationResult> SubmitLoanApplicationAsync(LoanApplicationRequest request);

        Task<LoanEstimate> GetLoanEstimateAsync(LoanApplicationRequest request);

        Task PayInstallmentAsync(int loanId, decimal? customAmount);

        Task<decimal?> GetParsedCustomPaymentAmountAsync(string input);

        Task<decimal> NormalizeCustomPaymentAmountAsync(Loan loan, decimal? currentCustomAmount);

        Task<double> GetRepaymentProgressAsync(Loan loan);

        Task<List<AmortizationRow>> GetAmortizationAsync(int loanId);
    }

    public class LoanApplicationResult
    {
        public LoanApplicationStatus Status { get; set; }
        public string? RejectionReason { get; set; }
    }
}
