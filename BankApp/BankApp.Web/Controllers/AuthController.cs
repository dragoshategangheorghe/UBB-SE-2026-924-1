using BankApp.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Web.Controllers;

[AllowAnonymousSession]
[Route("Auth")]
public class AuthController : Controller
{
    [HttpGet]
    public IActionResult Index(string? returnUrl = null)
    {
        //ViewData["ReturnUrl"] = returnUrl;
        return View();

    }
}
