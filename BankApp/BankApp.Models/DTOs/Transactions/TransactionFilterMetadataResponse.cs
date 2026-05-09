namespace BankApp.Models.DTOs.Transactions
{
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
}
