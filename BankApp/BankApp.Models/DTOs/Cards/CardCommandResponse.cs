namespace BankApp.Models.DTOs.Cards
{
    public class CardCommandResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public CardSummaryDto? Card { get; set; }
    }
}
