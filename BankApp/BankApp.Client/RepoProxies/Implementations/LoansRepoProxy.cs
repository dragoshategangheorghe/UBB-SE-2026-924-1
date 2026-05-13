using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
            await _apiService.PutVoidAsync<object>(
                $"/api/loans/applications/{applicationId}/status?status={status}{reasonParam}",
                new { });
        }

        public async Task<int> CreateLoanAsync(Loan loan)
        {
            int result = await _apiService.PostAsync<Loan, int>("/api/loans", loan);
            return result;
        }

        public async Task UpdateLoanAfterPaymentAsync(int loanId, decimal newBalance, int newRemainingMonths, LoanStatus newStatus)
        {
            var newBalanceText = newBalance.ToString(CultureInfo.InvariantCulture);

            await _apiService.PutVoidAsync<object>(
                $"/api/loans/{loanId}/after-payment?newBalance={newBalanceText}&newRemainingMonths={newRemainingMonths}&newStatus={newStatus}",
                new { });
        }

        public Task<List<AmortizationRow>?> GetAmortizationAsync(int loanId)
        {
            return _apiService.GetAsync<List<AmortizationRow>>($"/api/loans/{loanId}/amortization-schedule");
        }

        public async Task SaveAmortizationAsync(int loanId, List<AmortizationRow> rows)
        {
            var dtos = rows.Select(r => new AmortizationRowDto
            {
                InstallmentNumber = r.InstallmentNumber,
                DueDate = r.DueDate,
                PrincipalPortion = r.PrincipalPortion,
                InterestPortion = r.InterestPortion,
                RemainingBalance = r.RemainingBalance,
            }).ToList();
            await _apiService.PostVoidAsync<List<AmortizationRowDto>>($"/api/loans/{loanId}/amortization-schedule", dtos);
        }
    }
}
