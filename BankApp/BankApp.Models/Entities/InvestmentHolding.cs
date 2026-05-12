using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
namespace BankApp.Models.Entities
{
    public class InvestmentHolding
    {
        [Key]
        public int IdentificationNumber { get; set; }
        public int PortfolioId { get; set; }
        public virtual Portfolio Portfolio { get; set; } = null!;
        public string Ticker { get; set; } = string.Empty;
        public string AssetType { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal AveragePurchasePrice { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal UnrealizedGainLoss { get; set; }
        public virtual ICollection<InvestmentTransaction> Transactions { get; set; } = new List<InvestmentTransaction>();
    }
}