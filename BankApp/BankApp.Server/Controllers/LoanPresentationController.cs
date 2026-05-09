using BankApp.Models.Features.Loans;
using BankApp.Server.Services.Implementations;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Server.Controllers
{
    [ApiController]
    [Route("api/loans/repayment-progress")]
    public class LoanPresentationController : ControllerBase
    {
        private readonly LoanPresentationService _loanPresentationService = new ();

        [HttpPost]
        public IActionResult GetRepaymentProgress([FromBody] Loan loan)
        {
            double progress = _loanPresentationService.GetRepaymentProgress(loan);
            return Ok(progress);
        }
    }
}
