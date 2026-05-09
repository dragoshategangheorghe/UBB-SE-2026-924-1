using BankApp.Server.Services.Implementations;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Server.Controllers
{
    [ApiController]
    [Route("api/loans/should-compute-estimate")]
    public class LoanDialogStateController : ControllerBase
    {
        private readonly LoanDialogStateService _loanDialogStateService = new();

        private const int PositiveThreshold = 0;

        [HttpGet] // query means api/loans/should-compute-estimate"?q1=aaa&hmm=wow& ... I hope that's clear
        public IActionResult GetShouldComputeEstimate([FromQuery] double desiredAmount, [FromQuery] int preferredTermMonths, [FromQuery] string purpose)
        {
            bool result = desiredAmount > PositiveThreshold &&
                          preferredTermMonths > PositiveThreshold &&
                          !string.IsNullOrWhiteSpace(purpose);
            return Ok(result);
        }
    }
}
