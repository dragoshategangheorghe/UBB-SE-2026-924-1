using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankApp.Models.Entities
{
    public class UserCardPreference
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        public User User { get; set; }

        public string SortOption { get; set; } = string.Empty;

        public DateTime UpdatedAt { get; set; }
    }
}
