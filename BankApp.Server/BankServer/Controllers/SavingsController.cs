using BankApp.Models.DTOs.Savings;
using BankApp.Models.Features.Investments;
using BankApp.Models.Features.Savings;
using BankApp.Server.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SavingsController : ControllerBase
    {
        private readonly ISavingsService _savingsService;

        public SavingsController(ISavingsService savingsService)
        {
            _savingsService = savingsService;
        }

        [HttpPost("create-account")]
        public async Task<ActionResult<SavingsAccount>> CreateAccountAsync([FromBody] CreateSavingsAccountDto account)
        {
            try
            {
                var newSavingsAccount = _savingsService.CreateAccountAsync(account);
                return Ok(newSavingsAccount);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("user/{userId:int}")]
        public async Task<ActionResult<List<SavingsAccount>>> GetAccounts([FromRoute] int userId, [FromQuery] bool includesClosed = false)
        {
            try
            {
                var accounts = await _savingsService.GetAccountsAsync(userId, includesClosed);
                return Ok(accounts);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{accountId:int}/deposit")]
        public async Task<ActionResult<DepositResponseDto>> DepositAsync([FromRoute] int accountId, [FromQuery] decimal amount, [FromQuery] string source, [FromQuery] int userId)
        {
            try
            {
                var response = await _savingsService.DepositAsync(accountId, amount, source, userId);
                return response;
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{accountId}/withdraw")]
        public async Task<ActionResult<WithdrawResponseDto>> Withdraw(int accountId, [FromQuery] decimal amount, [FromQuery] string destinationLabel, [FromQuery] int userId)
        {
            try
            {
                var response = await _savingsService.WithdrawAsync(accountId, amount, destinationLabel, userId);
                return Ok(response);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{accountId}/close")]
        public async Task<ActionResult<ClosureResultDto>> CloseAccountAsync([FromRoute] int accountId, [FromQuery] int destinationAccountId, [FromQuery] int userId)
        {
            try
            {
                var response = await _savingsService.CloseAccountAsync(accountId, destinationAccountId, userId);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{accountId}/auto-deposit")]
        public async Task<ActionResult<AutoDeposit>> GetAutoDepositAsync(int accountId)
        {
            var autoDeposit = await _savingsService.GetAutoDepositAsync(accountId);
            if (autoDeposit == null)
            {
                return NotFound("Auto-deposit not found.");
            }

            return Ok(autoDeposit);
        }

        [HttpPost("auto-deposit")]
        public async Task<IActionResult> SaveAutoDepositAsync([FromBody] AutoDeposit autoDeposit)
        {
            await _savingsService.SaveAutoDepositAsync(autoDeposit);
            return Ok();
        }

        [HttpGet("user/{userId}/funding-sources")]
        public async Task<ActionResult<List<FundingSourceOption>>> GetFundingSourcesAsync(int userId)
        {
            var sources = await _savingsService.GetFundingSourcesAsync(userId);
            return Ok(sources);
        }

        [HttpGet("{accountId}/transactions")]
        public async Task<ActionResult> GetTransactionsAsync(int accountId, [FromQuery] string filter = "", [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var result = await _savingsService.GetTransactionsAsync(accountId, filter, page, pageSize);
                return Ok(new
                {
                    Items = result.Items,
                    TotalCount = result.TotalCount,
                    Page = page,
                    PageSize = pageSize
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{currentAccountId}/valid-destinations")]
        public async Task<ActionResult<List<SavingsAccount>>> GetValidTransferDestinationsAsync(int currentAccountId)
        {
            var destinations = await _savingsService.GetValidTransferDestinationsAsync(currentAccountId);
            return Ok(destinations);
        }

        [HttpGet("penalty/compute")]
        public ActionResult<decimal> ComputeWithdrawalPenalty([FromQuery] decimal amount)
        {
            var penalty = _savingsService.ComputeWithdrawalPenalty(amount);
            return Ok(penalty);
        }

        [HttpPost("risk-early-withdrawal")]
        public ActionResult<bool> HasRiskEarlyWithdrawal([FromBody] SavingsAccount savingsAccount)
        {
            var hasRisk = _savingsService.HasRiskEarlyWithdrawal(savingsAccount);
            return Ok(hasRisk);
        }

        [HttpGet("penalty/rate/{penaltyCase}")]
        public ActionResult<decimal> GetPenaltyDecimalFor(string penaltyCase)
        {
            try
            {
                var penaltyRate = _savingsService.GetPenaltyDecimalFor(penaltyCase);
                return Ok(penaltyRate);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
