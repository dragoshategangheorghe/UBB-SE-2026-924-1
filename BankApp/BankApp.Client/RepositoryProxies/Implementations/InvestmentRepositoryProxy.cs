using BankApp.Client.RepositoryProxies.Interfaces;
using BankApp.Client.Utilities;
using BankApp.Models.DTOs.Profile;
using BankApp.Models.Features.Investments;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BankApp.Client.RepositoryProxies.Implementations
{
    public class InvestmentRepositoryProxy : IInvestmentRepositoryProxy
    {
        private readonly ApiService apiService;

        public InvestmentRepositoryProxy(ApiService apiService)
        {
            this.apiService = apiService;
        }

        public Task<Portfolio?> GetPortfolioAsync(int userId)
        {
            return this.apiService.GetAsync<Portfolio>($"/api/investments/portfolio/{userId}");
        }

        public Task<IActionResult> TradeAsync(dynamic tradeData)
        {
            // Note: I don't know what request I'm supposed to use here
            //  I'm just going to use the variable like this, it's fishy anyway
            //  I also don't know about the response...
            return this.apiService.PostAsync<dynamic, IActionResult>($"/api/investments/trade", tradeData);
        }
    }
}
