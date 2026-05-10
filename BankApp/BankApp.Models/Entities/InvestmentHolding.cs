namespace BankApp.Models.Entities
{
    using System.Collections.Generic;

    public class InvestmentHolding
    {
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