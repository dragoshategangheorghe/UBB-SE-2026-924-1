using BankApp.Client.Services.Interfaces;
using BankApp.Models.DTOs.Savings;
using BankApp.Models.Features.Investments;
using BankApp.Models.Features.Savings;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankApp.Client.Services.Interfaces
{
    public interface ISavingsService
    {
        Task<SavingsAccount> CreateAccountAsync(CreateSavingsAccountDto account);
        Task<List<SavingsAccount>> GetAccountsAsync(int userId, bool includesClosed = false);
        Task<DepositResponseDto> DepositAsync(int accountId, decimal amount, string source, int userId);
        Task<WithdrawResponseDto> WithdrawAsync(int accountId, decimal amount, string destinationLabel, int userId);
        Task<ClosureResultDto> CloseAccountAsync(int accountId, int destinationAccountId, int userId);
        Task<AutoDeposit> GetAutoDepositAsync(int accountId);
        Task SaveAutoDepositAsync(AutoDeposit autoDeposit);
        Task<List<FundingSourceOption>> GetFundingSourcesAsync(int userId);
        Task<GetTransactionsResponse> GetTransactionsAsync(int accountId, string filter = "", int page = 1, int pageSize = 20);
        Task<List<SavingsAccount>> GetValidTransferDestinationsAsync(int currentAccountId, int userId);

        Task<decimal> ComputeWithdrawalPenalty(decimal amount);
        Task<bool> HasRiskEarlyWithdrawal(SavingsAccount savingsAccount);
        Task<decimal> GetPenaltyDecimalFor(string penaltyCase);
    }
}

