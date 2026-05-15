using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankApp.Models.Entities
{
    /// <summary>
    /// Represents a specific asset holding within a user's investment portfolio.
    /// </summary>
    [Table("InvestmentHolding")]
    public class InvestmentHolding
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("portfolioId")]
        public int PortfolioId { get; set; }

        [Column("ticker")]
        public string Ticker { get; set; } = string.Empty;

        [Column("assetType")]
        public string AssetType { get; set; } = string.Empty;

        [Column("quantity")]
        public decimal Quantity { get; set; }

        [Column("avgPurchasePrice")]
        public decimal AvgPurchasePrice { get; set; }

        [Column("currentPrice")]
        public decimal CurrentPrice { get; set; }

        [Column("unrealizedGainLoss")]
        public decimal UnrealizedGainLoss { get; set; }

        [ForeignKey("PortfolioId")]
        public virtual Portfolio Portfolio { get; set; } = null!;

        public virtual ICollection<InvestmentTransaction> Transactions { get; set; } = new List<InvestmentTransaction>();
    }
}