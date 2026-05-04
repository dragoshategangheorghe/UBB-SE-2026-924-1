using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankApp.Server.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using BankApp.Models.DTOs;
using BankApp.Models.DTOs.Savings;
using BankApp.Models.Features.Savings;
using BankApp.Models.Features.Investments;
using BankApp.Models.Enums;
using BankApp.Server.DataAccess;

namespace BankApp.Server.Repositories.Implementations
{
    /// <summary>
    /// SQL-backed savings repository implementation.
    /// </summary>
    public class SavingsRepository : ISavingsRepository
    {
        private const decimal ZeroAmount = 0m;
        private const int NoFundingAccountId = 0;
        private const int NewAutoDepositId = 0;
        private const decimal NoPenaltyAmount = 0m;
        private const int FirstPageNumber = 1;
        private const int PrimaryFundingSourceId = 1;
        private const int SecondaryFundingSourceId = 2;
        private const string PrimaryFundingSourceName = "Checking Account One";
        private const string SecondaryFundingSourceName = "Checking Account Two";

        private readonly AppDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="SavingsRepository"/> class.
        /// </summary>
        /// <param name="dbContext"></param>
        public SavingsRepository(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Gets savings accounts for a user with optional inclusion of closed accounts.
        /// </summary>
        /// <param name="userIdentificationNumber">The user identifier.</param>
        /// <param name="includesClosedAccounts">Whether closed accounts should be included.</param>
        /// <returns>The user's matching savings accounts.</returns>
        public async Task<List<SavingsAccount>> GetSavingsAccountsByUserIdAsync(
            int userIdentificationNumber,
            bool includesClosedAccounts = false)
        {
            var selectAccountsQuery = @"
                SELECT id, userId, savingsType, balance, accruedInterest, apy,
                       maturityDate, accountStatus, createdAt,
                       accountName, fundingAccountId, targetAmount, targetDate
                FROM SavingsAccount
                WHERE userId = @UserId"
                                      + (includesClosedAccounts ? string.Empty : " AND accountStatus != 'Closed'") +
                                      " ORDER BY balance DESC";

            var accountsList = new List<SavingsAccount>();

            using var reader = await dbContext.ExecuteQueryAsync(selectAccountsQuery, new object[] { userIdentificationNumber });
            while (await reader.ReadAsync())
            {
                accountsList.Add(MapReaderToAccount(reader));
            }

            return accountsList;
        }

        /// <summary>
        /// Creates a new savings account using the provided request and APY.
        /// </summary>
        /// <param name="dataTransferObject">The create-account request payload.</param>
        /// <param name="annualPercentageYield">The annual percentage yield to assign.</param>
        /// <returns>The created savings account.</returns>
        public async Task<SavingsAccount> CreateSavingsAccountAsync(CreateSavingsAccountDto dataTransferObject, decimal annualPercentageYield)
        {
            const string insertAccountQuery = @"
                INSERT INTO SavingsAccount
                    (userId, savingsType, balance, accruedInterest, apy, maturityDate,
                     accountStatus, createdAt, accountName,
                     fundingAccountId, targetAmount, targetDate)
                OUTPUT INSERTED.id
                VALUES
                    (@UserId, @SavingsType, @Balance, @AccruedInterest, @Apy, @MaturityDate,
                     'Active', @CreatedAt, @AccountName,
                     @FundingAccountId, @TargetAmount, @TargetDate)";

            var newSavingsAccountIdentificationNumber = (int)(await dbContext.ExecuteScalarAsync(insertAccountQuery, new object[]
            {
                dataTransferObject.UserIdentificationNumber,
                dataTransferObject.SavingsType,
                dataTransferObject.InitialDeposit,
                ZeroAmount,
                annualPercentageYield,
                (object?)dataTransferObject.MaturityDate ?? DBNull.Value,
                DateTime.Now,
                (object?)dataTransferObject.AccountName ?? DBNull.Value,
                dataTransferObject.FundingAccountId == NoFundingAccountId ? DBNull.Value : dataTransferObject.FundingAccountId,
                (object?)dataTransferObject.TargetAmount ?? DBNull.Value,
                (object?)dataTransferObject.TargetDate ?? DBNull.Value,
            }))!;

            return new SavingsAccount
            {
                IdentificationNumber = newSavingsAccountIdentificationNumber,
                UserIdentificationNumber = dataTransferObject.UserIdentificationNumber,
                SavingsType = dataTransferObject.SavingsType,
                AccountName = dataTransferObject.AccountName,
                Balance = dataTransferObject.InitialDeposit,
                AccruedInterest = ZeroAmount,
                AnnualPercentageYield = annualPercentageYield,
                AccountStatus = "Active",
                CreatedAt = DateTime.Now,
                FundingAccountIdentificationNumber = dataTransferObject.FundingAccountId == NoFundingAccountId ? null : dataTransferObject.FundingAccountId,
                TargetAmount = dataTransferObject.TargetAmount,
                TargetDate = dataTransferObject.TargetDate,
            };
        }

        /// <summary>
        /// Deposits funds into a savings account and records a transaction row.
        /// </summary>
        /// <param name="accountIdentificationNumber">The target account identifier.</param>
        /// <param name="amount">The amount to deposit.</param>
        /// <param name="source">The source label for the deposit.</param>
        /// <returns>The resulting deposit response.</returns>
        public async Task<DepositResponseDto> DepositAsync(int accountIdentificationNumber, decimal amount, string source)
        {
            await dbContext.BeginTransactionAsync();

            try
            {
                const string updateAccountBalanceQuery = @"
                    UPDATE SavingsAccount
                    SET balance = balance + @Amount
                    WHERE id = @AccountId";

                await dbContext.ExecuteNonQueryAsync(updateAccountBalanceQuery, new object[] { amount, accountIdentificationNumber });

                const string selectAccountBalanceQuery = "SELECT balance FROM SavingsAccount WHERE id = @AccountId";
                decimal newAccountBalance = (decimal)(await dbContext.ExecuteScalarAsync(selectAccountBalanceQuery, new object[] { accountIdentificationNumber }))!;

                const string insertTransactionQuery = @"
                INSERT INTO SavingsTransaction
                (accountId, transactionType, amount, balanceAfter, source, description, createdAt)
                OUTPUT INSERTED.id
                VALUES (@AccountId, @TransactionType, @Amount, @BalanceAfter, @Source, @Description, GETUTCDATE())";
                var newTransactionIdentificationNumber = (int)(await dbContext.ExecuteScalarAsync(insertTransactionQuery,
                    new object[] { accountIdentificationNumber, "Deposit", amount, newAccountBalance, source ?? "Manual", DBNull.Value }))!;

                await dbContext.CommitTransactionAsync();

                return new DepositResponseDto
                {
                    NewBalance = newAccountBalance,
                    TransactionId = newTransactionIdentificationNumber,
                    Timestamp = DateTime.Now,
                };
            }
            catch
            {
                await dbContext.RollbackTransactionAsync();
                throw;
            }
        }

        /// <summary>
        /// Closes a savings account and transfers the specified amount to another account.
        /// </summary>
        /// <param name="accountIdentificationNumber">The source account identifier to close.</param>
        /// <param name="destinationAccountIdentificationNumber">The destination account identifier.</param>
        /// <param name="transferAmount">The amount to transfer out during closure.</param>
        /// <param name="earlyClosurePenalty">The penalty applied on closure, if any.</param>
        /// <returns>The closure operation result.</returns>
        public async Task<ClosureResultDto> CloseSavingsAccountAsync(
            int accountIdentificationNumber,
            int destinationAccountIdentificationNumber,
            decimal transferAmount,
            decimal earlyClosurePenalty)
        {
            await dbContext.BeginTransactionAsync();

            try
            {
                decimal oldAccountBalance;
                string oldAccountType;
                DateTime? oldAccountMaturityDate;

                // First step: lock and fetch source account data.
                string selectSourceAccountDataQuery = @"
                SELECT balance, savingsType, maturityDate, accountStatus
                FROM SavingsAccount WITH (UPDLOCK, ROWLOCK)
                WHERE id = @Id";
                using var reader = await dbContext.ExecuteQueryAsync(selectSourceAccountDataQuery, new object[] { accountIdentificationNumber });

                oldAccountBalance = (decimal)reader["balance"];
                oldAccountType = reader["savingsType"].ToString()!;
                oldAccountMaturityDate = reader["maturityDate"] as DateTime?;

                // Second step: transfer funds to destination.
                string transferAmountToDestinationQuery = @"
                UPDATE SavingsAccount 
                SET balance = balance + @Amount
                WHERE id = @DestId";
                await dbContext.ExecuteNonQueryAsync(transferAmountToDestinationQuery, new object[] { transferAmount, destinationAccountIdentificationNumber });

                // Third step: close the source account.
                string closeSourceAccountQuery = @"
                UPDATE SavingsAccount
                SET balance = @ClosedBalance,
                    accountStatus = 'Closed',
                    updatedAt = GETUTCDATE()
                WHERE id = @Id";
                await dbContext.ExecuteNonQueryAsync(closeSourceAccountQuery, new object[] { ZeroAmount, accountIdentificationNumber });

                // Fourth step: insert closure transaction.
                string insertClosureTransactionQuery = @"
                INSERT INTO SavingsTransaction
                (accountId, transactionType, amount, balanceAfter, source, description, createdAt)
                VALUES
                (@AccountId, 'Closure', @Amount, @BalanceAfter, 'Closure', 'Account closed', GETUTCDATE())";
                await dbContext.ExecuteNonQueryAsync(insertClosureTransactionQuery, new object[] { accountIdentificationNumber, transferAmount, ZeroAmount });

                await dbContext.CommitTransactionAsync();

                return new ClosureResultDto
                {
                    Success = true,
                    TransferredAmount = transferAmount,
                    PenaltyApplied = earlyClosurePenalty,
                    Message = "Account closed successfully.",
                    ClosedAt = DateTime.UtcNow,
                };
            }
            catch (Exception exception)
            {
                await dbContext.RollbackTransactionAsync();

                return new ClosureResultDto
                {
                    Success = false,
                    TransferredAmount = ZeroAmount,
                    PenaltyApplied = ZeroAmount,
                    Message = exception.Message,
                    ClosedAt = DateTime.UtcNow,
                };
            }
        }

        /// <summary>
        /// Withdraws funds from a savings account and logs the transaction.
        /// </summary>
        /// <param name="accountId">The source account identifier.</param>
        /// <param name="amount">The amount to withdraw.</param>
        /// <param name="destinationLabel">The destination label shown in transaction history.</param>
        /// <param name="earlyWithdrawalPenalty">The early-withdrawal penalty, if any.</param>
        /// <returns>The withdrawal operation result.</returns>
        public async Task<WithdrawResponseDto> WithdrawAsync(
            int accountId,
            decimal amount,
            string destinationLabel,
            decimal earlyWithdrawalPenalty)
        {
            await dbContext.BeginTransactionAsync();

            try
            {
                string savingsAccountType;
                DateTime? maturityDate;
                decimal oldBalance;

                string selectAccountDataQuery = @"
                SELECT balance, savingsType, maturityDate
                FROM SavingsAccount WITH (UPDLOCK, ROWLOCK)
                WHERE id = @Id";
                using var reader = await dbContext.ExecuteQueryAsync(selectAccountDataQuery, new object[] { accountId });

                oldBalance = (decimal)reader["balance"];
                savingsAccountType = reader["savingsType"].ToString()!;
                maturityDate = reader["maturityDate"] as DateTime?;

                var newBalance = oldBalance - amount;

                string updateAccountBalanceQuery = @"
                UPDATE SavingsAccount SET balance = @Balance WHERE id = @Id";
                await dbContext.ExecuteNonQueryAsync(updateAccountBalanceQuery, new object[] { newBalance, accountId });

                string insertWithdrawalTransactionQuery = @"
                INSERT INTO SavingsTransaction
                (accountId, transactionType, amount, balanceAfter, source, description, createdAt)
                VALUES (@AccountId, 'Withdrawal', @Amount, @BalanceAfter, 'Manual',@Description, GETUTCDATE())";
                var withdrawalDescription = earlyWithdrawalPenalty > NoPenaltyAmount
                    ? $"To: {destinationLabel} | Early withdrawal penalty: {earlyWithdrawalPenalty:C2}"
                    : $"To: {destinationLabel}";

                await dbContext.ExecuteNonQueryAsync(insertWithdrawalTransactionQuery, new object[] { accountId, amount, newBalance, withdrawalDescription });

                await dbContext.CommitTransactionAsync();

                return new WithdrawResponseDto
                {
                    Success = true,
                    AmountWithdrawn = amount,
                    PenaltyApplied = earlyWithdrawalPenalty,
                    NewBalance = newBalance,
                    Message = earlyWithdrawalPenalty > NoPenaltyAmount
                        ? $"Withdrawal successful. Early penalty of {earlyWithdrawalPenalty:C2} applied."
                        : "Withdrawal successful.",
                    ProcessedAt = DateTime.UtcNow,
                };
            }
            catch (Exception exception)
            {
                await dbContext.RollbackTransactionAsync();
                return new WithdrawResponseDto
                {
                    Success = false,
                    Message = exception.Message,
                    ProcessedAt = DateTime.UtcNow,
                };
            }
        }

        /// <summary>
        /// Gets auto-deposit configuration for a savings account.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns>The auto-deposit settings, or <see langword="null"/> when missing.</returns>
        public async Task<AutoDeposit?> GetAutoDepositAsync(int accountId)
        {
            const string selectAutoDepositByAccountIdQuery = @"
                SELECT id, savingsAccountId, amount, frequency, nextRunDate, isActive
                FROM AutoDeposit
                WHERE savingsAccountId = @AccountId";
            using var reader = await dbContext.ExecuteQueryAsync(selectAutoDepositByAccountIdQuery, new object[] { accountId });

            if (!await reader.ReadAsync())
            {
                return null;
            }

            return new AutoDeposit
            {
                Id = (int)reader["id"],
                SavingsAccountId = (int)reader["savingsAccountId"],
                Amount = (decimal)reader["amount"],
                Frequency = Enum.Parse<DepositFrequency>(reader["frequency"].ToString()!),
                NextRunDate = (DateTime)reader["nextRunDate"],
                IsActive = (bool)reader["isActive"],
            };
        }

        /// <summary>
        /// Creates or updates auto-deposit settings for a savings account.
        /// </summary>
        /// <param name="autoDeposit">The auto-deposit entity to save.</param>
        /// <returns>A task that completes when persistence is done.</returns>
        public async Task SaveAutoDepositAsync(AutoDeposit autoDeposit)
        {
            if (autoDeposit.Id == NewAutoDepositId)
            {
                const string insertAutoDepositQuery = @"
                    INSERT INTO AutoDeposit (savingsAccountId, amount, frequency, nextRunDate, isActive)
                    VALUES (@AccountId, @Amount, @Frequency, @NextRunDate, @IsActive)";
                await dbContext.ExecuteNonQueryAsync(insertAutoDepositQuery, new object[]
                    { autoDeposit.SavingsAccountId, autoDeposit.Amount, autoDeposit.Frequency.ToString(), autoDeposit.NextRunDate, autoDeposit.IsActive });
            }
            else
            {
                const string updateAutoDepositQuery = @"
                    UPDATE AutoDeposit
                    SET amount = @Amount, frequency = @Frequency,
                        nextRunDate = @NextRunDate, isActive = @IsActive
                    WHERE id = @Id";
                await dbContext.ExecuteNonQueryAsync(updateAutoDepositQuery, new object[]
                    { autoDeposit.Amount, autoDeposit.Frequency.ToString(), autoDeposit.NextRunDate, autoDeposit.IsActive, autoDeposit.Id });
            }
        }

        /// <summary>
        /// Gets available funding-source options for a user.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The list of funding-source options.</returns>
        public Task<List<FundingSourceOption>> GetFundingSourcesAsync(int userId)
        {
            return Task.FromResult(
                new List<FundingSourceOption>
                {
                new() { Id = PrimaryFundingSourceId, DisplayName = PrimaryFundingSourceName },
                new() { Id = SecondaryFundingSourceId, DisplayName = SecondaryFundingSourceName },
                });
        }

        /// <summary>
        /// Gets paginated savings transactions for an account and filter.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="typeFilter">The transaction-type filter value.</param>
        /// <param name="page">The one-based page number.</param>
        /// <param name="pageSize">The page size.</param>
        /// <returns>A tuple containing page items and total transaction count.</returns>
        public async Task<(List<SavingsTransaction> Items, int TotalCount)> GetTransactionsPagedAsync(
            int accountId,
            string typeFilter,
            int page,
            int pageSize)
        {
            var baseQuery = @"
                FROM SavingsTransaction
                WHERE accountId = @AccountId";

            // filter
            if (!string.IsNullOrEmpty(typeFilter) && typeFilter != "All")
            {
                baseQuery += " AND transactionType = @Type";
            }

            // total count
            string selectCountQuery = "SELECT COUNT(*) " + baseQuery;
            int numberOfAccountTransactions;

            if (baseQuery.Contains("@Type"))
            {
                numberOfAccountTransactions = (int)(await dbContext.ExecuteScalarAsync(selectCountQuery, new object[] {accountId, typeFilter}))!;
            }
            else
            {
                numberOfAccountTransactions = (int)(await dbContext.ExecuteScalarAsync(selectCountQuery, new object[] { accountId }))!;
            }

            // paginated selectAccountsQuery
            var paginatedSelectAccountsQuery = @"
                SELECT id, accountId, transactionType, amount, balanceAfter, source, description, createdAt
                " + baseQuery + @"
                ORDER BY createdAt DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            SqlDataReader reader;
            if (baseQuery.Contains("@Type"))
            {
                reader = await dbContext.ExecuteQueryAsync(paginatedSelectAccountsQuery, new object[] { accountId, typeFilter, (page - FirstPageNumber) * pageSize, pageSize });
            }
            else
            {
                reader = await dbContext.ExecuteQueryAsync(paginatedSelectAccountsQuery, new object[] { accountId, (page - FirstPageNumber) * pageSize, pageSize });
            }

            var transactionsList = new List<SavingsTransaction>();

            while (await reader.ReadAsync())
            {
                transactionsList.Add(
                    new SavingsTransaction
                    {
                        Id = (int)reader["id"],
                        AccountId = (int)reader["accountId"],
                        Type = Enum.Parse<TransactionType>(reader["transactionType"].ToString()!),
                        Amount = (decimal)reader["amount"],
                        BalanceAfter = (decimal)reader["balanceAfter"],
                        Source = reader["source"].ToString(),
                        Description = reader["description"] as string,
                        CreatedAt = (DateTime)reader["createdAt"],
                    });
            }

            return (transactionsList, numberOfAccountTransactions);
        }

        private static SavingsAccount MapReaderToAccount(SqlDataReader sqlDataReader)
        {
            return new SavingsAccount
            {
                IdentificationNumber = sqlDataReader.GetInt32(sqlDataReader.GetOrdinal("id")),
                UserIdentificationNumber = sqlDataReader.GetInt32(sqlDataReader.GetOrdinal("userId")),
                SavingsType = sqlDataReader["savingsType"]?.ToString() ?? string.Empty,
                Balance = sqlDataReader.GetDecimal(sqlDataReader.GetOrdinal("balance")),
                AccruedInterest = sqlDataReader.GetDecimal(sqlDataReader.GetOrdinal("accruedInterest")),
                AnnualPercentageYield = sqlDataReader.GetDecimal(sqlDataReader.GetOrdinal("apy")),
                MaturityDate = sqlDataReader["maturityDate"] as DateTime?,
                AccountStatus = sqlDataReader["accountStatus"]?.ToString() ?? string.Empty,
                CreatedAt = sqlDataReader.GetDateTime(sqlDataReader.GetOrdinal("createdAt")),
                AccountName = sqlDataReader["accountName"] as string,
                FundingAccountIdentificationNumber = sqlDataReader["fundingAccountId"] as int?,
                TargetAmount = sqlDataReader["targetAmount"] as decimal?,
                TargetDate = sqlDataReader["targetDate"] as DateTime?,
            };
        }
    }
}