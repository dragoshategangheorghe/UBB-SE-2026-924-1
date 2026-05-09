using BankApp.Server.Services.Implementations;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Server.Controllers
{
    [ApiController]
    [Route("api/loans/should-compute-estimate")]
    public class LoanDialogStateController : ControllerBase
    {
        private readonly LoanDialogStateService _loanDialogStateService = new ();

        [HttpGet]
        public IActionResult GetShouldComputeEstimate([FromQuery] double desiredAmount, [FromQuery] int preferredTermMonths, [FromQuery] string purpose)
        {
            bool result = _loanDialogStateService.ShouldComputeEstimate(desiredAmount, preferredTermMonths, purpose);
            return Ok(result);
        }
    }
}
