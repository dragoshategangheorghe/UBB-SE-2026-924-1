namespace BankApp.Models.DTOs.Auth
{
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public bool Requires2FA { get; set; }
        public int? UserId { get; set; }
        public string? Error { get; set; }
    }
}