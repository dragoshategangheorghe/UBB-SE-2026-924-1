namespace BankApp.Models.Entities
{
    public class OAuthLink
    {
        public int Id { get; set; }
        public virtual User User { get; set; }
        public string Provider { get; set; } = string.Empty;
        public string ProviderUserId { get; set; } = string.Empty;
        public string? ProviderEmail { get; set; }
        public DateTime LinkedAt { get; set; }

        public override bool Equals(object? obj)
        {
            OAuthLink other = obj as OAuthLink;

            return other != null &&
                   Id == other.Id &&
                   User == other.User &&
                   Provider == other.Provider &&
                   ProviderUserId == other.ProviderUserId &&
                   ProviderEmail == other.ProviderEmail &&
                   LinkedAt == other.LinkedAt;
        }
    }
}