using BankApp.Client.Services.Interfaces;
using BankApp.Models.DTOs.Dashboard;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Web.Controllers
{
    //[Authorize]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index()
        {
            DashboardResponse? response = await _dashboardService.GetDashboardAsync();

            if (response == null)
            {
                return NotFound();
            }
            return View(response);
        }
    }
}
