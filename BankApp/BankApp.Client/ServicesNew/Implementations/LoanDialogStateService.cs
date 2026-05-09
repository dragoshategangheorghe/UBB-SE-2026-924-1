namespace BankApp.Server.Services.Implementations
{
    public class LoanDialogStateService
    {
        public bool ShouldComputeEstimate(double desiredAmount, int preferredTermMonths, string purpose)
        {
            return proxyLoanDialogStateRepository.GetShouldComputeEstimate(desiredAmount, preferredTermMonths, purpose);
        }
    }
}