using BankApp.Client.Services.Interfaces;
using BankApp.Client.Utilities;
using BankApp.Models.Features.Loans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApp.Client.Services.Implementations
{
    public class LoanPresentationApiService : ILoanPresentationApiService
    {
        private readonly ApiService _apiService;

        public LoanPresentationApiService(ApiService apiService)
        {
            _apiService = apiService;
        }

        public Task<decimal> GetRepaymentProgress(Loan loan)
        {
            return _apiService.PostAsync<Loan, decimal>("api/loans/repayment-progress", loan);
        }
    }
}
