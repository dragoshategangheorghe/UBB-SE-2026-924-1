using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BankApp.Client.RepoProxies.Interfaces;
using BankApp.Client.Services.Interfaces;
using BankApp.Models.DTOs.Savings;
using BankApp.Models.Enums;
using BankApp.Models.Features.Investments;
using BankApp.Models.Features.Savings;

namespace BankApp.Client.Services.Implementations
{
    public class SavingsService : ISavingsService
    {
        private const int MaxActiveAccounts = 5;
        private const int MinUserId = 0;
        private const decimal MinPositiveAmount = 0m;
        private const decimal NoPenalty = 0m;
        private const int MinPage = 1;
        private const int MaxPageSize = 100;
        private const int DefaultPageSize = 20;
        private const string InvalidPositiveAmountMessage = "Invalid amount. Please enter a positive number.";

        private const decimal FixedDepositApy = 0.04m;
        private const decimal GoalSavingsApy = 0.03m;
        private const decimal HighYieldApy = 0.03m;
        private const decimal DefaultApy = 0.02m;

        private const decimal DecimalEarlyClosurePenalty = 0.02m;
        private const decimal DecimalEarlyWithdrawalPenalty = 0.02m;

        private readonly ISavingsRepoProxy _savingsRepoProxy;
        private readonly ISavingsUiRulesRepoProxy _savingsUiRules;
        private readonly ISavingsPresentationRepoProxy _savingsPresentation;
        private readonly ISavingsWorkflowRepoProxy _savingsWorkflow;

        public SavingsService(
            ISavingsRepoProxy savingsRepoProxy,
            ISavingsUiRulesRepoProxy savingsUiRules,
            ISavingsPresentationRepoProxy savingsPresentation,
            ISavingsWorkflowRepoProxy savingsWorkflow)
        {
            _savingsRepoProxy = savingsRepoProxy ?? throw new ArgumentNullException(nameof(savingsRepoProxy));
            _savingsUiRules = savingsUiRules ?? throw new ArgumentNullException(nameof(savingsUiRules));
            _savingsPresentation = savingsPresentation ?? throw new ArgumentNullException(nameof(savingsPresentation));
            _savingsWorkflow = savingsWorkflow ?? throw new ArgumentNullException(nameof(savingsWorkflow));
        }

        public async Task<SavingsAccount> CreateAccountAsync(CreateSavingsAccountDto dto)
        {
            // Business rule: enforce max active accounts per user.
            var activeAccountsList = await _savingsRepoProxy.GetSavingsAccountsByUserIdAsync(dto.UserIdentificationNumber, false);
            if (activeAccountsList.Count >= MaxActiveAccounts)
            {
                throw new InvalidOperationException($"You cannot have more than {MaxActiveAccounts} active savings accounts.");
            }

            // Business rule: goal savings requires a future target date and positive target amount.
            if (dto.SavingsType == "GoalSavings")
            {
                if (!dto.TargetDate.HasValue)
                {
                    throw new ArgumentException("GoalSavings accounts require a target date.");
                }

                if (dto.TargetDate.Value <= DateTime.Today)
                {
                    throw new ArgumentException("Target date must be in the future.");
                }

                if (!dto.TargetAmount.HasValue || dto.TargetAmount.Value <= MinPositiveAmount)
                {
                    throw new ArgumentException("GoalSavings accounts require a positive target amount.");
                }
            }

            decimal apy = dto.SavingsType switch
            {
                "FixedDeposit" => FixedDepositApy,
                "GoalSavings" => GoalSavingsApy,
                "HighYield" => HighYieldApy,
                _ => DefaultApy,
            };

            return await _savingsRepoProxy.CreateSavingsAccountAsync(dto, apy);
        }

        public Task<List<SavingsAccount>> GetAccountsAsync(int userId, bool includesClosed = false)
        {
            if (userId < MinUserId)
            {
                throw new ArgumentException("User ID must be a positive integer.");
            }

            return _savingsRepoProxy.GetSavingsAccountsByUserIdAsync(userId, includesClosed);
        }

