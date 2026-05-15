using Microsoft.AspNetCore.Mvc;

namespace BankApp.Web.Controllers
{
    //[Authorize]
    public class ProfileController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
