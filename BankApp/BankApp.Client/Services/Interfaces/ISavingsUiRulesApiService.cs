using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BankApp.Models.DTOs.Savings;
using BankApp.Models.Enums;

namespace BankApp.Client.Services.Interfaces
{
    public interface ISavingsUiRulesApiService
    {
        Task<decimal> ParsePositiveAmount(string text);

        Task<string> GetDepositPreview(DepositPreviewRequest request);

        Task<decimal> GetWithdrawNetAmount(decimal requestedAmount, decimal penalty);

        Task<DepositFrequency> ParseDepositFrequency(string frequencyText);

        Task<int> GetTotalPages(int totalCount, int pageSize);

        Task<Dictionary<string, string>> ValidateCreateAccount(ValidateCreateAccountRequest request);
    }
}
