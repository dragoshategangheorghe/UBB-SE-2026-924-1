using Microsoft.AspNetCore.Mvc;

//[Authorize]
namespace BankApp.Web.Controllers
{
    public class StatisticsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
