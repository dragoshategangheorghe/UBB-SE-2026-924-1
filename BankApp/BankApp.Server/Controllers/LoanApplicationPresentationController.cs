using BankApp.Server.Services.Implementations;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Server.Controllers
{
    [ApiController]
    [Route("api/loans/loan-application-presentation-outcome")]
    public class LoanApplicationPresentationController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetBuildApplicationOutcome([FromQuery] string? rejectionReason)
        {
            var result = rejectionReason == null
                ? (true, "Your loan application has been approved!")
                : (false, $"Application rejected: {rejectionReason}");
            return Ok(result);
        }
    }
}
