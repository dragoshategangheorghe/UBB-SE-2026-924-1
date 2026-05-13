using Microsoft.AspNetCore.Mvc;

namespace BankApp.Web.Controllers
{
    public class CardsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
