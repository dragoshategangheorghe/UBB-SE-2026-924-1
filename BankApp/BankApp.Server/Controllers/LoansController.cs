using BankApp.Models.DTOs.Loans;
using BankApp.Models.Enums;
using BankApp.Models.Features.Loans;
using BankApp.Server.Repositories.Interfaces;
using BankApp.Server.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoansController : ControllerBase
    {
        private readonly ILoanRepository loanRepository;

        public LoansController(ILoanRepository loanRepository)
        {
            this.loanRepository = loanRepository;
        }

        [HttpGet]
        public async Task<ActionResult<List<Loan>>> GetAllLoansAsync()
        {
            var result = await loanRepository.GetAllLoansAsync();
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Loan>> GetLoanByIdAsync([FromRoute] int id)
        {
            var loan = await loanRepository.GetLoanByIdAsync(id);
            return Ok(loan);
        }

        [HttpGet("by-user/{userId:int}")]
        public async Task<ActionResult<List<Loan>>> GetLoansByUserAsync([FromRoute] int userId)
        {
            var result = await loanRepository.GetLoansByUserAsync(userId);
            /* I feel like it's better to return an empty list - Alex
            if (result == null || !result.Any())
            {
                return BadRequest();
            }
            */

            return Ok(result);
        }

        [HttpGet("by-status/{loanStatus}")]
        public async Task<ActionResult<List<Loan>>> GetLoansByStatusAsync([FromRoute] LoanStatus loanStatus)
        {
            var result = await loanRepository.GetLoansByStatusAsync(loanStatus);
            return Ok(result);
        }

        [HttpGet("by-type/{loanType}")]
        public async Task<ActionResult<List<Loan>>> GetLoansByTypeAsync([FromRoute] LoanType loanType)
        {
            var result = await loanRepository.GetLoansByTypeAsync(loanType);
            return Ok(result);
        }

        [HttpPut("saveAmortization")]
        public async Task<ActionResult<List<Loan>>> SaveAmortizationAsync([FromBody] List<AmortizationRow> amortizationRows)
        {
            await loanRepository.SaveAmortizationAsync(amortizationRows);
            return Ok();
        }

        [HttpPut("{loanId:int}/updateLoanApplicationStatus")]
        public async Task<ActionResult<List<Loan>>> UpdateLoanApplicationStatus([FromRoute] int loanId, [FromQuery] LoanApplicationStatus loanApplicationStatus, [FromQuery] string? reason)
        {
            await loanRepository.UpdateLoanApplicationStatusAsync(loanId, loanApplicationStatus, reason);
            return Ok();
        }

        [HttpPost("apply")]
        public async Task<ActionResult<(LoanApplicationStatus Status, string? RejectionReason)>> SubmitLoanApplicationAsync([FromBody] LoanApplicationRequest loanApplicationRequest)
        {
            try
            {
                var result = await loanRepository.CreateLoanApplicationAsync(loanApplicationRequest);
                return Ok(result);
            }
            catch (Exception ex) // this is sus
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("apply")]
        public async Task<ActionResult<int>> CreateLoan([FromBody] Loan loan)
        {
            try
            {
                var loanId = await loanRepository.CreateLoanAsync(loan);
                return Ok(loanId);
            }
            catch (Exception ex) // this is sus
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{loanId:int}/pay-installment")]
        public async Task<IActionResult> UpdateLoanAfterPayment([FromRoute] int loanId, [FromQuery] decimal newBalance, [FromQuery] int newRemainingMonths, [FromQuery] LoanStatus newLoanStatus)
        {
            try
            {
                await loanRepository.UpdateLoanAfterPaymentAsync(loanId, newBalance, newRemainingMonths, newLoanStatus);
                return Ok();
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

        [HttpGet("{loanId:int}/amortization")]
        public async Task<ActionResult<List<AmortizationRow>>> GetAmortizationAsync(int loanId)
        {
            var rows = loanRepository.GetAmortizationAsync(loanId);
            return Ok(rows);
        }
    }
}
