using System;
using System.Collections.Generic;
using System.Linq;
using BankApp.Models.Features.Savings;

namespace BankApp.Server.Services.Implementations
{
    public class SavingsPresentationService
    {
        private const int SingularAccountCount = 1;
        private const decimal DefaultBestApy = 0m;
        private const decimal PercentageScale = 100m;

        public string BuildTotalSavedAmount(IEnumerable<SavingsAccount> accounts)
        {
            return $"${accounts.Sum(account => account.Balance):F2}";
        }

        public string BuildNumberOfAccountsText(int accountCount)
        {
            return $"across {accountCount} account{(accountCount == SingularAccountCount ? string.Empty : "s")}";
        }

        public string BuildBestInterestRate(IEnumerable<SavingsAccount> accounts)
        {
            var bestApy = accounts.Any() ? accounts.Max(account => account.AnnualPercentageYield) : DefaultBestApy;
            return $"{bestApy * PercentageScale:F2}%";
        }

        public bool HasClosePenaltyRisk(SavingsAccount? selectedAccount)
        {
            return selectedAccount?.SavingsType == "FixedDeposit" &&
                   selectedAccount.MaturityDate.HasValue &&
                   selectedAccount.MaturityDate.Value > DateTime.UtcNow;
        }
    }
}