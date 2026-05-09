namespace BankApp.Models.DTOs.Transactions
{
    public class TransactionHistoryRequest
    {
        public string? SearchTerm { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? TransactionType { get; set; }
        public decimal? MinimumAmount { get; set; }
        public decimal? MaximumAmount { get; set; }
        public int? AccountId { get; set; }
        public int? CardId { get; set; }
        public string? Status { get; set; }
        public string? Direction { get; set; }
        public string SortField { get; set; } = TransactionSortFields.Date;
        public string SortDirection { get; set; } = SortDirections.Desc;
    }
}
