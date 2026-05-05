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

    public class TransactionExportRequest : TransactionHistoryRequest
    {
        public string Format { get; set; } = TransactionExportFormats.Csv;
    }

    public class TransactionHistoryResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public TransactionHistoryRequest AppliedFilters { get; set; } = new ();
        public List<TransactionHistoryItemDto> Transactions { get; set; } = new ();
    }

    public class TransactionDetailsResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public TransactionHistoryItemDto? Transaction { get; set; }
    }

    public class TransactionFilterMetadataResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<AccountFilterOptionDto> Accounts { get; set; } = new ();
        public List<CardFilterOptionDto> Cards { get; set; } = new ();
        public List<string> AvailableTransactionTypes { get; set; } = new ();
        public List<string> AvailableStatuses { get; set; } = new ();
        public List<string> AvailableDirections { get; set; } = new ();
    }

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

    public class AccountFilterOptionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Iban { get; set; } = string.Empty;
    }

    public class CardFilterOptionDto
    {
        public int Id { get; set; }
        public string Label { get; set; } = string.Empty;
    }

    public static class TransactionSortFields
    {
        public const string Date = "date";
        public const string Amount = "amount";
    }

    public static class SortDirections
    {
        public const string Asc = "asc";
        public const string Desc = "desc";
    }

    public static class TransactionExportFormats
    {
        public const string Csv = "csv";
        public const string Pdf = "pdf";
        public const string Xlsx = "xlsx";
    }
}
