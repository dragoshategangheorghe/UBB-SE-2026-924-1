using System.ComponentModel.DataAnnotations;

namespace BankApp.Models.Entities
{
    public class UserCardPreference
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public string SortOption { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
    }
}
