using BankApp.Models.DTOs.Dashboard;
using BankApp.Server.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using BankApp.Server.Services.Interfaces;

namespace BankApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardRepository _dashboardRepository;
        public DashboardController(IDashboardRepository dashboardRepository)
        {
            this._dashboardRepository = dashboardRepository;
        }

        private int GetAuthenticatedUserId() => (int)HttpContext.Items["UserId"]!;

        [HttpGet("cards")]
        public IActionResult GetCardsByUser()
        {
            return Ok(_dashboardRepository.GetCardsByUser(GetAuthenticatedUserId()));
        }

        [HttpGet("recentTransactions")]
        public IActionResult GetRecentTransactions()
        {
            return Ok(_dashboardRepository.GetRecentTransactions(GetAuthenticatedUserId()));

            // This is acting like an User can only have One Account, idk if it's ok
        }

        [HttpGet("unreadNotificationCount")]
        public IActionResult GetUnreadNotificationCount()
        {
            return Ok(_dashboardRepository.GetUnreadNotificationCount(GetAuthenticatedUserId()));
        }

        [HttpGet("accounts")]
        public IActionResult GetAccounts()
        {
            return Ok(_dashboardRepository.GetAccountsByUser(GetAuthenticatedUserId()));
        }
    }
}