using System;
using System.ComponentModel.DataAnnotations;
namespace BankApp.Models.Entities
{
    public class InvestmentTransaction
    {
        [Key]
        public int IdentificationNumber { get; set; }
        public int HoldingId { get; set; }
        public virtual InvestmentHolding Holding { get; set; } = null!;
        public string Ticker { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal PricePerUnit { get; set; }
        public decimal Fees { get; set; }
        public string OrderType { get; set; } = "Market";
        public DateTime ExecutedAt { get; set; }
    }
}