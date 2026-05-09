namespace BankApp.Models.DTOs.Savings
{
    public class ValidationResponse
    {
        public bool IsValid { get; set; }

        public string ErrorMessage { get; set; } = string.Empty;
    }
}
