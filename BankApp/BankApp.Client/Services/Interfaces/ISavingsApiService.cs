using BankApp.Models.DTOs.Savings;
using BankApp.Models.Features.Investments;
using BankApp.Models.Features.Savings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApp.Client.Services.Interfaces
{
    public interface ISavingsApiService
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

        Task<List<SavingsAccount>> GetValidTransferDestinationsAsync(int currentAccountId);

        Task<decimal> ComputeWithdrawalPenalty(decimal amount);

        Task<bool> HasRiskEarlyWithdrawal(SavingsAccount savingsAccount);

        Task<decimal> GetPenaltyDecimalFor(string penaltyCase);
    }

    public class GetTransactionsResponse
    {
        public List<SavingsTransaction> Items { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
