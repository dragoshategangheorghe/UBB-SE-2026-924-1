using BankApp.Models.DTOs.Savings;
using BankApp.Models.Features.Savings;
using BankApp.Server.Services.Implementations;
using BankApp.Server.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Server.Controllers
{
    [ApiController]
    [Route("api/savings-presentation")]
    public class SavingsPresentationController : ControllerBase
    {
        private readonly SavingsPresentationService _savingsService = new ();

        [HttpPost("total-saved")]
        public ActionResult<string> GetTotalSavedAmount([FromBody] IEnumerable<SavingsAccountSummaryDto> accounts)
        {
            var result = _savingsService.BuildTotalSavedAmount(accounts);
            return Ok(result);
        }

        [HttpGet("accounts-text/{accountCount}")]
        public ActionResult<string> GetNumberOfAccountsText([FromRoute] int accountCount)
        {
            var result = _savingsService.BuildNumberOfAccountsText(accountCount);
            return Ok(result);
        }

        [HttpPost("best-interest-rate")]
        public ActionResult<string> GetBestInterestRate([FromBody] IEnumerable<SavingsAccountSummaryDto> accounts)
        {
            var result = _savingsService.BuildBestInterestRate(accounts);
            return Ok(result);
        }

        [HttpPost("close-penalty-risk")]
        public ActionResult<bool> CheckClosePenaltyRisk([FromBody] SavingsAccountSummaryDto selectedAccount)
        {
            var hasRisk = _savingsService.HasClosePenaltyRisk(selectedAccount);
            return Ok(new { HasClosePenaltyRisk = hasRisk });
        }
    }
}
