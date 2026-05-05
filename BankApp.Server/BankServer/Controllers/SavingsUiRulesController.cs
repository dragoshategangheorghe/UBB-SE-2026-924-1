using BankApp.Models.DTOs.Savings;
using BankApp.Models.Enums;
using BankApp.Server.Services.Implementations;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Server.Controllers
{
    [ApiController]
    [Route("api/savings-ui-rules")]
    public class SavingsUiRulesController : ControllerBase
    {
        private readonly SavingsUiRulesService _uiRulesService = new();

        [HttpGet("parse-positive-amount")]
        public ActionResult<decimal> ParsePositiveAmount([FromQuery] string text)
        {
            var isValid = _uiRulesService.TryParsePositiveAmount(text, out var amount);
            if (isValid)
            {
                return Ok(amount);
            }
            return BadRequest("Invalid amount. Please enter a positive number.");
        }

        [HttpPost("deposit-preview")]
        public ActionResult<string> GetDepositPreview([FromBody] DepositPreviewRequest request)
        {
            var previewText = _uiRulesService.BuildDepositPreview(request.DepositAmountText, request.SelectedAccount);
            return Ok(previewText);
        }

        [HttpGet("withdraw-net-amount")]
        public ActionResult<decimal> GetWithdrawNetAmount([FromQuery] decimal requestedAmount, [FromQuery] decimal penalty)
        {
            var netAmount = _uiRulesService.CalculateWithdrawNetAmount(requestedAmount, penalty);
            return Ok(netAmount);
        }

        [HttpGet("parse-deposit-frequency")]
        public ActionResult<DepositFrequency> ParseDepositFrequency([FromQuery] string frequencyText)
        {
            var isValid = _uiRulesService.TryParseDepositFrequency(frequencyText, out DepositFrequency frequency);
            if (isValid)
            {
                return Ok(frequency);
            }
            return BadRequest();
        }

        [HttpGet("total-pages")]
        public ActionResult<int> GetTotalPages([FromQuery] int totalCount, [FromQuery] int pageSize)
        {
            var pages = _uiRulesService.CalculateTotalPages(totalCount, pageSize);
            return Ok(pages);
        }

        [HttpPost("validate-create-account")]
        public ActionResult<Dictionary<string, string>> ValidateCreateAccount([FromBody] ValidateCreateAccountRequest request)
        {
            var errors = _uiRulesService.ValidateCreateAccount(
                request.SelectedSavingsType,
                request.AccountName,
                request.InitialDepositText,
                request.HasFundingSource,
                request.SelectedFrequency,
                request.TargetAmount,
                request.TargetDate,
                request.IsGoalSavings
            );

            return Ok(errors);
        }
    }
}
