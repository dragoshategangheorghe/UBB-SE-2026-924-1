using Microsoft.AspNetCore.Mvc;

namespace BankApp.Web.Controllers
{
    //[Authorize]
    public class InvestmentsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
