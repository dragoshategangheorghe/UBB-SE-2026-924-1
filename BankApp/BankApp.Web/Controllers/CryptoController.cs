using Microsoft.AspNetCore.Mvc;

namespace BankApp.Web.Controllers
{
    //[Authorize]
    public class CryptoController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
