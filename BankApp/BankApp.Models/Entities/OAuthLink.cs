using System.ComponentModel.DataAnnotations;

namespace BankApp.Models.Entities
{
    public class OAuthLink
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public string Provider { get; set; } = string.Empty;
        public string ProviderUserId { get; set; } = string.Empty;
        public string? ProviderEmail { get; set; }
        public DateTime LinkedAt { get; set; }

        public override bool Equals(object? obj)
        {
            OAuthLink other = obj as OAuthLink;

            return other != null &&
                   UserId == other.UserId &&
                   User == other.User &&
                   Provider == other.Provider &&
                   ProviderUserId == other.ProviderUserId &&
                   ProviderEmail == other.ProviderEmail &&
                   LinkedAt == other.LinkedAt;
        }
    }
}