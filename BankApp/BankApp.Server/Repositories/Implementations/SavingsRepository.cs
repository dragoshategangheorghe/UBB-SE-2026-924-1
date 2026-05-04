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

            using var databaseConnection = DatabaseConfig.GetDatabaseConnection();
            await databaseConnection.OpenAsync();

            using var sqlCommand = new SqlCommand(selectAccountsQuery, databaseConnection);
            sqlCommand.Parameters.AddWithValue("@UserId", userIdentificationNumber);

            using var reader = await sqlCommand.ExecuteReaderAsync();
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

            using var databaseConnection = DatabaseConfig.GetDatabaseConnection();
            await databaseConnection.OpenAsync();

            using var sqlCommand = new SqlCommand(insertAccountQuery, databaseConnection);
            sqlCommand.Parameters.AddWithValue("@UserId", dataTransferObject.UserIdentificationNumber);
            sqlCommand.Parameters.AddWithValue("@SavingsType", dataTransferObject.SavingsType);
            sqlCommand.Parameters.AddWithValue("@Balance", dataTransferObject.InitialDeposit);
            sqlCommand.Parameters.AddWithValue("@AccruedInterest", ZeroAmount);
            sqlCommand.Parameters.AddWithValue("@Apy", annualPercentageYield);
            sqlCommand.Parameters.AddWithValue("@MaturityDate", (object?)dataTransferObject.MaturityDate ?? DBNull.Value);
            sqlCommand.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
            sqlCommand.Parameters.AddWithValue("@AccountName", (object?)dataTransferObject.AccountName ?? DBNull.Value);
            sqlCommand.Parameters.AddWithValue(
                "@FundingAccountId",
                dataTransferObject.FundingAccountId == NoFundingAccountId ? DBNull.Value : dataTransferObject.FundingAccountId);
            sqlCommand.Parameters.AddWithValue("@TargetAmount", (object?)dataTransferObject.TargetAmount ?? DBNull.Value);
            sqlCommand.Parameters.AddWithValue("@TargetDate", (object?)dataTransferObject.TargetDate ?? DBNull.Value);

            var newSavingsAccountIdentificationNumber = (int)(await sqlCommand.ExecuteScalarAsync())!;

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
            using var databaseConnection = DatabaseConfig.GetDatabaseConnection();
            await databaseConnection.OpenAsync();
            using var sqlTransaction = databaseConnection.BeginTransaction();

            try
            {
                const string updateAccountBalanceQuery = @"
                    UPDATE SavingsAccount
                    SET balance = balance + @Amount
                    WHERE id = @AccountId";

                using var sqlUpdateAccountBalanceCommand = new SqlCommand(
                    updateAccountBalanceQuery,
                    databaseConnection,
                    sqlTransaction);
                sqlUpdateAccountBalanceCommand.Parameters.AddWithValue("@Amount", amount);
                sqlUpdateAccountBalanceCommand.Parameters.AddWithValue("@AccountId", accountIdentificationNumber);
                await sqlUpdateAccountBalanceCommand.ExecuteNonQueryAsync();

                decimal newAccountBalance;

                const string selectAccountBalanceQuery = "SELECT balance FROM SavingsAccount WHERE id = @AccountId";
                using (var sqlSelectAccountBalanceCommand = new SqlCommand(
                           selectAccountBalanceQuery,
                           databaseConnection,
                           sqlTransaction))
                {
                    sqlSelectAccountBalanceCommand.Parameters.AddWithValue("@AccountId", accountIdentificationNumber);
                    newAccountBalance = (decimal)(await sqlSelectAccountBalanceCommand.ExecuteScalarAsync())!;
                }

                const string insertTransactionQuery = @"
                INSERT INTO SavingsTransaction
                (accountId, transactionType, amount, balanceAfter, source, description, createdAt)
                OUTPUT INSERTED.id
                VALUES (@AccountId, @TransactionType, @Amount, @BalanceAfter, @Source, @Description, GETUTCDATE())";

                using var sqlInsertTransactionCommand =
                    new SqlCommand(insertTransactionQuery, databaseConnection, sqlTransaction);

                sqlInsertTransactionCommand.Parameters.AddWithValue("@AccountId", accountIdentificationNumber);
                sqlInsertTransactionCommand.Parameters.AddWithValue("@TransactionType", "Deposit");
                sqlInsertTransactionCommand.Parameters.AddWithValue("@Amount", amount);
                sqlInsertTransactionCommand.Parameters.AddWithValue("@BalanceAfter", newAccountBalance);
                sqlInsertTransactionCommand.Parameters.AddWithValue("@Source", source ?? "Manual");
                sqlInsertTransactionCommand.Parameters.AddWithValue("@Description", DBNull.Value);

                var newTransactionIdentificationNumber = (int)(await sqlInsertTransactionCommand.ExecuteScalarAsync())!;

                await sqlTransaction.CommitAsync();

                return new DepositResponseDto
                {
                    NewBalance = newAccountBalance,
                    TransactionId = newTransactionIdentificationNumber,
                    Timestamp = DateTime.Now,
                };
            }
            catch
            {
                await sqlTransaction.RollbackAsync();
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
            using var databasebConnection = DatabaseConfig.GetDatabaseConnection();
            await databasebConnection.OpenAsync();

            using var databaseTransaction = databasebConnection.BeginTransaction();

            try
            {
                decimal oldAccountBalance;
                string oldAccountType;
                DateTime? oldAccountMaturityDate;

                // First step: lock and fetch source account data.
                using (var selectSourceAccountDataCommand = new SqlCommand(
                           @"
                SELECT balance, savingsType, maturityDate, accountStatus
                FROM SavingsAccount WITH (UPDLOCK, ROWLOCK)
                WHERE id = @Id",
                           databasebConnection,
                           databaseTransaction))
                {
                    selectSourceAccountDataCommand.Parameters.AddWithValue("@Id", accountIdentificationNumber);

                    using var reader = await selectSourceAccountDataCommand.ExecuteReaderAsync();

                    oldAccountBalance = (decimal)reader["balance"];
                    oldAccountType = reader["savingsType"].ToString()!;
                    oldAccountMaturityDate = reader["maturityDate"] as DateTime?;
                }

                // Second step: transfer funds to destination.
                using (var transferAmountToDestinationCommand = new SqlCommand(
                           @"
                UPDATE SavingsAccount 
                SET balance = balance + @Amount
                WHERE id = @DestId",
                           databasebConnection,
                           databaseTransaction))
                {
                    transferAmountToDestinationCommand.Parameters.AddWithValue("@Amount", transferAmount);
                    transferAmountToDestinationCommand.Parameters.AddWithValue("@DestId", destinationAccountIdentificationNumber);

                    await transferAmountToDestinationCommand.ExecuteNonQueryAsync();
                }

                // Third step: close the source account.
                using (var closeAccountCommand = new SqlCommand(
                           @"
                UPDATE SavingsAccount
                SET balance = @ClosedBalance,
                    accountStatus = 'Closed',
                    updatedAt = GETUTCDATE()
                WHERE id = @Id",
                           databasebConnection,
                           databaseTransaction))
                {
                    closeAccountCommand.Parameters.AddWithValue("@Id", accountIdentificationNumber);
                    closeAccountCommand.Parameters.AddWithValue("@ClosedBalance", ZeroAmount);
                    await closeAccountCommand.ExecuteNonQueryAsync();
                }

                // Fourth step: insert closure transaction.
                using (var insertClosureTransactionCommand = new SqlCommand(
                           @"
                INSERT INTO SavingsTransaction
                (accountId, transactionType, amount, balanceAfter, source, description, createdAt)
                VALUES
                (@AccountId, 'Closure', @Amount, @BalanceAfter, 'Closure', 'Account closed', GETUTCDATE())",
                           databasebConnection,
                           databaseTransaction))
                {
                    insertClosureTransactionCommand.Parameters.AddWithValue("@AccountId", accountIdentificationNumber);
                    insertClosureTransactionCommand.Parameters.AddWithValue("@Amount", transferAmount);
                    insertClosureTransactionCommand.Parameters.AddWithValue("@BalanceAfter", ZeroAmount);

                    await insertClosureTransactionCommand.ExecuteNonQueryAsync();
                }

                await databaseTransaction.CommitAsync();

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
                await databaseTransaction.RollbackAsync();

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
            using var databaseConnection = DatabaseConfig.GetDatabaseConnection();
            await databaseConnection.OpenAsync();
            using var databaseTransaction = databaseConnection.BeginTransaction();

            try
            {
                string savingsAccountType;
                DateTime? maturityDate;
                decimal oldBalance;

                using (var selectAccountDataCommand = new SqlCommand(
                           @"
                SELECT balance, savingsType, maturityDate
                FROM SavingsAccount WITH (UPDLOCK, ROWLOCK)
                WHERE id = @Id",
                           databaseConnection,
                           databaseTransaction))
                {
                    selectAccountDataCommand.Parameters.AddWithValue("@Id", accountId);
                    using var reader = await selectAccountDataCommand.ExecuteReaderAsync();

                    oldBalance = (decimal)reader["balance"];
                    savingsAccountType = reader["savingsType"].ToString()!;
                    maturityDate = reader["maturityDate"] as DateTime?;
                }

                var newBalance = oldBalance - amount;

                using (var updateAccountBalanceCommand = new SqlCommand(
                           @"
                UPDATE SavingsAccount SET balance = @Balance WHERE id = @Id",
                           databaseConnection,
                           databaseTransaction))
                {
                    updateAccountBalanceCommand.Parameters.AddWithValue("@Balance", newBalance);
                    updateAccountBalanceCommand.Parameters.AddWithValue("@Id", accountId);
                    await updateAccountBalanceCommand.ExecuteNonQueryAsync();
                }

                using (var insertWithdrawalTransactionCommand = new SqlCommand(
                           @"
                INSERT INTO SavingsTransaction
                (accountId, transactionType, amount, balanceAfter, source, description, createdAt)
                VALUES (@AccountId, 'Withdrawal', @Amount, @BalanceAfter, 'Manual',
                        @Description, GETUTCDATE())",
                           databaseConnection,
                           databaseTransaction))
                {
                    insertWithdrawalTransactionCommand.Parameters.AddWithValue("@AccountId", accountId);
                    insertWithdrawalTransactionCommand.Parameters.AddWithValue("@Amount", amount);
                    insertWithdrawalTransactionCommand.Parameters.AddWithValue("@BalanceAfter", newBalance);

                    var withdrawalDescription = earlyWithdrawalPenalty > NoPenaltyAmount
                        ? $"To: {destinationLabel} | Early withdrawal penalty: {earlyWithdrawalPenalty:C2}"
                        : $"To: {destinationLabel}";

                    insertWithdrawalTransactionCommand.Parameters.AddWithValue("@Description", withdrawalDescription);
                    await insertWithdrawalTransactionCommand.ExecuteNonQueryAsync();
                }

                await databaseTransaction.CommitAsync();

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
                await databaseTransaction.RollbackAsync();
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

            using var databaseConnection = DatabaseConfig.GetDatabaseConnection();
            await databaseConnection.OpenAsync();

            using var selectAutoDepositByAccountIdCommand = new SqlCommand(selectAutoDepositByAccountIdQuery, databaseConnection);
            selectAutoDepositByAccountIdCommand.Parameters.AddWithValue("@AccountId", accountId);
            using var reader = await selectAutoDepositByAccountIdCommand.ExecuteReaderAsync();

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
            using var databaseConnection = DatabaseConfig.GetDatabaseConnection();
            await databaseConnection.OpenAsync();

            if (autoDeposit.Id == NewAutoDepositId)
            {
                const string insertAutoDepositQuery = @"
                    INSERT INTO AutoDeposit (savingsAccountId, amount, frequency, nextRunDate, isActive)
                    VALUES (@AccountId, @Amount, @Frequency, @NextRunDate, @IsActive)";

                using var insertAutoDepositCommand = new SqlCommand(insertAutoDepositQuery, databaseConnection);
                insertAutoDepositCommand.Parameters.AddWithValue("@AccountId", autoDeposit.SavingsAccountId);
                insertAutoDepositCommand.Parameters.AddWithValue("@Amount", autoDeposit.Amount);
                insertAutoDepositCommand.Parameters.AddWithValue("@Frequency", autoDeposit.Frequency.ToString());
                insertAutoDepositCommand.Parameters.AddWithValue("@NextRunDate", autoDeposit.NextRunDate);
                insertAutoDepositCommand.Parameters.AddWithValue("@IsActive", autoDeposit.IsActive);
                await insertAutoDepositCommand.ExecuteNonQueryAsync();
            }
            else
            {
                const string updateAutoDepositQuery = @"
                    UPDATE AutoDeposit
                    SET amount = @Amount, frequency = @Frequency,
                        nextRunDate = @NextRunDate, isActive = @IsActive
                    WHERE id = @Id";

                using var updateAutoDepositCommand = new SqlCommand(updateAutoDepositQuery, databaseConnection);
                updateAutoDepositCommand.Parameters.AddWithValue("@Id", autoDeposit.Id);
                updateAutoDepositCommand.Parameters.AddWithValue("@Amount", autoDeposit.Amount);
                updateAutoDepositCommand.Parameters.AddWithValue("@Frequency", autoDeposit.Frequency.ToString());
                updateAutoDepositCommand.Parameters.AddWithValue("@NextRunDate", autoDeposit.NextRunDate);
                updateAutoDepositCommand.Parameters.AddWithValue("@IsActive", autoDeposit.IsActive);
                await updateAutoDepositCommand.ExecuteNonQueryAsync();
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
            using var databaseConnection = DatabaseConfig.GetDatabaseConnection();
            await databaseConnection.OpenAsync();

            var baseQuery = @"
                FROM SavingsTransaction
                WHERE accountId = @AccountId";

            // filter
            if (!string.IsNullOrEmpty(typeFilter) && typeFilter != "All")
            {
                baseQuery += " AND transactionType = @Type";
            }

            // total count
            using var countAccountTransactionsCommand = new SqlCommand("SELECT COUNT(*) " + baseQuery, databaseConnection);
            countAccountTransactionsCommand.Parameters.AddWithValue("@AccountId", accountId);

            if (baseQuery.Contains("@Type"))
            {
                countAccountTransactionsCommand.Parameters.AddWithValue("@Type", typeFilter);
            }

            var numberOfAccountTransactions = (int)(await countAccountTransactionsCommand.ExecuteScalarAsync())!;

            // paginated selectAccountsQuery
            var paginatedSelectAccountsQuery = @"
                SELECT id, accountId, transactionType, amount, balanceAfter, source, description, createdAt
                " + baseQuery + @"
                ORDER BY createdAt DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            using var paginatedSelectAccountsCommand = new SqlCommand(paginatedSelectAccountsQuery, databaseConnection);
            paginatedSelectAccountsCommand.Parameters.AddWithValue("@AccountId", accountId);
            paginatedSelectAccountsCommand.Parameters.AddWithValue("@Offset", (page - FirstPageNumber) * pageSize);
            paginatedSelectAccountsCommand.Parameters.AddWithValue("@PageSize", pageSize);

            if (baseQuery.Contains("@Type"))
            {
                paginatedSelectAccountsCommand.Parameters.AddWithValue("@Type", typeFilter);
            }

            var transactionsList = new List<SavingsTransaction>();

            using var reader = await paginatedSelectAccountsCommand.ExecuteReaderAsync();

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