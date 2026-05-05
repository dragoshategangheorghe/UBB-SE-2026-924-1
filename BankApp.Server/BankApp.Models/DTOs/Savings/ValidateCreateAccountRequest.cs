using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApp.Models.DTOs.Savings
{
    public class ValidateCreateAccountRequest
    {
        public string SelectedSavingsType { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string InitialDepositText { get; set; } = string.Empty;
        public bool HasFundingSource { get; set; }
        public string SelectedFrequency { get; set; } = string.Empty;
        public decimal? TargetAmount { get; set; }
        public DateTimeOffset? TargetDate { get; set; }
        public bool IsGoalSavings { get; set; }
    }
}
