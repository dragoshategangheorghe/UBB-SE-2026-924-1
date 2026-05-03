using System.Threading.Tasks;
using BankApp.Client.Services.Interfaces;
using BankApp.Client.Utilities;
using BankApp.Models.DTOs.Statistics;

namespace BankApp.Client.Services.Implementations
{
    public class StatisticsApiService : IStatisticsApiService
    {
        private readonly ApiService _apiService;

        public StatisticsApiService(ApiService apiService)
        {
            _apiService = apiService;
        }

        public Task<SpendingByCategoryResponse?> GetSpendingByCategoryAsync()
        {
            return _apiService.GetAsync<SpendingByCategoryResponse>("api/statistics/spending-by-category");
        }

        public Task<IncomeVsExpensesResponse?> GetIncomeVsExpensesAsync()
        {
            return _apiService.GetAsync<IncomeVsExpensesResponse>("api/statistics/income-vs-expenses");
        }

        public Task<BalanceTrendsResponse?> GetBalanceTrendsAsync()
        {
            return _apiService.GetAsync<BalanceTrendsResponse>("api/statistics/balance-trends");
        }

        public Task<TopRecipientsResponse?> GetTopRecipientsAsync()
        {
            return _apiService.GetAsync<TopRecipientsResponse>("api/statistics/top-recipients");
        }
    }
}
