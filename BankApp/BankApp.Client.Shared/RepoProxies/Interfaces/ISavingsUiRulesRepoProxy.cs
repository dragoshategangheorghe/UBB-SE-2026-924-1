using System.Collections.Generic;
using System.Threading.Tasks;
using BankApp.Models.DTOs.Savings;
using BankApp.Models.Enums;
using BankApp.Models.Features.Savings;

namespace BankApp.Client.RepoProxies.Interfaces
{
    public interface ISavingsUiRulesRepoProxy
    {
        Task<decimal> ParsePositiveAmount(string text);

        Task<string> GetDepositPreview(string depositAmountText, SavingsAccount selectedAccount);

        Task<decimal> GetWithdrawNetAmount(decimal requestedAmount, decimal penalty);

        Task<DepositFrequency> ParseDepositFrequency(string frequencyText);

        Task<int> GetTotalPages(int totalCount, int pageSize);

        Task<Dictionary<string, string>> ValidateCreateAccount(ValidateCreateAccountRequest request);
    }
}
