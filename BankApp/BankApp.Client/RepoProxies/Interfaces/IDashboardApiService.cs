using System.Threading.Tasks;
using BankApp.Models.DTOs.Dashboard;

namespace BankApp.Client.RepoProxies.Interfaces
{
    public interface IDashboardApiService
    {
        Task<DashboardResponse?> GetDashboardAsync();
    }
}
