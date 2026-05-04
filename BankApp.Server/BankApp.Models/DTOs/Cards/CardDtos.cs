namespace BankApp.Models.DTOs.Cards
{
    public class GetCardsResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string SortOption { get; set; } = CardSortOptions.Custom;
        public List<CardSummaryDto> Cards { get; set; } = new ();
    }

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

    public class CardDetailsResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public CardSummaryDto? Card { get; set; }
    }

    public class RevealCardRequest
    {
        public string Password { get; set; } = string.Empty;
        public string? OtpCode { get; set; }
    }

    public class RevealCardResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool RequiresOtp { get; set; }
        public int RevealDurationSeconds { get; set; }
        public SensitiveCardDetailsDto? SensitiveDetails { get; set; }
    }

    public class SensitiveCardDetailsDto
    {
        public string CardNumber { get; set; } = string.Empty;
        public string Cvv { get; set; } = string.Empty;
    }

    public class UpdateCardSettingsRequest
    {
        public decimal? SpendingLimit { get; set; }
        public bool? IsOnlinePaymentsEnabled { get; set; }
        public bool? IsContactlessPaymentsEnabled { get; set; }
    }

    public class UpdateCardSortPreferenceRequest
    {
        public string SortOption { get; set; } = CardSortOptions.Custom;
    }

    public class CardCommandResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public CardSummaryDto? Card { get; set; }
    }

    public static class CardSortOptions
    {
        public const string Custom = "custom";
        public const string CardholderName = "cardholderName";
        public const string ExpiryDate = "expiryDate";
        public const string Status = "status";
    }
}
