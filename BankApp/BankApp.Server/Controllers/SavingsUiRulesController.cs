using System.Globalization;
using BankApp.Models.DTOs.Savings;
using BankApp.Models.Enums;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Server.Controllers
{
    [ApiController]
    [Route("api/savings-ui-rules")]
    public class SavingsUiRulesController : ControllerBase
    {
        private const decimal PositiveAmountThreshold = 0m;
        private const int NoPages = 0;

        [HttpGet("parse-positive-amount")]
        public ActionResult<decimal> ParsePositiveAmount([FromQuery] string text)
        {
            var isValid = SavingsUiRulesController.TryParsePositiveAmount(text, out var amount);
            if (isValid)
            {
                return Ok(amount);
            }
            return BadRequest("Invalid amount. Please enter a positive number.");
        }

        [HttpPost("deposit-preview")]
        public ActionResult<string> GetDepositPreview([FromQuery] string depositAmountText, [FromBody] SavingsAccountSnapshotDto selectedAccount)
        {
            string previewText;
            bool isDepositTextPositiveAmount = SavingsUiRulesController.TryParsePositiveAmount(depositAmountText, out var amount);

            if (selectedAccount == null || !isDepositTextPositiveAmount)
            {
                previewText = string.Empty;
            }
            else
            {
                previewText = $"New balance will be: ${selectedAccount.Balance + amount:N2}";
            }
            return Ok(previewText);
        }

        [HttpGet("withdraw-net-amount")]
        public ActionResult<decimal> GetWithdrawNetAmount([FromQuery] decimal requestedAmount, [FromQuery] decimal penalty)
        {
            var netAmount = requestedAmount - penalty;
            return Ok(netAmount);
        }

        [HttpGet("parse-deposit-frequency")]
        public ActionResult<DepositFrequency> ParseDepositFrequency([FromQuery] string frequencyText)
        {
            var isValid = Enum.TryParse(frequencyText, out DepositFrequency frequency);
            if (isValid)
            {
                return Ok(frequency);
            }
            return BadRequest();
        }

        [HttpGet("total-pages")]
        public ActionResult<int> GetTotalPages([FromQuery] int totalCount, [FromQuery] int pageSize)
        {
            var pages = pageSize <= NoPages
                      ? NoPages
                      : (int)Math.Ceiling((double)totalCount / pageSize);
            return Ok(pages);
        }

        [HttpPost("validate-create-account")]
        public ActionResult<Dictionary<string, string>> ValidateCreateAccount([FromBody] ValidateCreateAccountRequest request)
        {
            var errors = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(request.SelectedSavingsType))
            {
                errors["SavingsType"] = "Please select an account type.";
            }

            if (string.IsNullOrWhiteSpace(request.AccountName))
            {
                errors["AccountName"] = "Account name is required.";
            }

            if (!SavingsUiRulesController.TryParsePositiveAmount(request.InitialDepositText, out _))
            {
                errors["InitialDeposit"] = "Initial deposit must be a positive number.";
            }

            if (!request.HasFundingSource)
            {
                errors["FundingSource"] = "Please select a funding source.";
            }

            if (string.IsNullOrWhiteSpace(request.SelectedFrequency))
            {
                errors["Frequency"] = "Please select a deposit frequency.";
            }

            if (request.IsGoalSavings)
            {
                if (!request.TargetAmount.HasValue || request.TargetAmount.Value <= PositiveAmountThreshold)
                {
                    errors["TargetAmount"] = "Target amount is required.";
                }

                if (!request.TargetDate.HasValue)
                {
                    errors["TargetDate"] = "Target date is required.";
                }
                else if (request.TargetDate.Value.Date <= DateTime.Today)
                {
                    errors["TargetDate"] = "Target date must be in the future.";
                }
            }

            return Ok(errors);
        }

        // Note: this function was salvaged over from the service, because it has been called multiple times
        //  outside of the function which it is "routed" to
        //  I don't know if it's good to have a function that is not routed to by anything inside an API controller,
        //  this is just a helper function to not have to write out the same long conditional multiple times (DRY)
        private static bool TryParsePositiveAmount(string text, out decimal amount)
        {
            if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out amount) &&
                amount > PositiveAmountThreshold)
            {
                return true;
            }

            if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out amount) &&
                amount > PositiveAmountThreshold)
            {
                return true;
            }

            amount = PositiveAmountThreshold;
            return false;
        }
    }
}
