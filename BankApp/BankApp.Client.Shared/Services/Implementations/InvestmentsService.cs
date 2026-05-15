namespace BankApp.Client.Services.Implementations
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using BankApp.Client.RepoProxies.Interfaces;
    using BankApp.Client.Services.Interfaces;
    using BankApp.Models.Entities;

    public class InvestmentsService : IInvestmentsService
    {
        private readonly IInvestmentsRepoProxy _investmentsRepo;
        private readonly IAuthService _authService;

        public InvestmentsService(IInvestmentsRepoProxy investmentsRepo, IAuthService authService)
        {
            this._investmentsRepo = investmentsRepo;
            this._authService = authService;
        }

        public async Task<Portfolio?> GetPortfolioAsync(int userId)
        {
            return await this._investmentsRepo.GetAsync<Portfolio>($"/api/investments/portfolio/{userId}");
        }

        public async Task<Portfolio?> GetPortfolioForCurrentUserAsync()
        {
            // Dynamically fetch the ID from the Auth Session
            int? userId = this._authService.GetCurrentUserId();

            if (!userId.HasValue)
            {
                Debug.WriteLine("InvestmentsService: Attempted to fetch portfolio, but no user is logged in.");
                return null;
            }

            return await this.GetPortfolioAsync(userId.Value);
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