using Microsoft.AspNetCore.Mvc;

namespace BankApp.Web.Controllers
{
    //[Authorize]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
