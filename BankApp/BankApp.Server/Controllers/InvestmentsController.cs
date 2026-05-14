namespace BankApp.Server.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;
    using BankApp.Models.Entities;
    using BankApp.Server.Repositories.Interfaces;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/[controller]")]
    public class InvestmentsController : ControllerBase
    {
        private readonly IInvestmentRepository _repo;

        public InvestmentsController(IInvestmentRepository repo)
        {
            _repo = repo;
        }

        // --- THE MISSING METHOD ---
        [HttpGet("portfolio/{userId}")]
        public async Task<IActionResult> GetPortfolio(int userId)
        {
            // This calls the repository method we just verified
            var portfolio = _repo.GetPortfolio(userId);

            if (portfolio == null)
            {
                return NotFound($"Portfolio for user {userId} not found.");
            }

            return Ok(portfolio);
        }

        [HttpPost("trade")]
        public async Task<IActionResult> ExecuteTrade([FromBody] TradeRequest request)
        {
            // 1. Calculate the math (Fees, etc.)
            decimal feeRate = 0.015m;
            decimal tradeValue = request.quantity * request.price;
            decimal fees = Math.Round(tradeValue * feeRate, 2);

            // 2. Fetch current portfolio to get totals
            var portfolio = _repo.GetPortfolio(request.userId);
            var holding = portfolio.Holdings.FirstOrDefault(h => h.Ticker == request.ticker);

            decimal currentQty = holding?.Quantity ?? 0;
            decimal currentAvgPrice = holding?.AvgPurchasePrice ?? 0;

            // 3. Calculate new totals
            decimal finalQty = request.action == "BUY" ? currentQty + request.quantity : currentQty - request.quantity;

            // Prevent division by zero if selling everything
            decimal finalAvgPrice = (request.action == "BUY" && finalQty > 0)
                ? ((currentQty * currentAvgPrice) + (request.quantity * request.price)) / finalQty
                : currentAvgPrice;

            // 4. Save to DB
            await _repo.RecordCryptoTradeAsync(
                portfolio.Id,
                request.ticker,
                request.action,
                request.quantity,
                request.price,
                fees,
                finalQty,
                finalAvgPrice);

            return Ok(true);
        }
    }

    public record TradeRequest(int userId, string ticker, string action, decimal quantity, decimal price);
}