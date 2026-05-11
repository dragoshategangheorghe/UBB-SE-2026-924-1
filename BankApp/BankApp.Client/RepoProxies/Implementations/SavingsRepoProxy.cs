using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using BankApp.Client.RepoProxies;
using BankApp.Client.RepoProxies.Interfaces;
using BankApp.Models.DTOs.Savings;
using BankApp.Models.Features.Investments;
using BankApp.Models.Features.Savings;

namespace BankApp.Client.RepoProxies.Implementations
{
    public class SavingsRepoProxy : ISavingsRepoProxy
    {
        private readonly ApiService _apiService;

        public SavingsRepoProxy(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<ClosureResultDto> CloseSavingsAccountAsync(int accountId, int destinationAccountId, decimal transferAmount, decimal earlyClosurePenalty)
        {
            var transferAmountText = transferAmount.ToString(CultureInfo.InvariantCulture);
            var earlyClosurePenaltyText = earlyClosurePenalty.ToString(CultureInfo.InvariantCulture);

            return await _apiService.PostAsync<object, ClosureResultDto>(
                $"/api/savings/{accountId}/close?destinationAccountId={destinationAccountId}&transferAmount={transferAmountText}&earlyClosurePenalty={earlyClosurePenaltyText}",
                new { });
        }

        public async Task<SavingsAccount> CreateSavingsAccountAsync(CreateSavingsAccountDto account, decimal apy)
        {
            var apyText = apy.ToString(CultureInfo.InvariantCulture);
            return await _apiService.PostAsync<CreateSavingsAccountDto, SavingsAccount>($"/api/savings/create-account?apy={apyText}", account);
        }

        public async Task<DepositResponseDto> DepositAsync(int accountId, decimal amount, string source)
        {
            var amountText = amount.ToString(CultureInfo.InvariantCulture);

            return await _apiService.PostAsync<object, DepositResponseDto>(
                $"/api/savings/{accountId}/deposit?amount={amountText}&source={Uri.EscapeDataString(source)}",
                new { });
        }

        public async Task<List<SavingsAccount>> GetSavingsAccountsByUserIdAsync(int userId, bool includesClosed = false)
        {
            return await _apiService.GetAsync<List<SavingsAccount>>($"/api/savings/user/{userId}?includesClosed={includesClosed}");
        }

        public async Task<AutoDeposit> GetAutoDepositAsync(int accountId)
        {
            return await _apiService.GetAsync<AutoDeposit>($"/api/savings/{accountId}/auto-deposit");
        }

        public async Task<List<FundingSourceOption>> GetFundingSourcesAsync(int userId)
        {
            return await _apiService.GetAsync<List<FundingSourceOption>>($"/api/savings/user/{userId}/funding-sources");
        }

        public async Task<decimal> GetPenaltyDecimalFor(string penaltyCase)
        {
            return await _apiService.GetAsync<decimal>($"/api/savings/penalty/rate/{penaltyCase}");
        }

        public async Task<GetTransactionsResponse> GetTransactionsAsync(int accountId, string filter = "", int page = 1, int pageSize = 20)
        {
            return await _apiService.GetAsync<GetTransactionsResponse>($"/api/savings/{accountId}/transactions?filter={filter}&page={page}&pageSize={pageSize}");
        }

        public async Task<List<SavingsAccount>> GetValidTransferDestinationsAsync(int currentAccountId, int userId)
        {
            return await _apiService.GetAsync<List<SavingsAccount>>($"/api/savings/{currentAccountId}/valid-destinations?userId={userId}");
        }

        public async Task SaveAutoDepositAsync(AutoDeposit autoDeposit)
        {
            await _apiService.PostAsync<AutoDeposit, Task>("/api/savings/auto-deposit", autoDeposit);
        }

        public async Task<WithdrawResponseDto> WithdrawAsync(int accountId, decimal amount, string destinationLabel, decimal earlyWithdrawalPenalty)
        {
            var amountText = amount.ToString(CultureInfo.InvariantCulture);
            var earlyWithdrawalPenaltyText = earlyWithdrawalPenalty.ToString(CultureInfo.InvariantCulture);

            return await _apiService.PostAsync<object, WithdrawResponseDto>(
                $"/api/savings/{accountId}/withdraw?amount={amountText}&destinationLabel={Uri.EscapeDataString(destinationLabel)}&earlyWithdrawalPenalty={earlyWithdrawalPenaltyText}",
                new { });
        }
    }
}
