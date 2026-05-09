using BankApp.Server.Repositories.Interfaces;

namespace BankApp.Server.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using BankApp.Server.Services.Interfaces;
    using BankApp.Models.Entities;
    using System.Threading.Tasks;

    [ApiController]
    [Route("api/[controller]")]
    public class InvestmentsController : ControllerBase
    {
        private readonly IInvestmentRepository investmentRepository;

        public InvestmentsController(IInvestmentRepository investmentRepository)
        {
            this.investmentRepository = investmentRepository;
        }

        // TODO THERE IS NO INVESTMENT SERVICE AND THE OTHER REPO METHODS ARE NOT IMPLEMENTED

        /// <summary>
        /// Retrieves the portfolio for a specific user.
        /// </summary>
        [HttpGet("portfolio/{userId}")]
        public IActionResult GetPortfolio(int userId)
        {
            var portfolio = investmentRepository.GetPortfolio(userId);
            return Ok(portfolio);
        }

        /// <summary>
        /// Placeholder for executing a trade (POST request).
        /// </summary>
        [HttpPost("trade")]
        public async Task<IActionResult> Trade([FromBody] dynamic tradeData)
        {
            // Note: Your teammates can expand the DTO/Logic here for Assignment 4
            // Note from Alex: Wasn't this mandatory for A3 ? Send help, I'm too busy rewriting all controllers :sob:
            return await Task.FromResult(Ok(new { message = "Trade received" }));
        }
    }
}