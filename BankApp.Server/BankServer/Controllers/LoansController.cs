using BankApp.Models.DTOs.Loans;
using BankApp.Models.Enums;
using BankApp.Models.Features.Loans;
using BankApp.Server.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoansController : ControllerBase
    {
        private readonly ILoanService _loanService;

        public LoansController(ILoanService loanService)
        {
            _loanService = loanService;
        }

        [HttpGet]
        public async Task<ActionResult<List<Loan>>> GetAllLoansAsync()
        {
            var result = await _loanService.GetAllLoansAsync();
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Loan>> GetLoanByIdAsync([FromRoute] int id)
        {
            var loan = await _loanService.GetLoanByIdAsync(id);
            return Ok(loan);
        }

        [HttpGet("by-user/{userId:int}")]
        public async Task<ActionResult<List<Loan>>> GetLoansByUserAsync([FromRoute] int userId)
        {
            var result = await _loanService.GetLoansByUserAsync(userId);
            if (result == null || !result.Any())
            {
                return BadRequest();
            }

            return Ok(result);
        }

        [HttpGet("by-status/{loanStatus}")]
        public async Task<ActionResult<List<Loan>>> GetLoansByStatusAsync([FromRoute] LoanStatus loanStatus)
        {
            var result = await _loanService.GetLoansByStatusAsync(loanStatus);
            return Ok(result);
        }

        [HttpGet("by-type/{loanType}")]
        public async Task<ActionResult<List<Loan>>> GetLoansByTypeAsync([FromRoute] LoanType loanType)
        {
            var result = await _loanService.GetLoansByTypeAsync(loanType);
            return Ok(result);
        }

        [HttpPost("apply")]
        public async Task<ActionResult<(LoanApplicationStatus Status, string? RejectionReason)>> SubmitLoanApplicationAsync([FromBody] LoanApplicationRequest request)
        {
            try
            {
                var result = await _loanService.SubmitLoanApplicationAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("estimate")]
        public ActionResult<LoanEstimate> GetLoanEstimate([FromBody] LoanApplicationRequest request)
        {
            try
            {
                var estimate = _loanService.GetLoanEstimate(request);
                return Ok(estimate);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{loanId:int}/pay-installment")]
        public async Task<IActionResult> PayInstallmentAsync([FromRoute] int loanId, [FromQuery] decimal? customAmount)
        {
            try
            {
                await _loanService.PayInstallmentAsync(loanId, customAmount);
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

        [HttpGet("payment-amount/{input:string}")]
        public ActionResult<decimal?> GetParsedCustomPaymentAmount([FromRoute] string input)
        {
            var result = _loanService.ParseCustomPaymentAmount(input);
            if (result == null)
            {
                return BadRequest("Invalid input format. Please provide a valid amount or percentage.");
            }

            return Ok(result);
        }

        [HttpPost("normalize-payment-amount")]
        public ActionResult<decimal> NormalizeCustomPaymentAmount([FromBody] Loan loan, [FromQuery] decimal? currentCustomAmount)
        {
            var result = _loanService.NormalizeCustomPaymentAmount(loan, currentCustomAmount);
            return Ok(result);
        }

        [HttpPost("repayment-progress")]
        public ActionResult<double> GetRepaymentProgress([FromBody] Loan loan)
        {
            var result = _loanService.GetRepaymentProgress(loan);
            return Ok(result);
        }

        [HttpGet("{loanId:int}/amortization-schedule")]
        public async Task<ActionResult<List<AmortizationRow>>> GetAmortizationAsync(int loanId)
        {
            var rows = _loanService.GetAmortizationAsync(loanId);
            return Ok(rows);
        }
    }
}
