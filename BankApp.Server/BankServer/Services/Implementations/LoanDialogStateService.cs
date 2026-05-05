namespace BankApp.Server.Services.Implementations
{
    public class LoanDialogStateService
    {
        private const int PositiveThreshold = 0;

        public bool ShouldComputeEstimate(double desiredAmount, int preferredTermMonths, string purpose)
        {
            return desiredAmount > PositiveThreshold &&
                   preferredTermMonths > PositiveThreshold &&
                   !string.IsNullOrWhiteSpace(purpose);
        }
    }
}