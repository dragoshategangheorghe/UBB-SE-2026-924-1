using System.ComponentModel.DataAnnotations;

namespace BankApp.Models.Entities
{
    public class Account
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;
        public string? AccountName { get; set; }
        public string IBAN { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string AccountType { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public DateTime CreatedAt { get; set; }

        // Navigation Properties
        public virtual ICollection<Card> Cards { get; set; } = new List<Card>();
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
