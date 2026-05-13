using Microsoft.AspNetCore.Mvc;

namespace BankApp.Web.Controllers
{
    public class SavingsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
