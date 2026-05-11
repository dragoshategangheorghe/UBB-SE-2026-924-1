using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
namespace BankApp.Models.Entities
{
    public class Portfolio
    {
        [Key]
        public int IdentificationNumber { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;
        public virtual ICollection<InvestmentHolding> Holdings { get; set; } = new List<InvestmentHolding>();

        public decimal TotalValue => this.Holdings?.Sum(h => h.Quantity * h.CurrentPrice) ?? 0m;
        public decimal TotalCostBasis => this.Holdings?.Sum(h => h.Quantity * h.AveragePurchasePrice) ?? 0m;
        public decimal TotalGainLoss => this.TotalValue - this.TotalCostBasis;
        public decimal GainLossPercent => this.TotalCostBasis == 0 ? 0 : (this.TotalGainLoss / this.TotalCostBasis);
    }
}