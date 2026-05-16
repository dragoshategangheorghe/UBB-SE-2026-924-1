using BankApp.Client.Services.Interfaces;
using BankApp.Models.DTOs.Auth;
using BankApp.Models.Enums;
using BankApp.Web.Infrastructure;
using BankApp.Web.Models.Auth;
using BankApp.Web.Models.Savings;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Web.Controllers;

[AllowAnonymousSession]
public class AuthController(IAuthService authService, IWebSessionContext webSessionContext) : Controller
{
    private readonly IAuthService _authService = authService;
    private IWebSessionContext webSessionContext = webSessionContext;

    public IActionResult Index(string? returnUrl = null)
    {
        if (webSessionContext.IsAuthenticated)
            return Redirect("/Dashboard");
        //ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // NOTE: if the parameter name (pascal or camel case) is not the same as its name inside the page model.. then it will not bind
    public async Task<IActionResult?> Login(LoginFormModel loginForm)
    {
        /*
        if (!ModelState.IsValid)
        {
            loginForm.LoginState = "Email or password format is invalid";
            return View("Index", new LoginModel { Login = loginForm });
        }
        */
        try
        {
            BankApp.Models.DTOs.Auth.LoginRequest request = new BankApp.Models.DTOs.Auth.LoginRequest
            {
                Email = loginForm.Email,
                Password = loginForm.Password
            };

            LoginResponse? loginResponse = await _authService.LoginAsync(request);

            if (loginResponse == null)
            {
                loginForm.LoginState = "An Error has occured";
                return View("Index", new LoginModel {Login = loginForm});
            }

            if (!loginResponse.Success)
            {
                if (loginResponse.Error != null && loginResponse.Error.Contains("locked"))
                {
                    loginForm.LoginState = "Account is locked.";
                }
                else
                {
                    loginForm.LoginState = "Invalid credentials.";
                }
                return View("Index", new LoginModel { Login = loginForm });
            }

            if (loginResponse.Requires2FA)
            {
                loginForm.LoginState = "The login requires 2FA.";
                return View("Index", new LoginModel { Login = loginForm });
            }
            // success

            webSessionContext.Authenticate(loginResponse.Token, loginResponse.UserId ?? 0);

            return Redirect("/Dashboard");
        }
        catch (HttpRequestException)
        {
            loginForm.LoginState = "No clue";
            return View("Index", loginForm);
        }
    }

    public async Task<IActionResult?> LogOut()
    {
        webSessionContext.Clear();
        return View("Index", new LoginModel());
    }
}
