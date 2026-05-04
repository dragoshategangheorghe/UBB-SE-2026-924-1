using System.Collections.Generic;
using System.Threading.Tasks;
using BankApp.Models.DTOs.Savings;
using BankApp.Models.Features.Investments;
using BankApp.Models.Features.Savings;
using KarmaBanking.App.Models;
using KarmaBanking.App.Models.DTOs;


namespace BankApp.Server.Services.Interfaces
{
    public interface ISavingsService
    {
        Task<SavingsAccount> CreateAccountAsync(CreateSavingsAccountDto dto);

        Task<List<SavingsAccount>> GetAccountsAsync(int userId, bool includesClosed = false);

        Task<DepositResponseDto> DepositAsync(int accountId, decimal amount, string source, int userId);

        Task<ClosureResultDto> CloseAccountAsync(int accountId, int destinationAccountId, int userId);

        Task<WithdrawResponseDto> WithdrawAsync(int accountId, decimal amount, string destinationLabel, int userId);

        Task<AutoDeposit?> GetAutoDepositAsync(int accountId);

        Task SaveAutoDepositAsync(AutoDeposit autoDeposit);

        Task<List<FundingSourceOption>> GetFundingSourcesAsync(int userId);

        Task<(List<SavingsTransaction> Items, int TotalCount)> GetTransactionsAsync(
            int accountId,
            string filter,
            int page,
            int pageSize);

        Task<List<SavingsAccount>> GetValidTransferDestinationsAsync(int currentAccountId);

        decimal ComputeWithdrawalPenalty(decimal amount);

        bool HasRiskEarlyWithdrawal(SavingsAccount savingsAccount);

        decimal GetPenaltyDecimalFor(string penaltyCase);
    }
}