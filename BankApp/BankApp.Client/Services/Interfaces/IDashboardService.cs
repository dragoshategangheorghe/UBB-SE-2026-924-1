using System.Threading.Tasks;
using BankApp.Models.DTOs.Dashboard;

namespace BankApp.Client.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardResponse?> GetDashboardAsync();
    }
}

