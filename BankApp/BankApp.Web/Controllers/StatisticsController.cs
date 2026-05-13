using Microsoft.AspNetCore.Mvc;

namespace BankApp.Web.Controllers
{
    public class StatisticsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
