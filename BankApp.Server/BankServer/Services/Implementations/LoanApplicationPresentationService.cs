namespace BankApp.Server.Services.Implementations
{
    public class LoanApplicationPresentationService
    {
        public (bool Approved, string Message) BuildApplicationOutcome(string? rejectionReason)
        {
            return rejectionReason == null
                ? (true, "Your loan application has been approved!")
                : (false, $"Application rejected: {rejectionReason}");
        }
    }
}
