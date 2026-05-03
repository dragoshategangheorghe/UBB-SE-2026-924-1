namespace BankApp.Models.Entities
{
    public class Account
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? AccountName { get; set; }
        public string IBAN { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string AccountType { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public DateTime CreatedAt { get; set; }
    }
}
