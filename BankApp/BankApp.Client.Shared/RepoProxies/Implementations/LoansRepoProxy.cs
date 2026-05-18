using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankApp.Client.RepoProxies;
using BankApp.Client.RepoProxies.Interfaces;
using BankApp.Models.DTOs.Loans;
using BankApp.Models.Enums;
using BankApp.Models.Features.Loans;

namespace BankApp.Client.RepoProxies.Implementations
{
    public class LoansRepoProxy : ILoansRepoProxy
    {
        private readonly ApiService _apiService;

        public LoansRepoProxy(ApiService apiService)
        {
            _apiService = apiService;
        }

        public Task<List<Loan>> GetAllLoansAsync()
        {
            return _apiService.GetAsync<List<Loan>>("/api/loans");
        }

        public Task<Loan> GetLoanByIdAsync(int id)
        {
            return _apiService.GetAsync<Loan>($"/api/loans/{id}");
        }

        public Task<List<Loan>> GetLoansByUserAsync(int userId)
        {
            return _apiService.GetAsync<List<Loan>>($"/api/loans/by-user/{userId}");
        }

        public Task<List<Loan>> GetLoansByStatusAsync(LoanStatus loanStatus)
        {
            return _apiService.GetAsync<List<Loan>>($"/api/loans/by-status/{loanStatus}");
        }

        public Task<List<Loan>> GetLoansByTypeAsync(LoanType loanType)
        {
            return _apiService.GetAsync<List<Loan>>($"/api/loans/by-type/{loanType}");
        }

        public async Task<int> CreateLoanApplicationAsync(LoanApplicationRequest request)
        {
            int result = await _apiService.PostAsync<LoanApplicationRequest, int>("/api/loans/applications", request);
            return result;
        }

        public async Task UpdateLoanApplicationStatusAsync(int applicationId, LoanApplicationStatus status, string? reason)
        {
            string reasonParam = reason == null ? string.Empty : $"&reason={Uri.EscapeDataString(reason)}";
            await _apiService.PutAsync<object, object>(
                $"/api/loans/applications/{applicationId}/status?status={status}{reasonParam}",
                new { });
        }

        public async Task<int> CreateLoanAsync(Loan loan)
        {
            int result = await _apiService.PostAsync<LoanCreateDto, int>("/api/loans", LoanCreateDto.FromLoan(loan));
            return result;
        }

        public async Task UpdateLoanAfterPaymentAsync(int loanId, decimal newBalance, int newRemainingMonths, LoanStatus newStatus)
        {
            await _apiService.PutAsync<object, object>(
                $"/api/loans/{loanId}/after-payment?newBalance={newBalance}&newRemainingMonths={newRemainingMonths}&newStatus={newStatus}",
                new { });
        }

        public Task<List<AmortizationRow>> GetAmortizationAsync(int loanId)
        {
            return _apiService.GetAsync<List<AmortizationRow>>($"/api/loans/{loanId}/amortization-schedule");
        }

        public async Task SaveAmortizationAsync(int loanId, List<AmortizationRow> rows)
        {
            await _apiService.PostAsync<List<AmortizationRowUpsertDto>, object>(
                $"/api/loans/{loanId}/amortization-schedule",
                rows.ConvertAll(AmortizationRowUpsertDto.FromAmortizationRow));
        }
    }
}
