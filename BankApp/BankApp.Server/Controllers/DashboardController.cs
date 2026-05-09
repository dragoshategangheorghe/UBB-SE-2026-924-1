using BankApp.Models.DTOs.Dashboard;
using BankApp.Server.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashService;
        public DashboardController(IDashboardService dashService)
        {
            this._dashService = dashService;
        }

        [HttpGet]
        public IActionResult GetDashboard()
        {
            try
            {
                int userId = (int)HttpContext.Items["UserId"]!;

                DashboardResponse dashboardData = _dashService.GetDashboardData(userId);
                if (dashboardData == null)
                {
                    return NotFound(new { message = "Dashboard data not found." });
                }

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