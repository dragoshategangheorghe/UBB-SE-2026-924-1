using BankApp.Server.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticsService _statisticsService;

        public StatisticsController(IStatisticsService statisticsService)
        {
            this._statisticsService = statisticsService;
        }

        private int GetAuthenticatedUserId() => (int)HttpContext.Items["UserId"]!;

        [HttpGet("spending-by-category")]
        public IActionResult GetSpendingByCategory()
        {
            return Ok(_statisticsService.GetSpendingByCategory(GetAuthenticatedUserId()));
        }

        [HttpGet("income-vs-expenses")]
        public IActionResult GetIncomeVsExpenses()
        {
            return Ok(_statisticsService.GetIncomeVsExpenses(GetAuthenticatedUserId()));
        }

        [HttpGet("balance-trends")]
        public IActionResult GetBalanceTrends()
        {
            return Ok(_statisticsService.GetBalanceTrends(GetAuthenticatedUserId()));
        }

        [HttpGet("top-recipients")]
        public IActionResult GetTopRecipients()
        {
            return Ok(_statisticsService.GetTopRecipients(GetAuthenticatedUserId()));
        }
    }
}
