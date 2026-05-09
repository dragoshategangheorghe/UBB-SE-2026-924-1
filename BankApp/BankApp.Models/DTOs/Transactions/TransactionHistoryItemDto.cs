namespace BankApp.Models.DTOs.Transactions
{
    public class TransactionHistoryItemDto
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public int? CardId { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public string AccountIban { get; set; } = string.Empty;
        public string? CardLabel { get; set; }
        public DateTime Timestamp { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public string ReferenceNumber { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string CounterpartyOrMerchant { get; set; } = string.Empty;
        public string? MerchantName { get; set; }
        public string? CounterpartyName { get; set; }
        public string? SourceAccountIban { get; set; }
        public string? DestinationAccountIban { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Direction { get; set; } = string.Empty;
        public decimal RunningBalanceAfterTransaction { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Fee { get; set; }
        public decimal? ExchangeRate { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }
}
