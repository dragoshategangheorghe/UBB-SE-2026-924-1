using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace BankApp.Models.Entities
{
    [Table("Portfolio")]
    public class Portfolio
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("userId")]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        public virtual ICollection<InvestmentHolding> Holdings { get; set; } = new List<InvestmentHolding>();

        // Ignored properties (Calculated on the fly, not stored in columns directly)
        [NotMapped]
        public decimal TotalValue => this.Holdings?.Sum(h => h.Quantity * h.CurrentPrice) ?? 0m;

        [NotMapped]
        public decimal TotalCostBasis => this.Holdings?.Sum(h => h.Quantity * h.AvgPurchasePrice) ?? 0m;

        [NotMapped]
        public decimal TotalGainLoss => this.TotalValue - this.TotalCostBasis;

        [NotMapped]
        public decimal GainLossPercent => this.TotalCostBasis == 0 ? 0 : (this.TotalGainLoss / this.TotalCostBasis);
    }
}