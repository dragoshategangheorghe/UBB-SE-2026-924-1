using BankApp.Models.DTOs.Savings;
using BankApp.Models.Features.Investments;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Server.Controllers
{
    [ApiController]
    [Route("api/savings-workflow")]
    public class SavingsWorkflowController : ControllerBase
    {
        private const int NoDestinationId = 0;
        private const decimal PositiveAmountThreshold = 0m;
        private const decimal NoPenaltyAmount = 0m;
        private const int FirstPage = 1;

        [HttpPost("default-funding-source")]
        public ActionResult<FundingSourceOption> GetDefaultFundingSource([FromBody] IEnumerable<FundingSourceOption> fundingSources)
        {
            if (fundingSources == null)
            {
                return BadRequest("List of funding sources cannot be null.");
            }

            var result = fundingSources.FirstOrDefault();

            if (result == null)
            {
                return NoContent();
            }

            return Ok(result);
        }

        [HttpPost("default-close-destination")]
        public ActionResult<int> GetDefaultCloseDestinationId([FromBody] IEnumerable<SavingsAccountSnapshotDto> destinationAccounts)
        {
            if (destinationAccounts == null)
            {
                return BadRequest("List of accounts cannot be null.");
            }

            var destinationId = destinationAccounts.FirstOrDefault()?.IdentificationNumber ?? NoDestinationId;
            return Ok(destinationId);
        }

        [HttpPost("validate-withdraw")]
        public ActionResult ValidateWithdrawRequest([FromQuery] decimal amount, [FromBody] FundingSourceOption? destination)
        {
            (bool IsValid, string ErrorMessage) result;

            if (amount <= PositiveAmountThreshold)
            {
                result = (false, "Please enter a valid amount.");
            }
            else if (destination == null)
            {
                result = (false, "Please select a destination account.");
            }
            else
            {
                result = (true, string.Empty);
            }

            return Ok(new
            {
                IsValid = result.IsValid,
                ErrorMessage = result.ErrorMessage
            });
        }

        [HttpPost("withdraw-result-message")]
        public ActionResult<string> BuildWithdrawResultMessage([FromBody] WithdrawResponseDto response)
        {
            string message;

            if (!response.Success)
            {
                message = response.Message;
            }
            else
            {
                var penaltyText = response.PenaltyApplied > NoPenaltyAmount
                                ? $" (penalty: ${response.PenaltyApplied:N2})"
                                : string.Empty;
                message = $"Withdrawn: ${response.AmountWithdrawn:N2}{penaltyText}. New balance: ${response.NewBalance:N2}";
            }

            return Ok(message);
        }

        [HttpGet("validate-close")]
        public ActionResult ValidateCloseConfirmation([FromQuery] bool userConfirmed, [FromQuery] int destinationId)
        {
            (bool IsValid, string ErrorMessage) result;

            if (!userConfirmed)
            {
                result = (false, "Please confirm account closure.");
            }
            else if (destinationId == NoDestinationId)
            {
                result = (false, "Please select a destination account.");
            }
            else
            {
                result = (true, string.Empty);
            }

            return Ok(new
            {
                IsValid = result.IsValid,
                ErrorMessage = result.ErrorMessage
            });
        }

        [HttpGet("can-move-next")]
        public ActionResult<bool> CanMoveToNextPage([FromQuery] int currentPage, [FromQuery] int totalPages)
        {
            var canMove = currentPage < totalPages;
            return Ok(canMove);
        }

        [HttpGet("can-move-previous")]
        public ActionResult<bool> CanMoveToPreviousPage([FromQuery] int currentPage)
        {
            var canMove = currentPage > FirstPage;
            return Ok(canMove);
        }
    }
}
