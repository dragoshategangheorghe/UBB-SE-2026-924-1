using BankApp.Server.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticsService statisticsService;

        public StatisticsController(IStatisticsService statisticsService)
        {
            this.statisticsService = statisticsService;
        }

        private int GetAuthenticatedUserId() => (int)HttpContext.Items["UserId"] !;

        [HttpGet("spending-by-category")]
        public IActionResult GetSpendingByCategory()
        {
            return Ok(statisticsService.GetSpendingByCategory(GetAuthenticatedUserId()));
        }

        [HttpGet("income-vs-expenses")]
        public IActionResult GetIncomeVsExpenses()
        {
            return Ok(statisticsService.GetIncomeVsExpenses(GetAuthenticatedUserId()));
        }

        [HttpGet("balance-trends")]
        public IActionResult GetBalanceTrends()
        {
            return Ok(statisticsService.GetBalanceTrends(GetAuthenticatedUserId()));
        }

        [HttpGet("top-recipients")]
        public IActionResult GetTopRecipients()
        {
            return Ok(statisticsService.GetTopRecipients(GetAuthenticatedUserId()));
        }
    }
}
