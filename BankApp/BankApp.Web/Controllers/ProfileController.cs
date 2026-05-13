using Microsoft.AspNetCore.Mvc;

namespace BankApp.Web.Controllers
{
    public class ProfileController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
