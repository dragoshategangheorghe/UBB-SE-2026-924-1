namespace BankApp.Client.Services.Implementations
{
    using System.Threading.Tasks;
    using BankApp.Client.RepoProxies.Interfaces;
    using BankApp.Client.Services.Interfaces;
    using BankApp.Models.Entities;

    public class InvestmentsService : IInvestmentsService
    {
        private readonly IInvestmentsRepoProxy _investmentsRepo;

        public InvestmentsService(IInvestmentsRepoProxy investmentsRepo)
        {
            this._investmentsRepo = investmentsRepo;
        }

        public async Task<Portfolio?> GetPortfolioAsync(int userId)
        {
            return await this._investmentsRepo.GetAsync<Portfolio>($"/api/investments/portfolio/{userId}");
        }

        public async Task<Portfolio?> GetPortfolioForCurrentUserAsync()
        {
            // Points to Vlad (ID 1) as requested for current setup
            return await this.GetPortfolioAsync(1);
        }

        public async Task<bool> ExecuteTradeAsync(int userId, string ticker, string action, decimal quantity, decimal price)
        {
            var request = new
            {
                UserId = userId,
                Ticker = ticker,
                Action = action,
                Quantity = quantity,
                Price = price
            };

            return await this._investmentsRepo.PostAsync<object, bool>("/api/investments/trade", request);
        }
    }
}