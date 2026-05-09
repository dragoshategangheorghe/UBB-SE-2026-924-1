using BankApp.Client.Services.Interfaces;
using BankApp.Client.Utilities;
using BankApp.Models.Features.Investments;
using System.Threading.Tasks;

namespace BankApp.Client.Services.Implementations
{
    public class InvestmentsService : IInvestmentsService
    {
        private readonly ApiService _apiService;

        public InvestmentsService(ApiService apiService)
        {
            _apiService = apiService;
        }

        public Task<Portfolio?> GetPortfolioAsync(int userId)
        {
            return _apiService.GetAsync<Portfolio>($"/api/investments/portfolio/{userId}");
        }
    }
}

