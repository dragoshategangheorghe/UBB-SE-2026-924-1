using System.Threading.Tasks;
using BankApp.Models.Features.Investments;

namespace BankApp.Client.RepoProxies.Interfaces
{
    public interface IInvestmentsApiService
    {
        Task<Portfolio?> GetPortfolioAsync(int userId);

        Task<Portfolio?> GetPortfolioForCurrentUserAsync();
    }
}
