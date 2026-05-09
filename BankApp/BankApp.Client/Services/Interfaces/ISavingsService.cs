using System.Collections.Generic;
using System.Threading.Tasks;
using BankApp.Models.DTOs.Savings;
using BankApp.Models.Enums;
using BankApp.Models.Features.Investments;
using BankApp.Models.Features.Savings;

namespace BankApp.Client.Services.Interfaces
{
    public interface ISavingsService
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
        Task<List<SavingsAccount>> GetValidTransferDestinationsAsync(int currentAccountId, int userId);

        Task<decimal> ComputeWithdrawalPenalty(decimal amount);
        Task<bool> HasRiskEarlyWithdrawal(SavingsAccount savingsAccount);
        Task<decimal> GetPenaltyDecimalFor(string penaltyCase);

        // Presentation / workflow (repo proxies live inside SavingsService — UI uses only this interface).
        Task<decimal> ParsePositiveAmountAsync(string text);
        Task<string> GetDepositPreviewAsync(string depositAmountText, SavingsAccount selectedAccount);
        Task<decimal> GetWithdrawNetAmountAsync(decimal requestedAmount, decimal penalty);
        Task<DepositFrequency> ParseDepositFrequencyAsync(string frequencyText);
        Task<int> GetTotalPagesAsync(int totalCount, int pageSize);
        Task<Dictionary<string, string>> ValidateCreateAccountAsync(ValidateCreateAccountRequest request);
        Task<string> GetTotalSavedAmountAsync(IEnumerable<SavingsAccount> accounts);
        Task<string> GetNumberOfAccountsTextAsync(int accountCount);
        Task<string> GetBestInterestRateAsync(IEnumerable<SavingsAccount> accounts);
        Task<bool> CheckClosePenaltyRiskAsync(SavingsAccount selectedAccount);
        Task<FundingSourceOption> GetDefaultFundingSourceAsync(IEnumerable<FundingSourceOption> fundingSources);
        Task<int> GetDefaultCloseDestinationIdAsync(IEnumerable<SavingsAccount> destinationAccounts);
        Task<ValidationResponse> ValidateWithdrawRequestAsync(decimal amount, FundingSourceOption? destination);
        Task<string> BuildWithdrawResultMessageAsync(WithdrawResponseDto response);
        Task<ValidationResponse> ValidateCloseConfirmationAsync(bool userConfirmed, int destinationId);
        Task<bool> CanMoveToNextPageAsync(int currentPage, int totalPages);
        Task<bool> CanMoveToPreviousPageAsync(int currentPage);
    }
}

