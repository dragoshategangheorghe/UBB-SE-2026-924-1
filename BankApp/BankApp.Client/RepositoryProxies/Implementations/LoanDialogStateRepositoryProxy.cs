using BankApp.Client.RepositoryProxies.Interfaces;
using BankApp.Client.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApp.Client.RepositoryProxies.Implementations
{
    public class LoanDialogStateRepositoryProxy : ILoanDialogStateRepositoryProxy
    {
        private readonly ApiService apiService;

        public LoanDialogStateRepositoryProxy(ApiService apiService)
        {
            this.apiService = apiService;
        }

        public Task<bool> GetShouldComputeEstimateAsync(double desiredAmount, int preferredTermMonths, string purpose)
        {
            return this.apiService.GetAsync<bool>($"/api/loans/should-compute-estimate?desiredAmount={desiredAmount}&preferredTermMonths={preferredTermMonths}&purpose={purpose}");
        }
    }
}
