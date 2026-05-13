using System.Collections.Generic;
using System.Threading.Tasks;
using BankApp.Models.Features.Savings;

namespace BankApp.Client.RepoProxies.Interfaces
{
    public interface ISavingsPresentationRepoProxy
    {
        Task<string> GetTotalSavedAmount(IEnumerable<SavingsAccount> accounts);

        Task<string> GetNumberOfAccountsText(int accountCount);

        Task<string> GetBestInterestRate(IEnumerable<SavingsAccount> accounts);

        Task<bool> CheckClosePenaltyRisk(SavingsAccount selectedAccount);
    }
}
