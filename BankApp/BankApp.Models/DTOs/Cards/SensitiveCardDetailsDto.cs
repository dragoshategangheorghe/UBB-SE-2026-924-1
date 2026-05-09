namespace BankApp.Models.DTOs.Cards
{
    public class SensitiveCardDetailsDto
    {
        public string CardNumber { get; set; } = string.Empty;
        public string Cvv { get; set; } = string.Empty;
    }
}
