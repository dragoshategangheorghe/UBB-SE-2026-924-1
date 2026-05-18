using System.Linq;
using BankApp.Models.DTOs.Savings;
using BankApp.Models.Features.Investments;
using BankApp.Models.Features.Savings;
using BankApp.Server.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SavingsController : ControllerBase
    {
        private readonly ISavingsRepository _savingsRepository;

        public SavingsController(ISavingsRepository savingsRepository)
        {
            _savingsRepository = savingsRepository;
        }

        [HttpPost("create-account")]
        public async Task<ActionResult<SavingsAccount>> CreateAccountAsync(
            [FromBody] CreateSavingsAccountDto account,
            [FromQuery] decimal apy)
        {
            try
            {
                var newSavingsAccount = await _savingsRepository.CreateSavingsAccountAsync(account, apy);
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
                var accounts = await _savingsRepository.GetSavingsAccountsByUserIdAsync(userId, includesClosed);
                return Ok(accounts);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{accountId:int}/deposit")]
        public async Task<ActionResult<DepositResponseDto>> DepositAsync([FromRoute] int accountId, [FromQuery] decimal amount, [FromQuery] string source)
        {
            try
            {
                var response = await _savingsRepository.DepositAsync(accountId, amount, source);
                return Ok(response);
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

        [HttpPost("{accountId:int}/withdraw")]
        public async Task<ActionResult<WithdrawResponseDto>> WithdrawAsync(
            [FromRoute] int accountId,
            [FromQuery] decimal amount,
            [FromQuery] string destinationLabel,
            [FromQuery] decimal earlyWithdrawalPenalty)
        {
            try
            {
                var response = await _savingsRepository.WithdrawAsync(accountId, amount, destinationLabel, earlyWithdrawalPenalty);
                return Ok(response);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{accountId:int}/close")]
        public async Task<ActionResult<ClosureResultDto>> CloseAccountAsync(
            [FromRoute] int accountId,
            [FromQuery] int destinationAccountId,
            [FromQuery] decimal transferAmount,
            [FromQuery] decimal earlyClosurePenalty)
        {
            try
            {
                var response = await _savingsRepository.CloseSavingsAccountAsync(accountId, destinationAccountId, transferAmount, earlyClosurePenalty);
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
            var autoDeposit = await _savingsRepository.GetAutoDepositAsync(accountId);
            if (autoDeposit == null)
            {
                return NotFound("Auto-deposit not found.");
            }

            return Ok(autoDeposit);
        }

        [HttpPost("auto-deposit")]
        public async Task<IActionResult> SaveAutoDepositAsync([FromBody] AutoDepositUpsertDto autoDeposit)
        {
            await _savingsRepository.SaveAutoDepositAsync(autoDeposit.ToAutoDeposit());
            return Ok();
        }

        [HttpGet("user/{userId}/funding-sources")]
        public async Task<ActionResult<List<FundingSourceOption>>> GetFundingSourcesAsync(int userId)
        {
            var sources = await _savingsRepository.GetFundingSourcesAsync(userId);
            return Ok(sources);
        }

        [HttpGet("{accountId}/transactions")]
        public async Task<ActionResult> GetTransactionsAsync(int accountId, [FromQuery] string filter = "", [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var result = await _savingsRepository.GetTransactionsPagedAsync(accountId, filter, page, pageSize);
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
        public async Task<ActionResult<List<SavingsAccount>>> GetValidTransferDestinationsAsync(
            int currentAccountId,
            [FromQuery] int userId)
        {
            var accounts = await _savingsRepository.GetSavingsAccountsByUserIdAsync(userId, false);
            return Ok(accounts.Where(a => a.IdentificationNumber != currentAccountId).ToList());
        }
    }
}
