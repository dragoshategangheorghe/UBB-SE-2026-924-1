using System;
using System.Collections.Generic;
using System.Globalization;
using KarmaBanking.App.Models;
using KarmaBanking.App.Models.Enums;

namespace BankApp.Server.Services.Implementations
{
    public class SavingsUiRulesService
    {
        private const decimal PositiveAmountThreshold = 0m;
        private const int NoPages = 0;

        public bool TryParsePositiveAmount(string text, out decimal amount)
        {
            if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out amount) && amount > PositiveAmountThreshold)
            {
                return true;
            }

            amount = PositiveAmountThreshold;
            return false;
        }

        public string BuildDepositPreview(string depositAmountText, SavingsAccount? selectedAccount)
        {
            if (selectedAccount == null || !this.TryParsePositiveAmount(depositAmountText, out var amount))
            {
                return string.Empty;
            }

            return $"New balance will be: ${selectedAccount.Balance + amount:N2}";
        }

        public decimal CalculateWithdrawNetAmount(decimal requestedAmount, decimal penalty)
        {
            return requestedAmount - penalty;
        }

        public bool TryParseDepositFrequency(string frequencyText, out DepositFrequency frequency)
        {
            return Enum.TryParse(frequencyText, out frequency);
        }

        public int CalculateTotalPages(int totalCount, int pageSize)
        {
            if (pageSize <= NoPages)
            {
                return NoPages;
            }

            return (int)Math.Ceiling((double)totalCount / pageSize);
        }

        public Dictionary<string, string> ValidateCreateAccount(
            string selectedSavingsType,
            string accountName,
            string initialDepositText,
            bool hasFundingSource,
            string selectedFrequency,
            decimal? targetAmount,
            DateTimeOffset? targetDate,
            bool isGoalSavings)
        {
            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(selectedSavingsType))
            {
                errors["SavingsType"] = "Please select an account type.";
            }

            if (string.IsNullOrWhiteSpace(accountName))
            {
                errors["AccountName"] = "Account name is required.";
            }

            if (!this.TryParsePositiveAmount(initialDepositText, out _))
            {
                errors["InitialDeposit"] = "Initial deposit must be a positive number.";
            }

            if (!hasFundingSource)
            {
                errors["FundingSource"] = "Please select a funding source.";
            }

            if (string.IsNullOrWhiteSpace(selectedFrequency))
            {
                errors["Frequency"] = "Please select a deposit frequency.";
            }

            if (isGoalSavings)
            {
                if (!targetAmount.HasValue || targetAmount.Value <= PositiveAmountThreshold)
                {
                    errors["TargetAmount"] = "Target amount is required.";
                }

                if (!targetDate.HasValue)
                {
                    errors["TargetDate"] = "Target date is required.";
                }
                else if (targetDate.Value.Date <= DateTime.Today)
                {
                    errors["TargetDate"] = "Target date must be in the future.";
                }
            }

            return errors;
        }
    }
}