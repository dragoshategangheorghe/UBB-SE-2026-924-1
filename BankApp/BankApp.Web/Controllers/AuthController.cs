using Microsoft.AspNetCore.Mvc;

namespace BankApp.Web.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