        public async Task<DepositResponseDto> DepositAsync(int accountId, decimal amount, string source, int userId)
        {
            if (amount <= MinPositiveAmount)
            {
                throw new ArgumentException("Deposit amount must be positive.");
            }

            // Business rule: validate ownership and status before deposit.
            var userAccountsList = await _savingsRepoProxy.GetSavingsAccountsByUserIdAsync(userId, true);
            var destinationAccount = userAccountsList.FirstOrDefault(account => account.IdentificationNumber == accountId)
                ?? throw new InvalidOperationException("Account not found or does not belong to you.");

            if (destinationAccount.AccountStatus == "Closed")
            {
                throw new InvalidOperationException("Cannot deposit into a closed account.");
            }

            if (destinationAccount.DisplayStatus == "Matured")
            {
                throw new InvalidOperationException("Cannot deposit into a matured account.");
            }

            return await _savingsRepoProxy.DepositAsync(accountId, amount, source);
        }

        public async Task<ClosureResultDto> CloseAccountAsync(int accountId, int destinationAccountId, int userId)
        {
            var userAccountsList = await _savingsRepoProxy.GetSavingsAccountsByUserIdAsync(userId, true);

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

            decimal earlyClosurePenalty = NoPenalty;
            if (closingAccount.SavingsType == "FixedDeposit" &&
                closingAccount.MaturityDate.HasValue &&
                closingAccount.MaturityDate > DateTime.UtcNow)
            {
                earlyClosurePenalty = closingAccount.Balance * DecimalEarlyClosurePenalty;
            }

            var transferAmount = closingAccount.Balance - earlyClosurePenalty;

            return await _savingsRepoProxy.CloseSavingsAccountAsync(
                accountId,
                destinationAccountId,
                transferAmount,
                earlyClosurePenalty);
        }

        public async Task<WithdrawResponseDto> WithdrawAsync(int accountId, decimal amount, string destinationLabel, int userId)
        {
            if (amount <= MinPositiveAmount)
            {
                throw new ArgumentException("Withdrawal amount must be positive.");
            }

            var userAccountsList = await _savingsRepoProxy.GetSavingsAccountsByUserIdAsync(userId, true);
            var destinationAccount = userAccountsList.FirstOrDefault(account => account.IdentificationNumber == accountId)
                ?? throw new InvalidOperationException("Account not found or does not belong to you.");

            if (destinationAccount.AccountStatus == "Closed")
            {
                throw new InvalidOperationException("Cannot withdraw from a closed account.");
            }

            if (destinationAccount.Balance < amount)
            {
                throw new InvalidOperationException("Insufficient balance.");
            }

            decimal earlyWithdrawalPenalty = NoPenalty;
            if (destinationAccount.SavingsType == "FixedDeposit" &&
                destinationAccount.MaturityDate.HasValue &&
                destinationAccount.MaturityDate.Value > DateTime.UtcNow)
            {
                earlyWithdrawalPenalty = amount * DecimalEarlyWithdrawalPenalty;
            }

            var totalSumToWithdraw = amount + earlyWithdrawalPenalty;
            if (totalSumToWithdraw > destinationAccount.Balance)
            {
                throw new InvalidOperationException("Insufficient balance after penalty.");
            }

            // Note: repository expects total amount debited; penalty recorded separately.
            return await _savingsRepoProxy.WithdrawAsync(
                accountId,
                totalSumToWithdraw,
                destinationLabel,
                earlyWithdrawalPenalty);
        }

        public Task<AutoDeposit> GetAutoDepositAsync(int accountId) => _savingsRepoProxy.GetAutoDepositAsync(accountId);

        public Task SaveAutoDepositAsync(AutoDeposit autoDeposit) => _savingsRepoProxy.SaveAutoDepositAsync(autoDeposit);

        public Task<List<FundingSourceOption>> GetFundingSourcesAsync(int userId) => _savingsRepoProxy.GetFundingSourcesAsync(userId);

        public async Task<GetTransactionsResponse> GetTransactionsAsync(int accountId, string filter = "", int page = 1, int pageSize = 20)
        {
            if (page < MinPage)
            {
                throw new ArgumentException("Page must be greater than or equal to one.");
            }

            if (pageSize <= MinUserId || pageSize > MaxPageSize)
            {
                pageSize = DefaultPageSize;
            }

            return await _savingsRepoProxy.GetTransactionsAsync(accountId, filter, page, pageSize);
        }

        public Task<List<SavingsAccount>> GetValidTransferDestinationsAsync(int currentAccountId, int userId)
            => _savingsRepoProxy.GetValidTransferDestinationsAsync(currentAccountId, userId);

        public Task<decimal> ComputeWithdrawalPenalty(decimal amount)
        {
            return Task.FromResult(amount * DecimalEarlyWithdrawalPenalty);
        }

