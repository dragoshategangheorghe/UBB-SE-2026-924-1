using System.Data;
using System.Globalization;
using BankApp.Models.DTOs.Savings;
using BankApp.Models.Entities;
using BankApp.Models.Enums;
using BankApp.Models.Features.Investments;
using BankApp.Models.Features.Savings;
using BankApp.Server.DataAccess;
using BankApp.Server.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankApp.Server.Repositories.Implementations
{
    /// <summary>
    /// EF Core-backed savings repository implementation.
    /// </summary>
    public class SavingsRepository : ISavingsRepository
    {
        private const decimal ZeroAmount = 0m;
        private const int NoFundingAccountId = 0;
        private const int NewAutoDepositId = 0;
        private const decimal NoPenaltyAmount = 0m;
        private const int FirstPageNumber = 1;
        private const string ClosedStatus = "Closed";

        private readonly AppDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="SavingsRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The application's EF Core database _dbContext.</param>
        public SavingsRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Gets savings accounts for a user through navigation mappings.
        /// </summary>
        public async Task<List<SavingsAccount>> GetSavingsAccountsByUserIdAsync(
            int userIdentificationNumber,
            bool includesClosedAccounts = false)
        {
            var query = _dbContext.SavingsAccounts
                .AsNoTracking()
                .Where(savingsAccount => EF.Property<int>(savingsAccount, "UserId") == userIdentificationNumber);
            if (!includesClosedAccounts)
            {
                query = query.Where(openSavingsAccount => openSavingsAccount.AccountStatus != "Closed");
            }

            return await query.OrderByDescending(savingsAccount => savingsAccount.Balance).ToListAsync();
        }

        /// <summary>
        /// Creates a new savings account using EF Core and returns the created entity.
        /// </summary>
        public async Task<SavingsAccount> CreateSavingsAccountAsync(CreateSavingsAccountDto dataTransferObject, decimal annualPercentageYield)
        {
            var user = await _dbContext.Users
                .Include(user => user.Accounts)
                .FirstOrDefaultAsync(user => user.Id == dataTransferObject.UserIdentificationNumber);

            if (user == null)
            {
                throw new InvalidOperationException("User was not found.");
            }

            var fundingAccount = dataTransferObject.FundingAccountId == NoFundingAccountId
                ? null
                : await ResolveUserFundingAccountAsync(
                    dataTransferObject.UserIdentificationNumber,
                    dataTransferObject.FundingAccountId,
                    null,
                    fallbackToAnyActiveAccount: false);

            if (dataTransferObject.FundingAccountId != NoFundingAccountId && fundingAccount == null)
            {
                throw new InvalidOperationException("Funding account was not found.");
            }

            var account = new SavingsAccount
            {
                User = user,
                SavingsType = dataTransferObject.SavingsType,
                AccountName = dataTransferObject.AccountName,
                Balance = dataTransferObject.InitialDeposit,
                AccruedInterest = ZeroAmount,
                AnnualPercentageYield = annualPercentageYield,
                AccountStatus = "Active",
                CreatedAt = DateTime.UtcNow,
                FundingAccount = fundingAccount,
                TargetAmount = dataTransferObject.TargetAmount,
                TargetDate = dataTransferObject.TargetDate,
                MaturityDate = dataTransferObject.MaturityDate,
            };

            _dbContext.SavingsAccounts.Add(account);
            await _dbContext.SaveChangesAsync();
            return account;
        }

        /// <summary>
        /// Deposits funds into a savings account and records a transaction row.
        /// </summary>
        public async Task<DepositResponseDto> DepositAsync(int accountIdentificationNumber, decimal amount, string source)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var account = await _dbContext.SavingsAccounts
                    .Include(savingsAccount => savingsAccount.User)
                    .Include(savingsAccount => savingsAccount.FundingAccount)
                    .FirstOrDefaultAsync(savingsAccount => savingsAccount.IdentificationNumber == accountIdentificationNumber);

                if (account == null)
                {
                    throw new InvalidOperationException("Savings account was not found.");
                }

                var fundingAccount = await EnsureFundingAccountAsync(account, source);

                account.Balance += amount;
                var newAccountBalance = account.Balance;

                var savingsTransaction = new SavingsTransaction
                {
                    // SavingsAccount = account,
                    Account = fundingAccount,
                    Amount = amount,
                    Type = TransactionType.Deposit,
                    Source = source ?? "Manual",
                    BalanceAfter = newAccountBalance,
                    CreatedAt = DateTime.UtcNow,
                };

                _dbContext.SavingsTransactions.Add(savingsTransaction);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return new DepositResponseDto
                {
                    NewBalance = newAccountBalance,
                    TransactionId = savingsTransaction.Id,
                    Timestamp = DateTime.UtcNow,
                };
            }
            catch (InvalidOperationException)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Closes a savings account and transfers the specified amount to another account.
        /// </summary>
        public async Task<ClosureResultDto> CloseSavingsAccountAsync(
            int accountIdentificationNumber,
            int destinationAccountIdentificationNumber,
            decimal transferAmount,
            decimal earlyClosurePenalty)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var sourceAccount = await _dbContext.SavingsAccounts
                    .Include(savingsAccount => savingsAccount.FundingAccount)
                    .Include(savingsAccount => savingsAccount.User)
                    .FirstOrDefaultAsync(savingsAccount => savingsAccount.IdentificationNumber == accountIdentificationNumber);

                var destinationAccount = await _dbContext.SavingsAccounts
                    .Include(savingsAccount => savingsAccount.FundingAccount)
                    .Include(savingsAccount => savingsAccount.User)
                    .FirstOrDefaultAsync(savingsAccount => savingsAccount.IdentificationNumber == destinationAccountIdentificationNumber);

                if (sourceAccount == null || destinationAccount == null)
                {
                    throw new InvalidOperationException("One or more savings accounts were not found.");
                }

                var fundingAccount = await EnsureFundingAccountAsync(sourceAccount, "Closure");

                sourceAccount.Balance = ZeroAmount;
                sourceAccount.AccountStatus = "Closed";
                destinationAccount.Balance += transferAmount;

                _dbContext.SavingsTransactions.Add(new SavingsTransaction
                {
                    // SavingsAccount = sourceAccount,
                    Account = fundingAccount,
                    Amount = transferAmount,
                    Type = TransactionType.Deposit,
                    Source = "Closure",
                    Description = "Account closed",
                    BalanceAfter = ZeroAmount,
                    CreatedAt = DateTime.UtcNow,
                });

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

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
                await transaction.RollbackAsync();

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
        public async Task<WithdrawResponseDto> WithdrawAsync(
            int accountId,
            decimal amount,
            string destinationLabel,
            decimal earlyWithdrawalPenalty)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var account = await _dbContext.SavingsAccounts
                    .Include(savingsAccount => savingsAccount.User)
                    .Include(savingsAccount => savingsAccount.FundingAccount)
                    .FirstOrDefaultAsync(savingsAccount => savingsAccount.IdentificationNumber == accountId);

                if (account == null)
                {
                    throw new InvalidOperationException("Savings account was not found.");
                }

                var fundingAccount = await EnsureFundingAccountAsync(account, destinationLabel);

                var newBalance = account.Balance - amount;
                account.Balance = newBalance;

                var withdrawalDescription = earlyWithdrawalPenalty > NoPenaltyAmount
                    ? $"To: {destinationLabel} | Early withdrawal penalty: {earlyWithdrawalPenalty:C2}"
                    : $"To: {destinationLabel}";

                _dbContext.SavingsTransactions.Add(new SavingsTransaction
                {
                    // SavingsAccount = account,
                    Account = fundingAccount,
                    Amount = amount,
                    Type = TransactionType.Withdrawal,
                    Source = "Manual",
                    Description = withdrawalDescription,
                    BalanceAfter = newBalance,
                    CreatedAt = DateTime.UtcNow,
                });

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

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
                await transaction.RollbackAsync();
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
        public async Task<AutoDeposit?> GetAutoDepositAsync(int accountId)
        {
            var connection = _dbContext.Database.GetDbConnection();
            var shouldCloseConnection = connection.State != ConnectionState.Open;

            if (shouldCloseConnection)
            {
                await connection.OpenAsync();
            }

            try
            {
                await using var command = connection.CreateCommand();
                command.CommandText = """
                                      SELECT TOP (1)
                                          Id,
                                          savingsAccountId,
                                          Amount,
                                          Frequency,
                                          NextRunDate,
                                          IsActive,
                                          sourceAccountId,
                                          dayOfMonth,
                                          dayOfWeek,
                                          updatedAt
                                      FROM AutoDeposit
                                      WHERE savingsAccountId = @accountId
                                      ORDER BY Id DESC
                                      """;

                var parameter = command.CreateParameter();
                parameter.ParameterName = "@accountId";
                parameter.DbType = DbType.Int32;
                parameter.Value = accountId;
                command.Parameters.Add(parameter);

                await using var reader = await command.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                {
                    return null;
                }

                return new AutoDeposit
                {
                    Id = ReadInt32(reader, "Id"),
                    SavingsAccountId = ReadInt32(reader, "savingsAccountId"),
                    Amount = ReadDecimal(reader, "Amount"),
                    Frequency = ReadEnum<DepositFrequency>(reader, "Frequency"),
                    NextRunDate = ReadDateTime(reader, "NextRunDate"),
                    IsActive = ReadBoolean(reader, "IsActive"),
                    SourceAccountId = ReadNullableInt32(reader, "sourceAccountId"),
                    DayOfMonth = ReadNullableInt32(reader, "dayOfMonth"),
                    DayOfWeek = ReadNullableInt32(reader, "dayOfWeek"),
                    UpdatedAt = ReadNullableDateTime(reader, "updatedAt"),
                };
            }
            finally
            {
                if (shouldCloseConnection)
                {
                    await connection.CloseAsync();
                }
            }
        }

        /// <summary>
        /// Creates or updates auto-deposit settings for a savings account.
        /// </summary>
        public async Task SaveAutoDepositAsync(AutoDeposit autoDeposit)
        {
            var savingsAccountId = autoDeposit.SavingsAccountId != NewAutoDepositId
                ? autoDeposit.SavingsAccountId
                : autoDeposit.SavingsAccount?.IdentificationNumber ?? NewAutoDepositId;

            if (savingsAccountId == NewAutoDepositId)
            {
                throw new InvalidOperationException("Savings account is required.");
            }

            var savingsAccount = await _dbContext.SavingsAccounts
                .FirstOrDefaultAsync(account => account.IdentificationNumber == savingsAccountId);

            if (savingsAccount == null)
            {
                throw new InvalidOperationException("Savings account was not found.");
            }

            autoDeposit.SavingsAccountId = savingsAccountId;
            autoDeposit.SavingsAccount = savingsAccount;

            if (autoDeposit.Id == NewAutoDepositId)
            {
                _dbContext.AutoDeposits.Add(autoDeposit);
            }
            else
            {
                _dbContext.AutoDeposits.Update(autoDeposit);
            }

            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Gets available funding-source _options for a user.
        /// </summary>
        public async Task<List<FundingSourceOption>> GetFundingSourcesAsync(int userId)
        {
            var userAccounts = await _dbContext.Accounts
                .AsNoTracking()
                .Where(account => account.UserId == userId && account.Status != ClosedStatus)
                .OrderBy(account => account.AccountName ?? account.AccountType)
                .ThenBy(account => account.Id)
                .ToListAsync();

            return userAccounts
                .Select(account => new FundingSourceOption
                {
                    Id = account.Id,
                    DisplayName = BuildFundingSourceDisplayName(account),
                })
                .ToList();
        }

        /// <summary>
        /// Gets paginated savings transactions for an account and filter.
        /// </summary>
        public async Task<(List<SavingsTransaction> Items, int TotalCount)> GetTransactionsPagedAsync(
            int accountId,
            string typeFilter,
            int page,
            int pageSize)
        {
            var query = _dbContext.SavingsTransactions
                .AsNoTracking()
                .Include(savingsTransaction => savingsTransaction.Account)
                .Where(savingsTransaction => savingsTransaction.SavingsAccount != null &&
                                             savingsTransaction.SavingsAccount.IdentificationNumber == accountId);

            if (!string.IsNullOrEmpty(typeFilter) && typeFilter != "All")
            {
                if (Enum.TryParse<TransactionType>(typeFilter, out var parsedType))
                {
                    query = query.Where(x => x.Type == parsedType);
                }
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(savingsTransaction => savingsTransaction.CreatedAt)
                .Skip((page - FirstPageNumber) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        private async Task<Account> EnsureFundingAccountAsync(SavingsAccount savingsAccount, string? preferredLabel)
        {
            if (savingsAccount.FundingAccount != null)
            {
                return savingsAccount.FundingAccount;
            }

            var userId = savingsAccount.User?.Id > 0
                ? savingsAccount.User.Id
                : _dbContext.Entry(savingsAccount).Property<int>("UserId").CurrentValue;
            var fundingAccountId = _dbContext.Entry(savingsAccount).Property<int?>("FundingAccountId").CurrentValue;

            var resolvedFundingAccount = await ResolveUserFundingAccountAsync(userId, fundingAccountId, preferredLabel);
            if (resolvedFundingAccount == null)
            {
                throw new InvalidOperationException("Funding account was not found.");
            }

            savingsAccount.FundingAccount = resolvedFundingAccount;
            _dbContext.Entry(savingsAccount).Property("FundingAccountId").CurrentValue = resolvedFundingAccount.Id;

            return resolvedFundingAccount;
        }

        private async Task<Account?> ResolveUserFundingAccountAsync(
            int userId,
            int? preferredAccountId,
            string? preferredLabel,
            bool fallbackToAnyActiveAccount = true)
        {
            var activeAccounts = await _dbContext.Accounts
                .Where(account => account.UserId == userId && account.Status != ClosedStatus)
                .OrderBy(account => account.Id)
                .ToListAsync();

            if (preferredAccountId.HasValue && preferredAccountId.Value != NoFundingAccountId)
            {
                var accountById = activeAccounts.FirstOrDefault(account => account.Id == preferredAccountId.Value);
                if (accountById != null || !fallbackToAnyActiveAccount)
                {
                    return accountById;
                }
            }

            if (!string.IsNullOrWhiteSpace(preferredLabel))
            {
                var normalizedLabel = preferredLabel.Trim();
                var accountByLabel = activeAccounts.FirstOrDefault(account =>
                    string.Equals(BuildFundingSourceDisplayName(account), normalizedLabel, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(account.AccountName, normalizedLabel, StringComparison.OrdinalIgnoreCase));

                if (accountByLabel != null || !fallbackToAnyActiveAccount)
                {
                    return accountByLabel;
                }
            }

            return fallbackToAnyActiveAccount
                ? activeAccounts.FirstOrDefault()
                : null;
        }

        private static string BuildFundingSourceDisplayName(Account account)
        {
            var primaryLabel = !string.IsNullOrWhiteSpace(account.AccountName)
                ? account.AccountName.Trim()
                : !string.IsNullOrWhiteSpace(account.AccountType)
                    ? account.AccountType.Trim()
                    : $"Account {account.Id}";

            return TryGetMaskedIbanSuffix(account.IBAN) is { Length: > 0 } maskedSuffix
                ? $"{primaryLabel} • {maskedSuffix}"
                : primaryLabel;
        }

        private static string TryGetMaskedIbanSuffix(string? iban)
        {
            if (string.IsNullOrWhiteSpace(iban))
            {
                return string.Empty;
            }

            var trimmedIban = iban.Trim();
            return trimmedIban.Length <= 4
                ? trimmedIban
                : trimmedIban[^4..];
        }

        private static int ReadInt32(IDataRecord record, string columnName)
        {
            return Convert.ToInt32(record[columnName], CultureInfo.InvariantCulture);
        }

        private static int? ReadNullableInt32(IDataRecord record, string columnName)
        {
            var value = record[columnName];
            return value == DBNull.Value
                ? null
                : Convert.ToInt32(value, CultureInfo.InvariantCulture);
        }

        private static decimal ReadDecimal(IDataRecord record, string columnName)
        {
            return Convert.ToDecimal(record[columnName], CultureInfo.InvariantCulture);
        }

        private static bool ReadBoolean(IDataRecord record, string columnName)
        {
            return Convert.ToBoolean(record[columnName], CultureInfo.InvariantCulture);
        }

        private static DateTime ReadDateTime(IDataRecord record, string columnName)
        {
            return Convert.ToDateTime(record[columnName], CultureInfo.InvariantCulture);
        }

        private static DateTime? ReadNullableDateTime(IDataRecord record, string columnName)
        {
            var value = record[columnName];
            return value == DBNull.Value
                ? null
                : Convert.ToDateTime(value, CultureInfo.InvariantCulture);
        }

        private static TEnum ReadEnum<TEnum>(IDataRecord record, string columnName)
            where TEnum : struct, Enum
        {
            var value = record[columnName];

            if (value is string textValue)
            {
                if (Enum.TryParse<TEnum>(textValue, true, out var enumByName))
                {
                    return enumByName;
                }

                return (TEnum)Enum.ToObject(typeof(TEnum), Convert.ToInt32(textValue, CultureInfo.InvariantCulture));
            }

            return (TEnum)Enum.ToObject(typeof(TEnum), Convert.ToInt32(value, CultureInfo.InvariantCulture));
        }
    }
}
