using BankApp.Client.RepositoryProxies.Interfaces;
using BankApp.Client.Utilities;
using BankApp.Models.Features.Loans;
using BankApp.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BankApp.Models.DTOs.Profile;

namespace BankApp.Client.RepositoryProxies.Implementations
{
    public class LoanRepositoryProxy : ILoanRepositoryProxy
    {
        private readonly ApiService apiService;

        public LoanRepositoryProxy(ApiService apiService)
        {
            this.apiService = apiService;
        }

        public Task<List<Loan>?> GetAllLoansAsync()
        {
            return this.apiService.GetAsync<List<Loan>>("/api/loans");
        }

        public Task<Loan?> GetLoanByIdAsync(int id)
        {
            return this.apiService.GetAsync<Loan>($"/api/loans/{id}");
        }

        public Task<List<Loan>?> GetLoansByUserAsync(int userId)
        {
            return this.apiService.GetAsync<List<Loan>>($"/api/loans/by-user/{userId}");
        }

        public Task<List<Loan>?> GetLoansByStatusAsync(LoanStatus loanStatus)
        {
            return this.apiService.GetAsync<List<Loan>>($"/api/loans/by-status/{loanStatus.ToString()}");
        }

        public Task<List<Loan>?> GetLoansByTypeAsync(LoanType loanType)
        {
            return this.apiService.GetAsync<List<Loan>>($"/api/loans/by-type/{loanType.ToString()}");
        }

        // idk if the response type is good here, the API doesn't really return anything
        //  other than an OK response
        public Task SaveAmortizationAsync(List<AmortizationRow> amortizationRows)
        {
            return this.apiService.PutAsync<List<AmortizationRow>, IActionResult>("/api/loans/saveAmortization", amortizationRows);
        }

        public Task UpdateLoanApplicationStatusAsync(int loanId, LoanApplicationStatus loanApplicationStatus, string? reason)
        {
            string reasonProcessed = reason ?? string.Empty;
            // idk what the request type is supposed to be, it doesn't matter anyway for now
            return this.apiService.PutAsync<UpdateProfileRequest, Task>($"/api/loans/{loanId}/updateLoanApplicationStatus?loanApplicationStatus={loanApplicationStatus.ToString()}&reason={reasonProcessed}", new UpdateProfileRequest { });
        }

        public Task<int> CreateLoanAsync(Loan loan)
        {
            return this.apiService.PostAsync<Loan, int>("/api/loans/apply", loan);
        }

        public Task<IActionResult?> UpdateLoanAfterPayment(int loanId, decimal newBalance, int newRemainingMonths, LoanStatus newLoanStatus)
        {
            return this.apiService.PutAsync<UpdateProfileRequest, IActionResult>($"/api/loans/{loanId}/pay-installment?newBalance={newBalance}&newRemainingMonths={newRemainingMonths}&newLoanStatus={newLoanStatus.ToString()}", new UpdateProfileRequest { });
        }

        public Task<List<AmortizationRow>?> GetAmortizationAsync(int loanId)
        {
            return this.apiService.GetAsync<List<AmortizationRow>>($"/api/loans/{loanId}/amortization");
        }
    }
}
