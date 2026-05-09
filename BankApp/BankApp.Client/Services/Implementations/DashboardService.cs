using BankApp.Client.Services.Interfaces;
using BankApp.Client.Utilities;
using BankApp.Models.DTOs.Dashboard;
using System.Threading.Tasks;

namespace BankApp.Client.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly ApiService _apiService;

        public DashboardService(ApiService apiService)
        {
            _apiService = apiService;
        }

        public Task<DashboardResponse?> GetDashboardAsync()
        {
            return _apiService.GetAsync<DashboardResponse>("/api/dashboard");
        }
    }
}

