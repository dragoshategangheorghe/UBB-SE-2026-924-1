using BankApp.Client.RepositoryProxies.Interfaces;
using BankApp.Client.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApp.Client.RepositoryProxies.Implementations
{
    public class LoanApplicationPresentationRepositoryProxy : ILoanApplicationPresentationRepositoryProxy
    {
        private readonly ApiService apiService;

        public LoanApplicationPresentationRepositoryProxy(ApiService apiService)
        {
            this.apiService = apiService;
        }

        public Task<(bool, string)> GetBuildApplicationOutcomeAsync(string? rejectionReason)
        {
            string reason = rejectionReason ?? string.Empty;
            return this.apiService.GetAsync<(bool, string)>($"/api/loans/loan-application-presentation-outcome?rejectionReason={reason}");
        }
    }
}
