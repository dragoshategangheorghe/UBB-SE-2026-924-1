namespace BankApp.Models.DTOs.Transactions
{
    public class TransactionHistoryResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public TransactionHistoryRequest AppliedFilters { get; set; } = new ();
        public List<TransactionHistoryItemDto> Transactions { get; set; } = new ();
    }
}
