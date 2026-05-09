namespace BankApp.Server.Services.Implementations
{
    public class LoanApplicationPresentationService
    {
        public (bool Approved, string Message) BuildApplicationOutcome(string? rejectionReason)
        {
            return proxyLoanApplicationPresentationRepository.GetBuildApplicationOutcome(rejectionReason);
        }
    }
}
