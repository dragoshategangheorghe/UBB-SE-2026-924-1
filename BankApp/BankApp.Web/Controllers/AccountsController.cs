using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BankApp.Client.Services.Interfaces;

namespace BankApp.Web.Controllers
{
    [Authorize]
    public class AccountsController : Controller
    {
        private readonly IAccountService _accountService;

        public AccountsController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var accounts = await _accountService.GetAccountsAsync();
                return View(accounts);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Index", "Auth");
            }
        }
    }
}
