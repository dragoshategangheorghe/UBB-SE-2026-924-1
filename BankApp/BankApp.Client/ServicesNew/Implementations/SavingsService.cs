using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BankApp.Models.DTOs.Savings;
using BankApp.Models.Features.Investments;
using BankApp.Models.Features.Savings;
using BankApp.Server.Repositories.Interfaces;
using BankApp.Server.Services.Interfaces;

namespace BankApp.Server.Services.Implementations
{
    public class SavingsService : ISavingsService
    {
        private const int MAX_ACTIVE_ACCOUNTS = 5;
        private const int MIN_USER_ID = 0;
        private const decimal MIN_POSITIVE_AMOUNT = 0m;
        private const decimal NO_PENALTY = 0m;
        private const int MIN_PAGE = 1;
        private const int MAX_PAGE_SIZE = 100;
        private const int DEFAULT_PAGE_SIZE = 20;

        private const decimal FIXED_DEPOSIT_APY = 0.04m;
        private const decimal GOAL_SAVINGS_APY = 0.03m;
        private const decimal HIGH_YIELD_APY = 0.03m;
        private const decimal DEFAULT_APY = 0.02m;

        private const decimal DECIMAL_EARLY_CLOSURE_PENALTY = 0.02m;
        private const decimal DECIMAL_EARLY_WITHDRAWAL_PENALTY = 0.02m;
        private readonly ISavingsRepository savingsRepository;

        public SavingsService(ISavingsRepository savingsRepository)
        {
            this.savingsRepository = savingsRepository;
        }

        public async Task<SavingsAccount> CreateAccountAsync(CreateSavingsAccountDto createSavingsAccountDto)
        {
            // Business rule: enforce max active accounts per user.
            //TODO i think this DTO shouldnt hold user ID, it cannot, its in the client now
            var activeAccountsList = await this.savingsRepository.GetSavingsAccountsByUserId(createSavingsAccountDto.UserIdentificationNumber, false);
            if (activeAccountsList.Count >= MAX_ACTIVE_ACCOUNTS)
            {
                throw new InvalidOperationException($"You cannot have more than {MAX_ACTIVE_ACCOUNTS} active savings accounts.");
            }

            // Business rule: goal savings requires a future target date.
            if (createSavingsAccountDto.SavingsType == "GoalSavings")
            {
                if (!createSavingsAccountDto.TargetDate.HasValue)
                {
                    throw new ArgumentException("GoalSavings accounts require a target date.");
                }

                if (createSavingsAccountDto.TargetDate.Value <= DateTime.Today)
                {
                    throw new ArgumentException("Target date must be in the future.");
                }

                if (!createSavingsAccountDto.TargetAmount.HasValue || createSavingsAccountDto.TargetAmount.Value <= MIN_POSITIVE_AMOUNT)
                {
                    throw new ArgumentException("GoalSavings accounts require a positive target amount.");
                }
            }

            var annualPercentageYield = createSavingsAccountDto.SavingsType switch
            {
                "FixedDeposit" => FIXED_DEPOSIT_APY,
                "GoalSavings" => GOAL_SAVINGS_APY,
                "HighYield" => HIGH_YIELD_APY,
                _ => DEFAULT_APY,
            };

            return await this.savingsRepository.CreateSavingsAccount(createSavingsAccountDto, annualPercentageYield);
        }

        public Task<List<SavingsAccount>> GetAccounts(int userId, bool includesClosed = false)
        {
            if (userId < MIN_USER_ID)
            {
                throw new ArgumentException("User ID must be a positive integer.");
            }

            return this.savingsRepository.GetSavingsAccountsByUserIdAsync(userId, includesClosed);
        }

        public async Task<DepositResponseDto> DepositAsync(int accountId, decimal amount, string source, int userId)
        {
            if (amount <= MIN_POSITIVE_AMOUNT)
            {
                throw new ArgumentException("Deposit amount must be positive.");
            }

            // Business rule: validate ownership and status before deposit.
            var userAccountsList = await this.savingsRepository.GetSavingsAccountsByUserIdAsync(userId, true);
            var destinationAccount = userAccountsList.Find(account => account.IdentificationNumber == accountId)
                                     ?? throw new InvalidOperationException("Account not found or does not belong to you.");

            // Business rule: blocked statuses cannot receive deposits.
            if (destinationAccount.AccountStatus == "Closed")
            {
                throw new InvalidOperationException("Cannot deposit into a closed account.");
            }

            if (destinationAccount.DisplayStatus == "Matured")
            {
                throw new InvalidOperationException("Cannot deposit into a matured account.");
            }

            return await this.savingsRepository.DepositAsync(accountId, amount, source);
        }

