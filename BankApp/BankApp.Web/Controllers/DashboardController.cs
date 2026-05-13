using Microsoft.AspNetCore.Mvc;

namespace BankApp.Web.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
