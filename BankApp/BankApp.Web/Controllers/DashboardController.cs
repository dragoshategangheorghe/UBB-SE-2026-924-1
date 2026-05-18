using BankApp.Client.Services.Interfaces;
using BankApp.Models.DTOs.Dashboard;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace BankApp.Web.Controllers
{
   // [Authorize]
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