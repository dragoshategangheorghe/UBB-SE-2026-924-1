using BankApp.Models.DTOs.Savings;
using BankApp.Models.Features.Savings;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Server.Controllers
{
    [ApiController]
    [Route("api/savings-presentation")]
    public class SavingsPresentationController : ControllerBase
    {
        private const int SingularAccountCount = 1;
        private const decimal DefaultBestApy = 0m;
        private const decimal PercentageScale = 100m;

        [HttpPost("total-saved")]
        public ActionResult<string> GetTotalSavedAmount([FromBody] IEnumerable<SavingsAccountSnapshotDto> accounts)
        {
            var result = $"${accounts.Sum(account => account.Balance):F2}";
            return Ok(result);
        }

        [HttpGet("accounts-text/{accountCount}")]
        public ActionResult<string> GetNumberOfAccountsText([FromRoute] int accountCount)
        {
            var result = $"across {accountCount} account{(accountCount == SingularAccountCount ? string.Empty : "s")}";
            return Ok(result);
        }

        [HttpPost("best-interest-rate")]
        public ActionResult<string> GetBestInterestRate([FromBody] IEnumerable<SavingsAccountSnapshotDto> accounts)
        {
            var bestApy = accounts.Any() ? accounts.Max(account => account.AnnualPercentageYield) : DefaultBestApy;
            var result = $"{bestApy * PercentageScale:F2}%";
            return Ok(result);
        }

        [HttpPost("close-penalty-risk")]
        public ActionResult<bool> CheckClosePenaltyRisk([FromBody] SavingsAccountSnapshotDto selectedAccount)
        {
            var hasRisk = selectedAccount?.SavingsType == "FixedDeposit" &&
                          selectedAccount.MaturityDate.HasValue &&
                          selectedAccount.MaturityDate.Value > DateTime.UtcNow;
            return Ok(hasRisk);
        }
    }
}
