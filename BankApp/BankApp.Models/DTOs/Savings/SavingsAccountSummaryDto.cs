using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApp.Models.DTOs.Savings
{
    public class SavingsAccountSummaryDto
    {
        public decimal Balance { get; set; }
        public decimal AnnualPercentageYield { get; set; }
        public string SavingsType { get; set; } = string.Empty;
        public DateTime? MaturityDate { get; set; }
    }
}
