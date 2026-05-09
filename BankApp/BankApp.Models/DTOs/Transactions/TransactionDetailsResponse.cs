namespace BankApp.Models.DTOs.Transactions
{
    public class TransactionDetailsResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public TransactionHistoryItemDto? Transaction { get; set; }
    }
}
