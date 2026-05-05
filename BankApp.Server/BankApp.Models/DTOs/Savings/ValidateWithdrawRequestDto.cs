using BankApp.Models.Features.Investments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApp.Models.DTOs.Savings
{
    public class ValidateWithdrawRequestDto
    {
        public decimal Amount { get; set; }
        public FundingSourceOption? Destination { get; set; }
    }
}
