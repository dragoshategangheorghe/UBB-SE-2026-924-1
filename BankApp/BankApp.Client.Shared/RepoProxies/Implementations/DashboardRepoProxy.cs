using System.Threading.Tasks;
using BankApp.Client.RepoProxies;
using BankApp.Client.RepoProxies.Interfaces;
using BankApp.Models.DTOs.Dashboard;

namespace BankApp.Client.RepoProxies.Implementations
{
    public class DashboardRepoProxy : IDashboardRepoProxy
    {
        private readonly ApiService _apiService;

        public DashboardRepoProxy(ApiService apiService)
        {
            _apiService = apiService;
        }

        public Task<DashboardResponse?> GetDashboardAsync()
        {
            return _apiService.GetAsync<DashboardResponse>("/api/dashboard");
        }
    }
}
