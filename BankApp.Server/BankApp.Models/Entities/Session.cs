namespace BankApp.Models.Entities
{
    public class Session
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public string? DeviceInfo { get; set; }
        public string? Browser { get; set; }
        public string? IpAddress { get; set; }
        public DateTime? LastActiveAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}