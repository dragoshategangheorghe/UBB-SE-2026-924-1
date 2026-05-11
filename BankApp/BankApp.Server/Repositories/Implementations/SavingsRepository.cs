using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BankApp.Models.DTOs;
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

        private readonly AppDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="SavingsRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The application's EF Core database context.</param>
        public SavingsRepository(AppDbContext dbContext)
        {
            _context = dbContext;
        }

        /// <summary>
        /// Gets savings accounts for a user through navigation mappings.
        /// </summary>
        public async Task<List<SavingsAccount>> GetSavingsAccountsByUserIdAsync(
            int userIdentificationNumber,
            bool includesClosedAccounts = false)
        {
            var query = _context.SavingsAccounts
                .AsNoTracking()
                .Include(a => a.User)
                .Include(a => a.FundingAccount)
                .Include(a => a.AutoDeposits)
                .Include(a => a.Transactions)
                .Where(a => a.User.Id == userIdentificationNumber);

            if (!includesClosedAccounts)
            {
                query = query.Where(a => a.AccountStatus != "Closed");
            }

            return await query.OrderByDescending(a => a.Balance).ToListAsync();
        }

        /// <summary>
        /// Creates a new savings account using EF Core and returns the created entity.
        /// </summary>
        public async Task<SavingsAccount> CreateSavingsAccountAsync(CreateSavingsAccountDto dataTransferObject, decimal annualPercentageYield)
        {
            var user = await _context.Users
                .Include(u => u.Accounts)
                .FirstOrDefaultAsync(u => u.Id == dataTransferObject.UserIdentificationNumber);

            if (user == null)
            {
                throw new InvalidOperationException("User was not found.");
            }

            var fundingAccount = dataTransferObject.FundingAccountId == NoFundingAccountId
                ? null
                : await _context.Accounts
                    .Include(a => a.User)
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

            _context.SavingsAccounts.Add(account);
            await _context.SaveChangesAsync();
            return account;
        }

        /// <summary>
        /// Deposits funds into a savings account and records a transaction row.
        /// </summary>
        public async Task<DepositResponseDto> DepositAsync(int accountIdentificationNumber, decimal amount, string source)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var account = await _context.SavingsAccounts
                    .Include(x => x.User)
                    .Include(x => x.FundingAccount)
                    .FirstOrDefaultAsync(x => x.IdentificationNumber == accountIdentificationNumber);

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
                    SavingsAccount = account,
                    Account = account.FundingAccount,
                    Amount = amount,
                    Type = TransactionType.Deposit,
                    Source = source ?? "Manual",
                    BalanceAfter = newAccountBalance,
                    CreatedAt = DateTime.UtcNow,
                };

                _context.SavingsTransactions.Add(savingsTransaction);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new DepositResponseDto
                {
                    NewBalance = newAccountBalance,
                    TransactionId = savingsTransaction.Id,
                    Timestamp = DateTime.UtcNow,
                };
            }
            catch
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
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var sourceAccount = await _context.SavingsAccounts
                    .Include(x => x.FundingAccount)
                    .Include(x => x.User)
                    .FirstOrDefaultAsync(x => x.IdentificationNumber == accountIdentificationNumber);

                var destinationAccount = await _context.SavingsAccounts
                    .Include(x => x.FundingAccount)
                    .Include(x => x.User)
                    .FirstOrDefaultAsync(x => x.IdentificationNumber == destinationAccountIdentificationNumber);

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

                _context.SavingsTransactions.Add(new SavingsTransaction
                {
                    SavingsAccount = sourceAccount,
                    Account = sourceAccount.FundingAccount,
                    Amount = transferAmount,
                    Type = TransactionType.Deposit,
                    Source = "Closure",
                    Description = "Account closed",
                    BalanceAfter = ZeroAmount,
                    CreatedAt = DateTime.UtcNow,
                });

                await _context.SaveChangesAsync();
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
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var account = await _context.SavingsAccounts
                    .Include(x => x.User)
                    .Include(x => x.FundingAccount)
                    .FirstOrDefaultAsync(x => x.IdentificationNumber == accountId);

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

                _context.SavingsTransactions.Add(new SavingsTransaction
                {
                    SavingsAccount = account,
                    Account = account.FundingAccount,
                    Amount = amount,
                    Type = TransactionType.Withdrawal,
                    Source = "Manual",
                    Description = withdrawalDescription,
                    BalanceAfter = newBalance,
                    CreatedAt = DateTime.UtcNow,
                });

                await _context.SaveChangesAsync();
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
            return await _context.AutoDeposits
                .AsNoTracking()
                .Include(x => x.SavingsAccount)
                .ThenInclude(x => x.User)
                // .FirstOrDefaultAsync(x => x.SavingsAccount.IdentificationNumber == accountId);
                .FirstOrDefaultAsync(x => x.SavingsAccountId == accountId);
        }

        /// <summary>
        /// Creates or updates auto-deposit settings for a savings account.
        /// </summary>
        public async Task SaveAutoDepositAsync(AutoDeposit autoDeposit)
        {
            if (autoDeposit.Id == NewAutoDepositId)
            {
                _context.AutoDeposits.Add(autoDeposit);
            }
            else
            {
                _context.AutoDeposits.Update(autoDeposit);
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Gets available funding-source options for a user.
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
            var query = _context.SavingsTransactions
                .AsNoTracking()
                .Include(x => x.Account)
                .Where(x => x.SavingsAccount != null &&
                            x.SavingsAccount.IdentificationNumber == accountId);

            if (!string.IsNullOrEmpty(typeFilter) && typeFilter != "All")
            {
                if (Enum.TryParse<TransactionType>(typeFilter, out var parsedType))
                {
                    query = query.Where(x => x.Type == parsedType);
                }
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - FirstPageNumber) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
