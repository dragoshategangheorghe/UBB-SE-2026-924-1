using BankApp.Client.RepoProxies.Interfaces;
using BankApp.Client.Services.Interfaces;
using BankApp.Models.DTOs.Statistics;
using System.Threading.Tasks;

namespace BankApp.Client.Services.Implementations
{
    public class StatisticsService : IStatisticsService
    {
        private readonly IStatisticsRepoProxy _repoProxy;

        public StatisticsService(IStatisticsRepoProxy repoProxy)
        {
            _repoProxy = repoProxy;
        }

        public Task<SpendingByCategoryResponse?> GetSpendingByCategoryAsync() => _repoProxy.GetSpendingByCategoryAsync();
        public Task<IncomeVsExpensesResponse?> GetIncomeVsExpensesAsync() => _repoProxy.GetIncomeVsExpensesAsync();
        public Task<BalanceTrendsResponse?> GetBalanceTrendsAsync() => _repoProxy.GetBalanceTrendsAsync();
        public Task<TopRecipientsResponse?> GetTopRecipientsAsync() => _repoProxy.GetTopRecipientsAsync();
    }
}

