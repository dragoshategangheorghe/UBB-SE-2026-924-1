namespace BankApp.Client.RepoProxies.Implementations
{
    using System.Threading.Tasks;
    using BankApp.Client.RepoProxies.Interfaces;
    using BankApp.Client.Utilities;
    using BankApp.Models.Entities;

    public class InvestmentsRepoProxy : IInvestmentsRepoProxy
    {
        private readonly ApiService apiService;

        public InvestmentsRepoProxy(ApiService apiService) => this.apiService = apiService;

        public async Task<Portfolio?> GetPortfolioAsync(int userId)
        {
            return await this.apiService.GetAsync<Portfolio>($"/api/investments/portfolio/{userId}");
        }

        public async Task<Portfolio?> GetPortfolioForCurrentUserAsync()
        {
            int? userId = this.apiService.GetCurrentUserId();
            return userId == null ? null : await this.GetPortfolioAsync(userId.Value);
        }
    }
}