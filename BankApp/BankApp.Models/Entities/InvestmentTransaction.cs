using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankApp.Models.Entities
{
    /// <summary>
    /// Represents a specific trade or transaction within an investment holding.
    /// </summary>
    [Table("InvestmentTransaction")]
    public class InvestmentTransaction
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("holdingId")]
        public int HoldingId { get; set; }

        [ForeignKey(nameof(HoldingId))] // Fixed case-sensitivity bug
        public virtual InvestmentHolding Holding { get; set; } = null!;

        [Column("ticker")]
        public string Ticker { get; set; } = string.Empty;

        [Column("actionType")]
        public string ActionType { get; set; } = string.Empty; // BUY, SELL

        [Column("quantity")]
        public decimal Quantity { get; set; }

        [Column("pricePerUnit")]
        public decimal PricePerUnit { get; set; }

        [Column("fees")]
        public decimal Fees { get; set; }

        [Column("orderType")]
        public string OrderType { get; set; } = "Market";

        [Column("executedAt")]
        public DateTime ExecutedAt { get; set; }
    }
}