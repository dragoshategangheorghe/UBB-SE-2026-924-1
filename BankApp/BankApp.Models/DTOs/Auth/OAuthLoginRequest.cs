namespace BankApp.Models.DTOs.Auth
{
    public class OAuthLoginRequest
    {
        public string Provider { get; set; } = string.Empty;
        public string ProviderToken { get; set; } = string.Empty;
    }
}