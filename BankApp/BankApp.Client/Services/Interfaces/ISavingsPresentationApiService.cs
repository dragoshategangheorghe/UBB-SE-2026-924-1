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
        string GetTotalSavedAmount(IEnumerable<SavingsAccount> accounts);

        string GetNumberOfAccountsText(int accountCount);

        string GetBestInterestRate(IEnumerable<SavingsAccount> accounts);

        bool CheckClosePenaltyRisk(SavingsAccount selectedAccount);
    }
}
