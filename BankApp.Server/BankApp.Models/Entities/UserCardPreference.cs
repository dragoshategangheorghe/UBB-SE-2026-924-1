namespace BankApp.Models.Entities
{
    public class UserCardPreference
    {
        public int UserId { get; set; }

        public User User { get; set; } = null!;

        public string SortOption { get; set; } = string.Empty;

        public DateTime UpdatedAt { get; set; }
    }
}
