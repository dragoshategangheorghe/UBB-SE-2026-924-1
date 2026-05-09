using BankApp.Models.DTOs.Savings;
using BankApp.Models.Features.Investments;
using BankApp.Models.Features.Savings;
using BankApp.Server.Repositories.Interfaces;
using BankApp.Server.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SavingsController : ControllerBase
    {
        private readonly ISavingsRepository savingsRepository;

        public SavingsController(ISavingsRepository savingsRepository)
        {
            savingsRepository = savingsRepository;
        }

        private int GetAuthenticatedUserId() => (int)HttpContext.Items["UserId"]!;

        [HttpPost("create-account")]
        public async Task<ActionResult<SavingsAccount>> CreateAccountAsync([FromBody] CreateSavingsAccountDto account, [FromQuery] decimal annualPercentageYield)
        {
            try
            {
                var newSavingsAccount = savingsRepository.CreateSavingsAccountAsync(account, annualPercentageYield);
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
        public async Task<ActionResult<List<SavingsAccount>>> GetAccountsAsync([FromRoute] int userId, [FromQuery] bool includesClosed = false)
        {
            try
            {
                var accounts = await savingsRepository.GetSavingsAccountsByUserIdAsync(userId, includesClosed);
                return Ok(accounts);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{accountId:int}/deposit")]
        public async Task<ActionResult<DepositResponseDto>> DepositAsync([FromRoute] int accountId, [FromQuery] decimal amount, [FromQuery] string source)
        {
            try
            {
                var response = await savingsRepository.DepositAsync(accountId, amount, source);
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

        [HttpGet("{accountId}/withdraw")]
        public async Task<ActionResult<WithdrawResponseDto>> WithdrawAsync(int accountId, [FromQuery] decimal amount, [FromQuery] string destinationLabel, [FromQuery] decimal earlyWithdrawalPenalty)
        {
            try
            {
                var response = await savingsRepository.WithdrawAsync(accountId, amount, destinationLabel, earlyWithdrawalPenalty);
                return Ok(response);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{accountId}/close")]
        public async Task<ActionResult<ClosureResultDto>> CloseAccountAsync([FromRoute] int accountId, [FromQuery] int destinationAccountId, [FromQuery] int userId, [FromQuery] decimal earlyClosurePenalty)
        {
            try
            {
                if (userId == 0)
                    userId = GetAuthenticatedUserId();

                var response = await savingsRepository.CloseSavingsAccountAsync(accountId, destinationAccountId, userId, earlyClosurePenalty);
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
            var autoDeposit = await savingsRepository.GetAutoDepositAsync(accountId);
            if (autoDeposit == null)
            {
                return NotFound("Auto-deposit not found.");
            }

            return Ok(autoDeposit);
        }

        [HttpPost("auto-deposit")]
        public async Task<IActionResult> SaveAutoDepositAsync([FromBody] AutoDeposit autoDeposit)
        {
            await savingsRepository.SaveAutoDepositAsync(autoDeposit);
            return Ok();
        }

        [HttpGet("user/{userId}/funding-sources")]
        public async Task<ActionResult<List<FundingSourceOption>>> GetFundingSourcesAsync(int userId)
        {
            var sources = await savingsRepository.GetFundingSourcesAsync(userId);
            return Ok(sources);
        }

        [HttpGet("{accountId}/transactions")]
        public async Task<ActionResult> GetTransactionsAsync(int accountId, [FromQuery] string filter = "", [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var result = await savingsRepository.GetTransactionsPagedAsync(accountId, filter, page, pageSize);
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
    }
}
