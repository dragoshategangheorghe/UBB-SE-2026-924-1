namespace BankApp.Models.DTOs.Profile
{
    public class ChangePasswordRequest
    {
        public int UserId { get; set; }
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;

        public ChangePasswordRequest()
        {
        }
        public ChangePasswordRequest(int userId, string currentPassword, string newPassword)
        {
            UserId = userId;
            CurrentPassword = currentPassword;
            NewPassword = newPassword;
        }

        public override bool Equals(object? obj)
        {
            ChangePasswordRequest? other = obj as ChangePasswordRequest;

            return other != null &&
                   UserId == other.UserId &&
                   CurrentPassword == other.CurrentPassword &&
                   NewPassword == other.NewPassword;
        }
    }
}
