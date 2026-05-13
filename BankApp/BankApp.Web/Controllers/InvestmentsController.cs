using Microsoft.AspNetCore.Mvc;

namespace BankApp.Web.Controllers
{
    public class InvestmentsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
