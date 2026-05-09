using BankApp.Models.DTOs.Dashboard;
using System.Threading.Tasks;

namespace BankApp.Client.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardResponse?> GetDashboardAsync();
    }
}

