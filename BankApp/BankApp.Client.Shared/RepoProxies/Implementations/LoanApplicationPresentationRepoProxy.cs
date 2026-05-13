using System;
using System.Threading.Tasks;
using BankApp.Client.RepoProxies;
using BankApp.Client.RepoProxies.Interfaces;
using BankApp.Models.DTOs.Loans;

namespace BankApp.Client.RepoProxies.Implementations
{
    public class LoanApplicationPresentationRepoProxy : ILoanApplicationPresentationRepoProxy
    {
        private readonly ApiService _apiService;

        public LoanApplicationPresentationRepoProxy(ApiService apiService)
        {
            _apiService = apiService;
        }

        public Task<BuildApplicationOutcomeResponse?> GetBuildApplicationOutcome(string? rejectionReason)
        {
            return _apiService.GetAsync<BuildApplicationOutcomeResponse>(
                $"/api/loans/loan-application-presentation-outcome?rejectionReason={Uri.EscapeDataString(rejectionReason ?? string.Empty)}");
        }
    }
}
