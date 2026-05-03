using BankApp.Models.DTOs.Dashboard;
namespace BankApp.Server.Services.Interfaces
{
    public interface IDashboardService
    {
        DashboardResponse GetDashboardData(int userId);
    }
}