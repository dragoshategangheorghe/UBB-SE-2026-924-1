using System.Threading.Tasks;
using BankApp.Client.RepoProxies;
using BankApp.Client.RepoProxies.Interfaces;
using BankApp.Models.Features.Investments;

namespace BankApp.Client.RepoProxies.Implementations
{
    public class InvestmentsRepoProxy : IInvestmentsRepoProxy
    {
        private readonly ApiService _apiService;

        public InvestmentsRepoProxy(ApiService apiService)
        {
            _apiService = apiService;
        }

        public Task<Portfolio?> GetPortfolioAsync(int userId)
        {
            return _apiService.GetAsync<Portfolio>($"/api/investments/portfolio/{userId}");
        }

        public Task<Portfolio?> GetPortfolioForCurrentUserAsync()
        {
            int? userId = _apiService.GetCurrentUserId();
            return userId == null ? Task.FromResult<Portfolio?>(null) : GetPortfolioAsync(userId.Value);
        }
    }
}
