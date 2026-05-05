using BankApp.Models.Features.Savings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApp.Models.DTOs.Savings
{
    public class DepositPreviewRequest
    {
        public string DepositAmountText { get; set; } = string.Empty;
        public SavingsAccount? SelectedAccount { get; set; }
    }
}