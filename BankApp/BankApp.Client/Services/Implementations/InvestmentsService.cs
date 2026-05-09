using System.Threading.Tasks;
using BankApp.Client.RepoProxies.Interfaces;
using BankApp.Client.Services.Interfaces;
using BankApp.Models.Features.Investments;

namespace BankApp.Client.Services.Implementations
{
    public class InvestmentsService : IInvestmentsService
    {
        private readonly IInvestmentsApiService _investmentsRepo;

        public InvestmentsService(IInvestmentsApiService investmentsRepo)
        {
            _investmentsRepo = investmentsRepo;
        }

        public Task<Portfolio?> GetPortfolioAsync(int userId)
        {
            return _investmentsRepo.GetPortfolioAsync(userId);
        }

        public Task<Portfolio?> GetPortfolioForCurrentUserAsync()
        {
            return _investmentsRepo.GetPortfolioForCurrentUserAsync();
        }
    }
}