        public Task<bool> HasRiskEarlyWithdrawal(SavingsAccount savingsAccount)
        {
            bool hasRisk = savingsAccount?.SavingsType == "FixedDeposit" &&
                           savingsAccount.MaturityDate.HasValue &&
                           savingsAccount.MaturityDate.Value > DateTime.UtcNow;
            return Task.FromResult(hasRisk);
        }

        public Task<decimal> GetPenaltyDecimalFor(string penaltyCase)
        {
            decimal penaltyRate = penaltyCase switch
            {
                "EarlyWithdrawal" => DecimalEarlyWithdrawalPenalty,
                "EarlyClosure" => DecimalEarlyClosurePenalty,
                _ => throw new ArgumentException("Invalid penalty case."),
            };

            return Task.FromResult(penaltyRate);
        }

        public Task<decimal> ParsePositiveAmountAsync(string text)
        {
            if (!TryParsePositiveAmount(text, out var amount))
            {
                throw new InvalidOperationException(InvalidPositiveAmountMessage);
            }

            return Task.FromResult(amount);
        }

        public Task<string> GetDepositPreviewAsync(string depositAmountText, SavingsAccount selectedAccount) =>
            _savingsUiRules.GetDepositPreview(depositAmountText, selectedAccount);

        public Task<decimal> GetWithdrawNetAmountAsync(decimal requestedAmount, decimal penalty) =>
            _savingsUiRules.GetWithdrawNetAmount(requestedAmount, penalty);

        public Task<DepositFrequency> ParseDepositFrequencyAsync(string frequencyText) =>
            _savingsUiRules.ParseDepositFrequency(frequencyText);

        public Task<int> GetTotalPagesAsync(int totalCount, int pageSize) =>
            _savingsUiRules.GetTotalPages(totalCount, pageSize);

        public Task<Dictionary<string, string>> ValidateCreateAccountAsync(ValidateCreateAccountRequest request) =>
            _savingsUiRules.ValidateCreateAccount(request);

        public Task<string> GetTotalSavedAmountAsync(IEnumerable<SavingsAccount> accounts) =>
            _savingsPresentation.GetTotalSavedAmount(accounts);

        public Task<string> GetNumberOfAccountsTextAsync(int accountCount) =>
            _savingsPresentation.GetNumberOfAccountsText(accountCount);

        public Task<string> GetBestInterestRateAsync(IEnumerable<SavingsAccount> accounts) =>
            _savingsPresentation.GetBestInterestRate(accounts);

        public Task<bool> CheckClosePenaltyRiskAsync(SavingsAccount selectedAccount) =>
            _savingsPresentation.CheckClosePenaltyRisk(selectedAccount);

        public Task<FundingSourceOption> GetDefaultFundingSourceAsync(IEnumerable<FundingSourceOption> fundingSources) =>
            _savingsWorkflow.GetDefaultFundingSource(fundingSources);

        public Task<int> GetDefaultCloseDestinationIdAsync(IEnumerable<SavingsAccount> destinationAccounts) =>
            _savingsWorkflow.GetDefaultCloseDestinationId(destinationAccounts);

        public Task<ValidationResponse> ValidateWithdrawRequestAsync(decimal amount, FundingSourceOption? destination) =>
            _savingsWorkflow.ValidateWithdrawRequest(amount, destination);

        public Task<string> BuildWithdrawResultMessageAsync(WithdrawResponseDto response) =>
            _savingsWorkflow.BuildWithdrawResultMessage(response);

        public Task<ValidationResponse> ValidateCloseConfirmationAsync(bool userConfirmed, int destinationId) =>
            _savingsWorkflow.ValidateCloseConfirmation(userConfirmed, destinationId);

        public Task<bool> CanMoveToNextPageAsync(int currentPage, int totalPages) =>
            _savingsWorkflow.CanMoveToNextPage(currentPage, totalPages);

        public Task<bool> CanMoveToPreviousPageAsync(int currentPage) =>
            _savingsWorkflow.CanMoveToPreviousPage(currentPage);

        private static bool TryParsePositiveAmount(string text, out decimal amount)
        {
            if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out amount) &&
                amount > MinPositiveAmount)
            {
                return true;
            }

            if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out amount) &&
                amount > MinPositiveAmount)
            {
                return true;
            }

            amount = MinPositiveAmount;
            return false;
        }
    }
}

