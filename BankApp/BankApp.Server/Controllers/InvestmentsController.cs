namespace BankApp.Server.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using BankApp.Server.Repositories.Interfaces;
    using BankApp.Models.Entities;
    using System.Threading.Tasks;

    [ApiController]
    [Route("api/[controller]")]
    public class InvestmentsController : ControllerBase
    {
        private readonly IInvestmentRepository _investmentRepository;

        public InvestmentsController(IInvestmentRepository investmentRepository)
        {
            _investmentRepository = investmentRepository;
        }

        /// <summary>
        /// Retrieves the portfolio for a specific user.
        /// </summary>
        [HttpGet("portfolio/{userId}")]
        public IActionResult GetPortfolio(int userId)
        {
            var portfolio = _investmentRepository.GetPortfolio(userId);
            return Ok(portfolio);
        }

        /// <summary>
        /// Placeholder for executing a trade (POST request).
        /// </summary>
        [HttpPost("trade")]
        public async Task<IActionResult> Trade([FromBody] dynamic tradeData)
        {
            // Note: Your teammates can expand the DTO/Logic here for Assignment 4
            return await Task.FromResult(Ok(new { message = "Trade received" }));
        }
    }
}