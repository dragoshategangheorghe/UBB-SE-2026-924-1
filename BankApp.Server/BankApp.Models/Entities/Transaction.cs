namespace BankApp.Models.Entities
{
    public class Transaction
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public int? CardId { get; set; }
        public string TransactionRef { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Direction { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public decimal BalanceAfter { get; set; }
        public string? CounterpartyName { get; set; }
        public string? CounterpartyIBAN { get; set; }
        public string? MerchantName { get; set; }
        public int? CategoryId { get; set; }
        public string? Description { get; set; }
        public decimal Fee { get; set; }
        public decimal? ExchangeRate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? RelatedEntityType { get; set; }
        public int? RelatedEntityId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}