using Microsoft.AspNetCore.Mvc;

namespace BankApp.Web.Controllers
{
    public class AccountsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
