using Microsoft.AspNetCore.Mvc;

namespace BankApp.Web.Controllers
{
    public class CryptoController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
