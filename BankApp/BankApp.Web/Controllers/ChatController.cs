using Microsoft.AspNetCore.Mvc;

namespace BankApp.Web.Controllers
{
    public class ChatController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
