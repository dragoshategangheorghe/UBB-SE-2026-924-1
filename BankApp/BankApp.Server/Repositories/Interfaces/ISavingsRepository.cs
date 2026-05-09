using System.Collections.Generic;
using System.Threading.Tasks;
using BankApp.Models;
using BankApp.Models.DTOs.Savings;
using BankApp.Models.Features.Savings;
using BankApp.Models.Features.Investments;

namespace BankApp.Server.Repositories.Interfaces
{
    /// <summary>
    /// Defines persistence operations for savings accounts and transactions.
    /// </summary>
    public interface ISavingsRepository
    {
        /// <summary>
        /// Creates a new savings account.
        /// </summary>
        /// <param name="dataTransferObject">The create-account request payload.</param>
        /// <param name="annualPercentageYield">The APY assigned to the new account.</param>
        /// <returns>The created savings account.</returns>
        Task<SavingsAccount> CreateSavingsAccountAsync(CreateSavingsAccountDto dataTransferObject, decimal annualPercentageYield);

        /// <summary>
        /// Gets savings accounts for a user.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="includesClosedAccounts">Whether to include closed accounts.</param>
        /// <returns>The matching savings accounts.</returns>
        Task<List<SavingsAccount>> GetSavingsAccountsByUserIdAsync(int userId, bool includesClosedAccounts = false);

        /// <summary>
        /// Deposits funds into a savings account.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="amount">The amount to deposit.</param>
        /// <param name="source">The source label for the deposit.</param>
        /// <returns>The deposit operation result.</returns>
        Task<DepositResponseDto> DepositAsync(int accountId, decimal amount, string source);

        /// <summary>
        /// Closes a savings account and transfers remaining funds.
        /// </summary>
        /// <param name="accountId">The source account identifier.</param>
        /// <param name="destinationAccountId">The destination account identifier.</param>
        /// <param name="transferAmount">The amount transferred out.</param>
        /// <param name="earlyClosurePenalty">The applied early-closure penalty.</param>
        /// <returns>The closure operation result.</returns>
        Task<ClosureResultDto> CloseSavingsAccountAsync(
            int accountId,
            int destinationAccountId,
            decimal transferAmount,
            decimal earlyClosurePenalty);

        /// <summary>
        /// Withdraws funds from a savings account.
        /// </summary>
        /// <param name="accountId">The source account identifier.</param>
        /// <param name="amount">The withdrawal amount.</param>
        /// <param name="destinationLabel">The destination label.</param>
        /// <param name="earlyWithdrawalPenalty">The applied early-withdrawal penalty.</param>
        /// <returns>The withdrawal operation result.</returns>
        Task<WithdrawResponseDto> WithdrawAsync(
            int accountId,
            decimal amount,
            string destinationLabel,
            decimal earlyWithdrawalPenalty);

        /// <summary>
        /// Gets recurring auto-deposit settings for an account.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns>The auto-deposit configuration, or <see langword="null"/>.</returns>
        Task<AutoDeposit?> GetAutoDepositAsync(int accountId);

        /// <summary>
        /// Creates or updates auto-deposit settings.
        /// </summary>
        /// <param name="autoDeposit">The auto-deposit payload.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        Task SaveAutoDepositAsync(AutoDeposit autoDeposit);

        /// <summary>
        /// Gets available funding sources for the user.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The available funding source _options.</returns>
        Task<List<FundingSourceOption>> GetFundingSourcesAsync(int userId);

        /// <summary>
        /// Gets paged transaction history and total count.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="typeFilter">The type filter value.</param>
        /// <param name="page">The one-based page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <returns>A tuple containing page items and total row count.</returns>
        Task<(List<SavingsTransaction> Items, int TotalCount)> GetTransactionsPagedAsync(
            int accountId,
            string typeFilter,
            int page,
            int pageSize);
    }
}