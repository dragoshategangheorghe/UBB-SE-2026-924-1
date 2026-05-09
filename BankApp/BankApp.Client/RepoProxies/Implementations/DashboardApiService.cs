using System.Threading.Tasks;
using BankApp.Client.RepoProxies;
using BankApp.Client.RepoProxies.Interfaces;
using BankApp.Models.DTOs.Dashboard;

namespace BankApp.Client.RepoProxies.Implementations
{
    public class DashboardApiService : IDashboardApiService
    {
        private readonly ApiService _apiService;

        public DashboardApiService(ApiService apiService)
        {
            _apiService = apiService;
        }

        public Task<DashboardResponse?> GetDashboardAsync()
        {
            return _apiService.GetAsync<DashboardResponse>("/api/dashboard");
        }
    }
}
