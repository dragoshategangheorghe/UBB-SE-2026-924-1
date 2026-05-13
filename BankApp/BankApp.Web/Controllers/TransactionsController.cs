using Microsoft.AspNetCore.Mvc;

namespace BankApp.Web.Controllers
{
    public class TransactionsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
