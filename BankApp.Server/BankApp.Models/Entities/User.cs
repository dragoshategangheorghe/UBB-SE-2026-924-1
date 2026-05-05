namespace BankApp.Models.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? Nationality { get; set; }
        public string PreferredLanguage { get; set; } = "en";
        public bool Is2FAEnabled { get; set; }
        public string? Preferred2FAMethod { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public int FailedLoginAttempts { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            User other = (User)obj;

            return Id == other.Id &&
                   Email == other.Email &&
                   PasswordHash == other.PasswordHash &&
                   FullName == other.FullName &&
                   PhoneNumber == other.PhoneNumber &&
                   DateOfBirth == other.DateOfBirth &&
                   Address == other.Address &&
                   Nationality == other.Nationality &&
                   PreferredLanguage == other.PreferredLanguage &&
                   Is2FAEnabled == other.Is2FAEnabled &&
                   Preferred2FAMethod == other.Preferred2FAMethod &&
                   IsLocked == other.IsLocked &&
                   LockoutEnd == other.LockoutEnd &&
                   FailedLoginAttempts == other.FailedLoginAttempts &&
                   CreatedAt == other.CreatedAt &&
                   UpdatedAt == other.UpdatedAt;
        }
    }
}