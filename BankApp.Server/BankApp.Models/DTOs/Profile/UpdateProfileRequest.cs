namespace BankApp.Models.DTOs.Profile
{
    public class UpdateProfileRequest
    {
        public int? UserId { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }

        public UpdateProfileRequest()
        {
        }

        public UpdateProfileRequest(int? userId, string? phoneNumber, string? address)
        {
            UserId = userId;
            PhoneNumber = phoneNumber;
            Address = address;
        }
    }
}
