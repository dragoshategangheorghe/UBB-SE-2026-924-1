using BankApp.Client.Services.Interfaces;
using BankApp.Web.Infrastructure;
using BankApp.Web.Models.Auth;
using BankApp.Web.Models.Savings;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Web.Controllers;

[AllowAnonymousSession]
public class AuthController(IAuthService authService) : Controller
{
    private readonly IAuthService authService = authService;

    public IActionResult Index(string? returnUrl = null)
    {
        //ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login([Bind(Prefix = "Login")] LoginFormModel login)
    {

        return Redirect("/Dashboard");
    }
}
