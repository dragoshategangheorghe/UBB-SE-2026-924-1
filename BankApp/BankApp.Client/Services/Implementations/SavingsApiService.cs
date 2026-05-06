using BankApp.Client.Services.Interfaces;
using BankApp.Client.Utilities;
using BankApp.Models.DTOs.Savings;
using BankApp.Models.Features.Investments;
using BankApp.Models.Features.Savings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApp.Client.Services.Implementations
{
    public class SavingsApiService : ISavingsApiService
    {
        private readonly ApiService _apiService;

        public SavingsApiService(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<ClosureResultDto> CloseAccountAsync(int accountId, int destinationAccountId, int userId)
        {
            return await _apiService.GetAsync<ClosureResultDto>($"api/savings/{accountId}/close?destinationAccountId={destinationAccountId}&userId={userId}");
        }

        public async Task<decimal> ComputeWithdrawalPenalty(decimal amount)
        {
            return await _apiService.GetAsync<decimal>($"api/savings/withdrawal/compute-penalty?amount={amount}");
        }

        public async Task<SavingsAccount> CreateAccountAsync(CreateSavingsAccountDto account)
        {
            return await _apiService.PostAsync<CreateSavingsAccountDto, SavingsAccount>("api/savings/create-account", account);
        }

        public async Task<DepositResponseDto> DepositAsync(int accountId, decimal amount, string source, int userId)
        {
            return await _apiService.GetAsync<DepositResponseDto>($"api/savings/{accountId}/deposit?amount={amount}&source={source}&userId={userId}");
        }

        public async Task<List<SavingsAccount>> GetAccountsAsync(int userId, bool includesClosed = false)
        {
            return await _apiService.GetAsync<List<SavingsAccount>>($"api/savings/user/{userId}?includesClosed={includesClosed}");
        }

        public async Task<AutoDeposit> GetAutoDepositAsync(int accountId)
        {
            return await _apiService.GetAsync<AutoDeposit>($"api/savings/{accountId}/auto-deposit");
        }

        public async Task<List<FundingSourceOption>> GetFundingSourcesAsync(int userId)
        {
            return await _apiService.GetAsync<List<FundingSourceOption>>($"api/savings/user/{userId}/funding-sources");
        }

        public async Task<decimal> GetPenaltyDecimalFor(string penaltyCase)
        {
            return await _apiService.GetAsync<decimal>($"api/savings/penalty/rate/{penaltyCase}");
        }

        public async Task<GetTransactionsResponse> GetTransactionsAsync(int accountId, string filter = "", int page = 1, int pageSize = 20)
        {
            return await _apiService.GetAsync<GetTransactionsResponse>($"api/savings/{accountId}/transactions?filter={filter}&page={page}&pageSize={pageSize}");
        }

        public async Task<List<SavingsAccount>> GetValidTransferDestinationsAsync(int currentAccountId)
        {
            return await _apiService.GetAsync<List<SavingsAccount>>($"api/savings/{currentAccountId}/valid-destinations");
        }

        public async Task<bool> HasRiskEarlyWithdrawal(SavingsAccount savingsAccount)
        {
            return await _apiService.PostAsync<SavingsAccount, bool>("api/savings/risk-early-withdrawal", savingsAccount);
        }

        public async Task SaveAutoDepositAsync(AutoDeposit autoDeposit)
        {
            await _apiService.PostAsync<AutoDeposit, Task>("api/savings/auto-deposit", autoDeposit);
        }

        public async Task<WithdrawResponseDto> WithdrawAsync(int accountId, decimal amount, string destinationLabel, int userId)
        {
            return await _apiService.GetAsync<WithdrawResponseDto>($"api/savings/{accountId}/withdraw?amount={amount}&destinationLabel={destinationLabel}&userId={userId}");
        }
    }
}
