using BankApp.Models.DTOs.Dashboard;
using Microsoft.AspNetCore.Mvc;
using BankApp.Server.Repositories.Interfaces;

namespace BankApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardRepository dashboardRepository;
        private readonly IUserRepository userRepository;

        public DashboardController(IDashboardRepository dashboardRepository, IUserRepository userRepository)
        {
            this.dashboardRepository = dashboardRepository;
            this.userRepository = userRepository;
        }

        [HttpGet]
        public IActionResult GetDashboard()
        {
            try
            {
                int userId = (int)HttpContext.Items["UserId"] !;

                var user = userRepository.FindById(userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                DashboardResponse dashboardData = new DashboardResponse
                {
                    CurrentUser = user,
                    Cards = dashboardRepository.GetCardsByUser(userId),
                    RecentTransactions = dashboardRepository.GetRecentTransactions(userId, 10),
                    UnreadNotificationCount = dashboardRepository.GetUnreadNotificationCount(userId)
                };

                return Ok(dashboardData);
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    new { error = "An error occured while fetching the dashboard data." });
            }
        }
    }
}