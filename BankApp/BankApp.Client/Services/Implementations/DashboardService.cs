using System.Threading.Tasks;
using BankApp.Client.RepoProxies.Interfaces;
using BankApp.Client.Services.Interfaces;
using BankApp.Models.DTOs.Dashboard;

namespace BankApp.Client.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepoProxy _dashboardRepo;

        public DashboardService(IDashboardRepoProxy dashboardRepo)
        {
            _dashboardRepo = dashboardRepo;
        }

        public Task<DashboardResponse?> GetDashboardAsync()
        {
            return _dashboardRepo.GetDashboardAsync();
        }
    }
}
