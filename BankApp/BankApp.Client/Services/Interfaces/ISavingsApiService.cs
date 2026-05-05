using BankApp.Models.DTOs.Savings;
using BankApp.Models.Features.Investments;
using BankApp.Models.Features.Savings;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApp.Client.Services.Interfaces
{
    public interface ISavingsApiService
    {
        SavingsAccount CreateAccountAsync(CreateSavingsAccountDto account);

        List<SavingsAccount> GetAccountsAsync(int userId, bool includesClosed = false);

        DepositResponseDto DepositAsync(int accountId, decimal amount, string source, int userId);

        WithdrawResponseDto WithdrawAsync(int accountId, decimal amount, string destinationLabel, int userId);

        ClosureResultDto CloseAccountAsync(int accountId, int destinationAccountId, int userId);

        AutoDeposit GetAutoDepositAsync(int accountId);

        void SaveAutoDepositAsync(AutoDeposit autoDeposit);

        List<FundingSourceOption> GetFundingSourcesAsync(int userId);

        GetTransactionsResponse GetTransactionsAsync(int accountId, string filter = "", int page = 1, int pageSize = 20);

        List<SavingsAccount> GetValidTransferDestinationsAsync(int currentAccountId);

        decimal ComputeWithdrawalPenalty(decimal amount);

        bool HasRiskEarlyWithdrawal(SavingsAccount savingsAccount);

        decimal GetPenaltyDecimalFor(string penaltyCase);
    }

    public class GetTransactionsResponse
    {
        public List<SavingsTransaction> Items { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
