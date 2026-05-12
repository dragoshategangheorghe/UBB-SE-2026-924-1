using BankApp.Models.Entities;
using BankApp.Server.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly IDashboardRepository dashboardRepository;

        public AccountsController(IDashboardRepository dashboardRepository)
        {
            this.dashboardRepository = dashboardRepository;
        }

        private int GetAuthenticatedUserId() => (int)this.HttpContext.Items["UserId"] !;

        /// <summary>
        /// Returns bank accounts for the authenticated user. The route userId must match the JWT user.
        /// </summary>
        [HttpGet("user/{userId:int}")]
        public ActionResult<List<Account>> GetAccountsForUser(int userId)
        {
            int authenticatedUserId = this.GetAuthenticatedUserId();
            if (userId != authenticatedUserId)
            {
                return this.Forbid();
            }

            List<Account> accounts = this.dashboardRepository.GetAccountsByUser(userId);
            return this.Ok(accounts);
        }
    }
}
