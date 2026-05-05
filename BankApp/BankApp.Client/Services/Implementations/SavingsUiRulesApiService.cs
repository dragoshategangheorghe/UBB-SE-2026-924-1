using BankApp.Client.Services.Interfaces;
using BankApp.Client.Utilities;
using BankApp.Models.Enums;
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

        public string GetDepositPreview(DepositPreviewRequest request)
        {
            
        }

        public int GetTotalPages(int totalCount, int pageSize)
        {
            throw new NotImplementedException();
        }

        public decimal GetWithdrawNetAmount(decimal requestedAmount, decimal penalty)
        {
            throw new NotImplementedException();
        }

        public DepositFrequency ParseDepositFrequency(string frequencyText)
        {
            throw new NotImplementedException();
        }

        public decimal ParsePositiveAmount(string text)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, string> ValidateCreateAccount(ValidateCreateAccountRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
