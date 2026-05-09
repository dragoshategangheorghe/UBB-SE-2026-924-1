namespace BankApp.Models.DTOs.Cards
{
    public class RevealCardRequest
    {
        public string Password { get; set; } = string.Empty;
        public string? OtpCode { get; set; }
    }
}
