namespace BankApp.Client.Services.Implementations
{
    using System.Threading.Tasks;
    using BankApp.Client.RepoProxies.Interfaces;
    using BankApp.Client.Services.Interfaces;
    using BankApp.Models.Entities;

    public class InvestmentsService : IInvestmentsService
    {
        private readonly IInvestmentsRepoProxy investmentsRepo;

        public InvestmentsService(IInvestmentsRepoProxy investmentsRepo)
        {
            this.investmentsRepo = investmentsRepo;
        }

        public Task<Portfolio?> GetPortfolioAsync(int userId)
        {
            return this.investmentsRepo.GetPortfolioAsync(userId);
        }

        public Task<Portfolio?> GetPortfolioForCurrentUserAsync()
        {
            return this.investmentsRepo.GetPortfolioForCurrentUserAsync();
        }
    }
}