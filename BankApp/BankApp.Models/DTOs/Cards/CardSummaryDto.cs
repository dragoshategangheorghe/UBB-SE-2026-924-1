namespace BankApp.Models.DTOs.Cards
{
    public class CardSummaryDto
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public string AccountIban { get; set; } = string.Empty;
        public string MaskedCardNumber { get; set; } = string.Empty;
        public string CardholderName { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public string CardType { get; set; } = string.Empty;
        public string CardBrand { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal? SpendingLimit { get; set; }
        public bool IsOnlinePaymentsEnabled { get; set; }
        public bool IsContactlessPaymentsEnabled { get; set; }
        public int SortOrder { get; set; }
    }
}