        public async Task<ClosureResultDto> CloseAccountAsync(int accountId, int destinationAccountId, int userId)
        {
            var userAccountsList = await this.savingsRepository.GetSavingsAccountsByUserIdAsync(userId, true);

            var closingAccount = userAccountsList.FirstOrDefault(account => account.IdentificationNumber == accountId)
                                 ?? throw new InvalidOperationException("Account not found.");

            if (closingAccount.AccountStatus == "Closed")
            {
                throw new InvalidOperationException("Account already closed.");
            }

            var destinationAccount = userAccountsList.FirstOrDefault(account => account.IdentificationNumber == destinationAccountId)
                                     ?? throw new InvalidOperationException("Destination account not found.");

            if (destinationAccount.AccountStatus == "Closed")
            {
                throw new InvalidOperationException("Cannot transfer to a closed account.");
            }

            decimal earlyClosurePenalty = NO_PENALTY;
            if (closingAccount.SavingsType == "FixedDeposit" &&
                closingAccount.MaturityDate.HasValue &&
                closingAccount.MaturityDate > DateTime.UtcNow)
            {
                earlyClosurePenalty = closingAccount.Balance * DECIMAL_EARLY_CLOSURE_PENALTY;
            }

            var transferAmount = closingAccount.Balance - earlyClosurePenalty;

            return await this.savingsRepository.CloseSavingsAccountAsync(
                accountId,
                destinationAccountId,
                transferAmount,
                earlyClosurePenalty);
        }

        public async Task<WithdrawResponseDto> WithdrawAsync(
            int accountId,
            decimal amount,
            string destinationLabel,
            int userId)
        {
            if (amount <= MIN_POSITIVE_AMOUNT)
            {
                throw new ArgumentException("Withdrawal amount must be positive.");
            }

            var userAccountsList = await this.savingsRepository.GetSavingsAccountsByUserIdAsync(userId, true);
            var destinationAccount = userAccountsList.Find(account => account.IdentificationNumber == accountId)
                                     ?? throw new InvalidOperationException("Account not found or does not belong to you.");

            if (destinationAccount.AccountStatus == "Closed")
            {
                throw new InvalidOperationException("Cannot withdraw from a closed account.");
            }

            if (destinationAccount.Balance < amount)
            {
                throw new InvalidOperationException("Insufficient balance.");
            }

            decimal earlyWithdrawalPenalty = NO_PENALTY;
            if (destinationAccount.SavingsType == "FixedDeposit" &&
                destinationAccount.MaturityDate.HasValue &&
                destinationAccount.MaturityDate.Value > DateTime.UtcNow)
            {
                earlyWithdrawalPenalty = amount * DECIMAL_EARLY_WITHDRAWAL_PENALTY;
            }

            var totalSumToWithdraw = amount + earlyWithdrawalPenalty;
            if (totalSumToWithdraw > destinationAccount.Balance)
            {
                throw new InvalidOperationException("Insufficient balance after penalty.");
            }

            return await this.savingsRepository.WithdrawAsync(
                accountId,
                totalSumToWithdraw,
                destinationLabel,
                earlyWithdrawalPenalty);
        }

        public Task<AutoDeposit?> GetAutoDepositAsync(int accountId)
        {
            return this.savingsRepository.GetAutoDepositAsync(accountId);
        }

        public Task SaveAutoDepositAsync(AutoDeposit autoDeposit)
        {
            return this.savingsRepository.SaveAutoDepositAsync(autoDeposit);
        }

        public Task<List<FundingSourceOption>> GetFundingSourcesAsync(int userId)
        {
            return this.savingsRepository.GetFundingSourcesAsync(userId);
        }

        public async Task<(List<SavingsTransaction> Items, int TotalCount)> GetTransactionsAsync(
            int accountId,
            string filter,
            int page,
            int pageSize)
        {
            if (page < MIN_PAGE)
            {
                throw new ArgumentException("Page must be greater than or equal to one.");
            }

            if (pageSize <= MIN_USER_ID || pageSize > MAX_PAGE_SIZE)
            {
                pageSize = DEFAULT_PAGE_SIZE;
            }

            return await this.savingsRepository.GetTransactionsPagedAsync(
                accountId,
                filter,
                page,
                pageSize);
        }

        public async Task<List<SavingsAccount>> GetValidTransferDestinationsAsync(int currentAccountId)
        {
            var openAccountsList = await this.savingsRepository.GetSavingsAccountsByUserIdAsync(currentAccountId);
            return openAccountsList.Where(account => account.IdentificationNumber != currentAccountId).ToList();
        }

        public decimal ComputeWithdrawalPenalty(decimal amount)
        {
            return amount * DECIMAL_EARLY_WITHDRAWAL_PENALTY;
        }

        public bool HasRiskEarlyWithdrawal(SavingsAccount savingsAccount)
        {
            return savingsAccount?.SavingsType == "FixedDeposit" &&
                   savingsAccount.MaturityDate.HasValue &&
                   savingsAccount.MaturityDate.Value > DateTime.UtcNow;
        }

        public decimal GetPenaltyDecimalFor(string penaltyCase)
        {
            return penaltyCase switch
            {
                "EarlyWithdrawal" => DECIMAL_EARLY_WITHDRAWAL_PENALTY,
                "EarlyClosure" => DECIMAL_EARLY_CLOSURE_PENALTY,
                _ => throw new ArgumentException("Invalid penalty case."),
            };
        }
    }
}