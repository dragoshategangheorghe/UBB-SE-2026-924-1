using System.Threading.Tasks;
using BankApp.Client.RepoProxies;
using BankApp.Client.RepoProxies.Interfaces;
using BankApp.Models.DTOs.Statistics;

namespace BankApp.Client.RepoProxies.Implementations
{
    public class StatisticsRepoProxy : IStatisticsRepoProxy
    {
        private readonly ApiService _apiService;

        public StatisticsRepoProxy(ApiService apiService)
        {
            _apiService = apiService;
        }

        public Task<SpendingByCategoryResponse?> GetSpendingByCategoryAsync()
        {
            return _apiService.GetAsync<SpendingByCategoryResponse>("/api/statistics/spending-by-category");
        }

        public Task<IncomeVsExpensesResponse?> GetIncomeVsExpensesAsync()
        {
            return _apiService.GetAsync<IncomeVsExpensesResponse>("/api/statistics/income-vs-expenses");
        }

        public Task<BalanceTrendsResponse?> GetBalanceTrendsAsync()
        {
            return _apiService.GetAsync<BalanceTrendsResponse>("/api/statistics/balance-trends");
        }

        public Task<TopRecipientsResponse?> GetTopRecipientsAsync()
        {
            return _apiService.GetAsync<TopRecipientsResponse>("/api/statistics/top-recipients");
        }
    }
}
