using System.Collections.Generic;
using System.Threading.Tasks;
using BankApp.Client.RepoProxies;
using BankApp.Client.RepoProxies.Interfaces;
using BankApp.Models.Features.Savings;

namespace BankApp.Client.RepoProxies.Implementations
{
    public class SavingsPresentationRepoProxy : ISavingsPresentationRepoProxy
    {
        private readonly ApiService _apiService;

        public SavingsPresentationRepoProxy(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<bool> CheckClosePenaltyRisk(SavingsAccount selectedAccount)
        {
            return await _apiService.PostAsync<SavingsAccount, bool>("/api/savings-presentation/close-penalty-risk", selectedAccount);
        }

        public async Task<string> GetBestInterestRate(IEnumerable<SavingsAccount> accounts)
        {
            return await _apiService.PostAsync<IEnumerable<SavingsAccount>, string>("/api/savings-presentation/best-interest-rate", accounts);
        }

        public async Task<string> GetNumberOfAccountsText(int accountCount)
        {
            return await _apiService.GetAsync<string>($"/accounts-text/{accountCount}");
        }

        public async Task<string> GetTotalSavedAmount(IEnumerable<SavingsAccount> accounts)
        {
            return await _apiService.PostAsync<IEnumerable<SavingsAccount>, string>("/api/savings-presentation/total-saved", accounts);
        }
    }
}
