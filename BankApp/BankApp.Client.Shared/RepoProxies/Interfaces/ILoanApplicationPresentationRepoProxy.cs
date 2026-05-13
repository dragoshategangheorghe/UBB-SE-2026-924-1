using System.Threading.Tasks;
using BankApp.Models.DTOs.Loans;

namespace BankApp.Client.RepoProxies.Interfaces
{
    public interface ILoanApplicationPresentationRepoProxy
    {
        Task<BuildApplicationOutcomeResponse?> GetBuildApplicationOutcome(string? rejectionReason);
    }
}
