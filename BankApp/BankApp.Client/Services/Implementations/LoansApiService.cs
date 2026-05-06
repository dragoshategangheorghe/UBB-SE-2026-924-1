using BankApp.Client.Services.Interfaces;
using BankApp.Client.Utilities;
using BankApp.Models.DTOs.Loans;
using BankApp.Models.Enums;
using BankApp.Models.Features.Loans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApp.Client.Services.Implementations
{
    public class LoansApiService : ILoansApiService
    {
        private readonly ApiService _apiService;

        public LoansApiService(ApiService apiService)
        {
            _apiService = apiService;
        }

        public Task<List<Loan>> GetAllLoansAsync()
        {
            return _apiService.GetAsync<List<Loan>>("loans");
        }

        public Task<Loan> GetLoanByIdAsync(int id)
        {
            return _apiService.GetAsync<Loan>($"loans/{id}");
        }

        public Task<List<Loan>> GetLoansByUserAsync(int userId)
        {
            return _apiService.GetAsync<List<Loan>>($"loans/by-user/{userId}");
        }

        public Task<List<Loan>> GetLoansByStatusAsync(LoanStatus loanStatus)
        {
            return _apiService.GetAsync<List<Loan>>($"loans/by-status/{loanStatus}");
        }

        public Task<List<Loan>> GetLoansByTypeAsync(LoanType loanType)
        {
            return _apiService.GetAsync<List<Loan>>($"loans/by-type/{loanType}");
        }

        public Task<LoanApplicationResult> SubmitLoanApplicationAsync(LoanApplicationRequest request)
        {
            return _apiService.PostAsync<LoanApplicationRequest, LoanApplicationResult>("loans/apply", request);
        }

        public Task<LoanEstimate> GetLoanEstimateAsync(LoanApplicationRequest request)
        {
            return _apiService.PostAsync<LoanApplicationRequest, LoanEstimate>("loans/estimate", request);
        }

        public Task PayInstallmentAsync(int loanId, decimal? customAmount)
        {
            return _apiService.GetAsync<decimal>($"loans/{loanId}/pay-installment?customAmount={customAmount}");
        }

        public Task<decimal?> GetParsedCustomPaymentAmountAsync(string input)
        {
            return _apiService.GetAsync<decimal?>($"loans/payment-amount/{Uri.EscapeDataString(input)}");
        }
        
        public Task<decimal> NormalizeCustomPaymentAmountAsync(Loan loan, decimal? currentCustomAmount)
        {
            return _apiService.PostAsync<Loan, decimal>($"loans/normalize-payment-amount?currentCustomAmount={currentCustomAmount}", loan);
        }

        public Task<double> GetRepaymentProgressAsync(Loan loan)
        {
            return _apiService.PostAsync<Loan, double>("loans/repayment-progress", loan);
        }

        public Task<List<AmortizationRow>> GetAmortizationAsync(int loanId)
        {
            return _apiService.GetAsync<List<AmortizationRow>>($"loans/{loanId}/amortization-schedule");
        }
    }
}
