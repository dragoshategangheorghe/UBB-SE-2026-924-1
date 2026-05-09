using BankApp.Client.RepositoryProxies.Interfaces;
using BankApp.Client.Utilities;
using BankApp.Models.Features.Loans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApp.Client.RepositoryProxies.Implementations
{
    public class LoanPresentationRepositoryProxy : ILoanPresentationRepositoryProxy
    {
        private readonly ApiService apiService;

        public LoanPresentationRepositoryProxy(ApiService apiService)
        {
            this.apiService = apiService;
        }

        public Task<double> GetRepaymentProgressAsync(Loan loan)
        {
            return this.apiService.PostAsync<Loan, double>("/api/loans/repayment-progress", loan);
        }
    }
}
