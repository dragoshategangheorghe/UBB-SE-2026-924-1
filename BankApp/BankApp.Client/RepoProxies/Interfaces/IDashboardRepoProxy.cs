using System.Threading.Tasks;
using BankApp.Models.DTOs.Dashboard;

namespace BankApp.Client.RepoProxies.Interfaces
{
    public interface IDashboardRepoProxy
    {
        Task<DashboardResponse?> GetDashboardAsync();
    }
}
