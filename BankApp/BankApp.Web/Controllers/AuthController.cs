using BankApp.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Web.Controllers;

[AllowAnonymousSession]
public class AuthController : Controller
{
    public IActionResult Index(string? returnUrl = null)
    {
        //ViewData["ReturnUrl"] = returnUrl;
        return View();

    }
}
