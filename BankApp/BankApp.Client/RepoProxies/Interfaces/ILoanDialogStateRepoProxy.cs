using System.Threading.Tasks;

namespace BankApp.Client.RepoProxies.Interfaces
{
    public interface ILoanDialogStateRepoProxy
    {
        Task<bool> GetShouldComputeEstimate(double desiredAmount, int preferredTermMonths, string purpose);
    }
}
