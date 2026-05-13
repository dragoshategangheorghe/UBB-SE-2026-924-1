using Microsoft.AspNetCore.Mvc;

namespace BankApp.Web.Controllers
{
    public class LoansController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
