using Microsoft.AspNetCore.Mvc;

//[Authorize]
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
