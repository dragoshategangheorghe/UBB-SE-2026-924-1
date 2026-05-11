using Microsoft.AspNetCore.Mvc;

namespace BankApp.Server.Controllers
{
    [ApiController]
    [Route("api/loans/should-compute-estimate")]
    public class LoanDialogStateController : ControllerBase
    {
        private const int PositiveThreshold = 0;

        [HttpGet]
        public IActionResult GetShouldComputeEstimate([FromQuery] double desiredAmount, [FromQuery] int preferredTermMonths, [FromQuery] string purpose)
        {
            bool result = desiredAmount > PositiveThreshold &&
                          preferredTermMonths > PositiveThreshold &&
                          !string.IsNullOrWhiteSpace(purpose);
            return Ok(result);
        }
    }
}
