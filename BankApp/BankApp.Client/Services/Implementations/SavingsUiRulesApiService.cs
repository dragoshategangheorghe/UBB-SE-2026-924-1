using BankApp.Client.Services.Interfaces;
using BankApp.Client.Utilities;
using BankApp.Models.DTOs.Savings;
using BankApp.Models.Enums;
using BankApp.Models.Features.Savings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApp.Client.Services.Implementations
{
    public class SavingsUiRulesApiService : ISavingsUiRulesApiService
    {
        private readonly ApiService _apiService;

        public SavingsUiRulesApiService(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<string> GetDepositPreview(string depositAmountText, SavingsAccount selectedAccount)
        {
            return await _apiService.PostAsync<SavingsAccount, string>($"api/savings-ui-rules/deposit-preview?depositAmountText={depositAmountText}", selectedAccount);
        }

        public async Task<int> GetTotalPages(int totalCount, int pageSize)
        {
            return await _apiService.GetAsync<int>($"api/savings-ui-rules/total-pages?totalCount={totalCount}&pageSize={pageSize}");
        }

        public async Task<decimal> GetWithdrawNetAmount(decimal requestedAmount, decimal penalty)
        {
            return await _apiService.GetAsync<decimal>($"api/savings-ui-rules/withdraw-net-amount?requestedAmount={requestedAmount}&penalty={penalty}");
        }

        public async Task<DepositFrequency> ParseDepositFrequency(string frequencyText)
        {
            return await _apiService.GetAsync<DepositFrequency>($"api/savings-ui-rules/parse-deposit-frequency?frequencyText={frequencyText}");
        }

        public async Task<decimal> ParsePositiveAmount(string text)
        {
            return await _apiService.GetAsync<decimal>($"api/savings-ui-rules/parse-positive-amount?text={text}");
        }

        public async Task<Dictionary<string, string>> ValidateCreateAccount(ValidateCreateAccountRequest request)
        {
            return await _apiService.PostAsync<ValidateCreateAccountRequest, Dictionary<string, string>>($"api/savings-ui-rules/validate-create-account", request);
        }
    }
}
