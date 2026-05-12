using BankApp.Models.DTOs.Savings;
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
        private const int PrimaryFundingSourceId = 1;
        private const int SecondaryFundingSourceId = 2;
        private const string PrimaryFundingSourceName = "Checking Account One";
        private const string SecondaryFundingSourceName = "Checking Account Two";

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
                .Include(savingsAccount => savingsAccount.User)
                .Include(savingsAccount => savingsAccount.FundingAccount)
                .Include(savingsAccount => savingsAccount.AutoDeposits)
                .Include(savingsAccount => savingsAccount.Transactions)
                .Where(savingsAccount => savingsAccount.User.Id == userIdentificationNumber);
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
                : await _dbContext.Accounts
                    .Include(account => account.User)
                    .FirstOrDefaultAsync(a => a.Id == dataTransferObject.FundingAccountId);

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

                if (account.FundingAccount == null)
                {
                    throw new InvalidOperationException("Funding account was not found.");
                }

                account.Balance += amount;
                var newAccountBalance = account.Balance;

                var savingsTransaction = new SavingsTransaction
                {
                    // SavingsAccount = account,
                    Account = account.FundingAccount,
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

                if (sourceAccount.FundingAccount == null)
                {
                    throw new InvalidOperationException("Funding account was not found.");
                }

                sourceAccount.Balance = ZeroAmount;
                sourceAccount.AccountStatus = "Closed";
                destinationAccount.Balance += transferAmount;

                _dbContext.SavingsTransactions.Add(new SavingsTransaction
                {
                    // SavingsAccount = sourceAccount,
                    Account = sourceAccount.FundingAccount,
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

                if (account.FundingAccount == null)
                {
                    throw new InvalidOperationException("Funding account was not found.");
                }

                var newBalance = account.Balance - amount;
                account.Balance = newBalance;

                var withdrawalDescription = earlyWithdrawalPenalty > NoPenaltyAmount
                    ? $"To: {destinationLabel} | Early withdrawal penalty: {earlyWithdrawalPenalty:C2}"
                    : $"To: {destinationLabel}";

                _dbContext.SavingsTransactions.Add(new SavingsTransaction
                {
                    // SavingsAccount = account,
                    Account = account.FundingAccount,
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
            return await _dbContext.AutoDeposits
                .AsNoTracking()
                .Include(autoDeposit => autoDeposit.SavingsAccount)
                .ThenInclude(savingsAccount => savingsAccount.User)
                // .FirstOrDefaultAsync(x => x.SavingsAccount.IdentificationNumber == accountId);
                .FirstOrDefaultAsync(autoDeposit => autoDeposit.SavingsAccountId == accountId);
        }

        /// <summary>
        /// Creates or updates auto-deposit settings for a savings account.
        /// </summary>
        public async Task SaveAutoDepositAsync(AutoDeposit autoDeposit)
        {
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
        public Task<List<FundingSourceOption>> GetFundingSourcesAsync(int userId)
        {
            return Task.FromResult(
                new List<FundingSourceOption>
                {
                    new () { Id = PrimaryFundingSourceId, DisplayName = PrimaryFundingSourceName },
                    new () { Id = SecondaryFundingSourceId, DisplayName = SecondaryFundingSourceName },
                });
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
    }
}