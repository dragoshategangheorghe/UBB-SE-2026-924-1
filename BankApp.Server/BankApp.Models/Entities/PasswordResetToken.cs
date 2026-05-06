namespace BankApp.Models.Entities
{
    public class PasswordResetToken
    {
        public int Id { get; set; }
        public virtual User User { get; set; }
        public string TokenHash { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime? UsedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
