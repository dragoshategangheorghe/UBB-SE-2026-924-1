using Microsoft.AspNetCore.Mvc;
using BankApp.Client.Services.Interfaces;
using BankApp.Web.ViewModels.Statistics;

namespace BankApp.Web.Controllers
{
    public class StatisticsController : Controller
    {
        private readonly IStatisticsService _statisticsService;

        public StatisticsController(IStatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var spendingByCategory = await _statisticsService.GetSpendingByCategoryAsync();
                var incomeVsExpenses = await _statisticsService.GetIncomeVsExpensesAsync();
                var balanceTrends = await _statisticsService.GetBalanceTrendsAsync();
                var topRecipients = await _statisticsService.GetTopRecipientsAsync();

                var viewModel = new StatisticsViewModel
                {
                    SpendingByCategory = spendingByCategory,
                    IncomeVsExpenses = incomeVsExpenses,
                    BalanceTrends = balanceTrends,
                    TopRecipients = topRecipients
                };

                return View(viewModel);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Index", "Auth");
            }
        }
    }
}
