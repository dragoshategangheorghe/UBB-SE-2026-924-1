using BankApp.Models.DTOs.Savings;
using BankApp.Models.Features.Investments;
using BankApp.Models.Features.Savings;
using BankApp.Server.Services.Implementations;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Server.Controllers
{
    [ApiController]
    public class SavingsWorkflowController : ControllerBase
    {
        private readonly SavingsWorkflowService _workflowService = new();

        [HttpPost("default-funding-source")]
        public ActionResult<FundingSourceOption> GetDefaultFundingSource([FromBody] IEnumerable<FundingSourceOption> fundingSources)
        {
            if (fundingSources == null) return BadRequest("List of funding sources cannot be null.");

            var result = _workflowService.GetDefaultFundingSource(fundingSources);

            if (result == null)
            {
                return NoContent();
            }

            return Ok(result);
        }

        [HttpPost("default-close-destination")]
        public ActionResult<int> GetDefaultCloseDestinationId([FromBody] IEnumerable<SavingsAccount> destinationAccounts)
        {
            if (destinationAccounts == null) return BadRequest("List of accounts cannot be null.");

            var destinationId = _workflowService.GetDefaultCloseDestinationId(destinationAccounts);
            return Ok(destinationId);
        }

        [HttpPost("validate-withdraw")]
        public ActionResult ValidateWithdrawRequest([FromBody] ValidateWithdrawRequestDto request)
        {
            var result = _workflowService.ValidateWithdrawRequest(request.Amount, request.Destination);
            return Ok(new
            {
                IsValid = result.IsValid,
                ErrorMessage = result.ErrorMessage
            });
        }

        [HttpPost("withdraw-result-message")]
        public ActionResult<string> BuildWithdrawResultMessage([FromBody] WithdrawResponseDto response)
        {
            var message = _workflowService.BuildWithdrawResultMessage(response);
            return Ok(message);
        }

        [HttpGet("validate-close")]
        public ActionResult ValidateCloseConfirmation([FromQuery] bool userConfirmed, [FromQuery] int destinationId)
        {
            var result = _workflowService.ValidateCloseConfirmation(userConfirmed, destinationId);

            return Ok(new
            {
                IsValid = result.IsValid,
                ErrorMessage = result.ErrorMessage
            });
        }

        [HttpGet("can-move-next")]
        public ActionResult<bool> CanMoveToNextPage([FromQuery] int currentPage, [FromQuery] int totalPages)
        {
            var canMove = _workflowService.CanMoveToNextPage(currentPage, totalPages);
            return Ok(canMove);
        }

        [HttpGet("can-move-previous")]
        public ActionResult<bool> CanMoveToPreviousPage([FromQuery] int currentPage)
        {
            var canMove = _workflowService.CanMoveToPreviousPage(currentPage);
            return Ok(canMove);
        }
    }
}
