using BankApp.Models.DTOs.Loans;
using BankApp.Models.Enums;
using BankApp.Models.Features.Loans;
using BankApp.Server.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoansController : ControllerBase
    {
        private readonly ILoanRepository _loanRepository;

        public LoansController(ILoanRepository loanRepository)
        {
            _loanRepository = loanRepository;
        }

        [HttpGet]
        public async Task<ActionResult<List<Loan>>> GetAllLoansAsync()
        {
            var result = await _loanRepository.GetAllLoansAsync();
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Loan>> GetLoanByIdAsync([FromRoute] int id)
        {
            var loan = await _loanRepository.GetLoanByIdAsync(id);
            return Ok(loan);
        }

        [HttpGet("by-user/{userId:int}")]
        public async Task<ActionResult<List<Loan>>> GetLoansByUserAsync([FromRoute] int userId)
        {
            var result = await _loanRepository.GetLoansByUserAsync(userId);
            return Ok(result ?? new List<Loan>());
        }

        [HttpGet("by-status/{loanStatus}")]
        public async Task<ActionResult<List<Loan>>> GetLoansByStatusAsync([FromRoute] LoanStatus loanStatus)
        {
            var result = await _loanRepository.GetLoansByStatusAsync(loanStatus);
            return Ok(result);
        }

        [HttpGet("by-type/{loanType}")]
        public async Task<ActionResult<List<Loan>>> GetLoansByTypeAsync([FromRoute] LoanType loanType)
        {
            var result = await _loanRepository.GetLoansByTypeAsync(loanType);
            return Ok(result);
        }

        [HttpPost("applications")]
        public async Task<ActionResult<int>> CreateLoanApplicationAsync([FromBody] LoanApplicationRequest request)
        {
            try
            {
                var id = await _loanRepository.CreateLoanApplicationAsync(request);
                return Ok(id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("applications/{applicationId:int}/status")]
        public async Task<IActionResult> UpdateLoanApplicationStatusAsync(
            [FromRoute] int applicationId,
            [FromQuery] LoanApplicationStatus status,
            [FromQuery] string? reason)
        {
            try
            {
                await _loanRepository.UpdateLoanApplicationStatusAsync(applicationId, status, reason);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<int>> CreateLoanAsync([FromBody] LoanCreateDto loan)
        {
            try
            {
                var id = await _loanRepository.CreateLoanAsync(loan.ToLoan());
                return Ok(id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{loanId:int}/after-payment")]
        public async Task<IActionResult> UpdateLoanAfterPaymentAsync(
            [FromRoute] int loanId,
            [FromQuery] decimal newBalance,
            [FromQuery] int newRemainingMonths,
            [FromQuery] LoanStatus newStatus)
        {
            try
            {
                await _loanRepository.UpdateLoanAfterPaymentAsync(loanId, newBalance, newRemainingMonths, newStatus);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{loanId:int}/amortization-schedule")]
        public async Task<ActionResult<List<AmortizationRow>>> GetAmortizationAsync(int loanId)
        {
            var rows = await _loanRepository.GetAmortizationAsync(loanId);
            return Ok(rows);
        }

        [HttpPost("{loanId:int}/amortization-schedule")]
        public async Task<IActionResult> SaveAmortizationAsync([FromRoute] int loanId, [FromBody] List<AmortizationRowUpsertDto> rows)
        {
            try
            {
                if (rows == null || rows.Any(r => r.LoanId != loanId))
                {
                    return BadRequest("Invalid amortization rows payload.");
                }

                await _loanRepository.SaveAmortizationAsync(rows.Select(row => row.ToAmortizationRow()).ToList());
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
