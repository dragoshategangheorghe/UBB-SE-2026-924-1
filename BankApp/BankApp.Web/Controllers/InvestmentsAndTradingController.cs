using System;
using System.Threading.Tasks;
using BankApp.Client.Services.Interfaces;
using BankApp.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Web.Controllers
{
    // The global RequireSessionLoginFilter in Program.cs covers this, 
    // but inheriting or leaving it plain aligns with your team's architecture.
    public class InvestmentsAndTradingController : Controller
    {
        private readonly IInvestmentsService _investmentsService;
        private readonly IWebSessionContext _sessionContext;

        public InvestmentsAndTradingController(IInvestmentsService investmentsService, IWebSessionContext sessionContext)
        {
            this._investmentsService = investmentsService;
            this._sessionContext = sessionContext;
        }

        // GET: /InvestmentsAndTrading
        public async Task<IActionResult> Index()
        {
            // Pull the dynamic logged-in user ID via your team's WebSessionContext
            var portfolio = await this._investmentsService.GetPortfolioForCurrentUserAsync();

            if (portfolio == null)
            {
                // Fallback safe object instantiation to prevent NullReferenceExceptions in the View
                portfolio = new BankApp.Models.Entities.Portfolio();
            }

            return View(portfolio);
        }

        // GET: /InvestmentsAndTrading/Trade
        public async Task<IActionResult> Trade()
        {
            var portfolio = await this._investmentsService.GetPortfolioForCurrentUserAsync();

            // Send the available portfolio cash/value balance over to the view data container
            ViewBag.WalletBalance = portfolio?.TotalValue ?? 0m;

            return View();
        }

        // POST: /InvestmentsAndTrading/ExecuteTrade
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExecuteTrade(string ticker, string actionType, decimal quantity)
        {
            int? userId = this._sessionContext.CurrentUserId;
            if (!userId.HasValue)
            {
                return RedirectToAction("Index", "Auth");
            }

            // Standard target asset prices matching your system specifications
            decimal executionPrice = ticker switch
            {
                "BTC" => 65000.00m,
                "ETH" => 2550.00m,
                "SOL" => 145.00m,
                _ => 0m
            };

            try
            {
                bool success = await this._investmentsService.ExecuteTradeAsync(
                    userId.Value,
                    ticker,
                    actionType,
                    quantity,
                    executionPrice);

                if (success)
                {
                    return RedirectToAction(nameof(Index));
                }

                TempData["ErrorMessage"] = "Transaction was rejected by transaction validation rule parameters.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Execution failure: {ex.Message}";
            }

            return RedirectToAction(nameof(Trade));
        }
    }
}