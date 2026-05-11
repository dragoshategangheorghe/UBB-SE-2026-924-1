namespace BankApp.Models.DTOs.Transactions
{
    public class TransactionExportRequest : TransactionHistoryRequest
    {
        public string Format { get; set; } = TransactionExportFormats.Csv;
    }
}
