using BankApp.Models.Features.Loans;
using BankApp.Server.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Server.Controllers
{
    [ApiController]
    [Route("api/loans/repayment-progress")]
    public class LoanPresentationController : ControllerBase
    {
        [HttpPost]
        public IActionResult GetRepaymentProgress([FromBody] Loan loan)
        {
            double progress = (double)AmortizationCalculator.ComputeRepaymentProgress(
                                loan.Principal,
                                loan.OutstandingBalance);
            return Ok(progress);
        }
    }
}
