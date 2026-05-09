namespace BankApp.Models.DTOs.Loans
{
    public class BuildApplicationOutcomeResponse
    {
        public bool IsApproved { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}
