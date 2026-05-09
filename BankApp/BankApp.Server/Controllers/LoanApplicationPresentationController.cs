using BankApp.Server.Repositories.Implementations;
using BankApp.Server.Repositories.Interfaces;
using BankApp.Server.Services.Implementations;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Server.Controllers
{
    [ApiController]
    [Route("api/loans/loan-application-presentation-outcome")]
    public class LoanApplicationPresentationController : ControllerBase
    {
        // this service doesn't have an interface and doesn't have any dependency,
        // so it can be instantiated directly here (it's like a utility class)
        [HttpGet]
        public IActionResult GetBuildApplicationOutcome([FromQuery] string? rejectionReason)
        {
            var rejectionResult = rejectionReason == null
                ? (true, "Your loan application has been approved!")
                : (false, $"Application rejected: {rejectionReason}");
            return Ok(rejectionResult);
        }
    }
}
