using System.ComponentModel.DataAnnotations;

namespace BankApp.Models.Entities
{
    public class Card
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }
        public int AccountId { get; set; }
        public virtual Account Account { get; set; } = null!;
        public virtual User User { get; set; } = null!;
        public string CardNumber { get; set; } = string.Empty;
        public string CardholderName { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public string CVV { get; set; } = string.Empty;
        public string CardType { get; set; } = string.Empty;
        public string? CardBrand { get; set; }
        public string Status { get; set; } = "Active";
        public decimal? DailyTransactionLimit { get; set; }
        public decimal? MonthlySpendingCap { get; set; }
        public decimal? AtmWithdrawalLimit { get; set; }
        public decimal? ContactlessLimit { get; set; }
        public bool IsContactlessEnabled { get; set; } = true;
        public bool IsOnlineEnabled { get; set; } = true;
        public int SortOrder { get; set; }
        public DateTime? CancelledAt { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation Properties
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
