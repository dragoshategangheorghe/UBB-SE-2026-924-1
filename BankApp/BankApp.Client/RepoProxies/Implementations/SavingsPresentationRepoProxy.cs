using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BankApp.Client.RepoProxies;
using BankApp.Client.RepoProxies.Interfaces;
using BankApp.Models.DTOs.Savings;
using BankApp.Models.Entities;
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
            var dto = new SavingsAccountSummaryDto
            {
                Balance = selectedAccount.Balance,
                AnnualPercentageYield = selectedAccount.AnnualPercentageYield,
                SavingsType = selectedAccount.SavingsType,
                MaturityDate = selectedAccount.MaturityDate,
            };
            return await _apiService.PostAsync<SavingsAccountSummaryDto, bool>("/api/savings-presentation/close-penalty-risk", dto);
        }

        public async Task<string> GetBestInterestRate(IEnumerable<SavingsAccount> accounts)
        {
            var dtos = accounts.Select(a => new SavingsAccountSummaryDto
            {
                Balance = a.Balance,
                AnnualPercentageYield = a.AnnualPercentageYield,
                SavingsType = a.SavingsType,
                MaturityDate = a.MaturityDate,
            });
            return await _apiService.PostAsync<IEnumerable<SavingsAccountSummaryDto>, string>("/api/savings-presentation/best-interest-rate", dtos);
        }

        public async Task<string> GetNumberOfAccountsText(int accountCount)
        {
            return await _apiService.GetAsync<string>($"/accounts-text/{accountCount}");
        }

        public async Task<string> GetTotalSavedAmount(IEnumerable<SavingsAccount> accounts)
        {
            var dtos = accounts.Select(a => new SavingsAccountSummaryDto
            {
                Balance = a.Balance,
                AnnualPercentageYield = a.AnnualPercentageYield,
                SavingsType = a.SavingsType,
                MaturityDate = a.MaturityDate,
            });
            return await _apiService.PostAsync<IEnumerable<SavingsAccountSummaryDto>, string>("/api/savings-presentation/total-saved", dtos);
        }
    }
}
