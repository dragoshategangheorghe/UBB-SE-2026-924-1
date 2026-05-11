namespace BankApp.Models.DTOs.Cards
{
    public class RevealCardResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool RequiresOtp { get; set; }
        public int RevealDurationSeconds { get; set; }
        public SensitiveCardDetailsDto? SensitiveDetails { get; set; }
    }
}
