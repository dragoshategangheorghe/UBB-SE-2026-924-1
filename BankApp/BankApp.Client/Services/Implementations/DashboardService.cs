using System.Threading.Tasks;
using BankApp.Client.RepoProxies.Interfaces;
using BankApp.Client.Services.Interfaces;
using BankApp.Models.DTOs.Dashboard;

namespace BankApp.Client.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardApiService _dashboardRepo;

        public DashboardService(IDashboardApiService dashboardRepo)
        {
            _dashboardRepo = dashboardRepo;
        }

        public Task<DashboardResponse?> GetDashboardAsync()
        {
            return _dashboardRepo.GetDashboardAsync();
        }
    }
}
