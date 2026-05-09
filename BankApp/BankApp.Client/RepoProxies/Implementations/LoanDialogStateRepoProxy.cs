using System;
using System.Threading.Tasks;
using BankApp.Client.RepoProxies;
using BankApp.Client.RepoProxies.Interfaces;

namespace BankApp.Client.RepoProxies.Implementations
{
    public class LoanDialogStateRepoProxy : ILoanDialogStateRepoProxy
    {
        private readonly ApiService _apiService;

        public LoanDialogStateRepoProxy(ApiService apiService)
        {
            _apiService = apiService;
        }

        public Task<bool> GetShouldComputeEstimate(double desiredAmount, int preferredTermMonths, string purpose)
        {
            return _apiService.GetAsync<bool>(
                $"/api/loans/should-compute-estimate?desiredAmount={desiredAmount}&preferredTermMonths={preferredTermMonths}&purpose={Uri.EscapeDataString(purpose)}");
        }
    }
}
