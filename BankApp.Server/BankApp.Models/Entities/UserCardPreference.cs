namespace BankApp.Models.Entities
{
    public class UserCardPreference
    {
        public int UserId { get; set; }
        public string SortOption { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
    }
}
