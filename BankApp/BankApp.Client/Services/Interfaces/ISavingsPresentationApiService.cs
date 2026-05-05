using BankApp.Models.Features.Savings;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApp.Client.Services.Interfaces
{
    public interface ISavingsPresentationApiService
    {
        Task<string> GetTotalSavedAmount(IEnumerable<SavingsAccount> accounts);

        Task<string> GetNumberOfAccountsText(int accountCount);

        Task<string> GetBestInterestRate(IEnumerable<SavingsAccount> accounts);

        Task<bool> CheckClosePenaltyRisk(SavingsAccount selectedAccount);
    }
}
