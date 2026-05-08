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
        private readonly LoanApplicationPresentationService _loanApplicationPresentationService = new();

        [HttpGet]
        public IActionResult GetBuildApplicationOutcome([FromQuery] string? rejectionReason)
        {
            var result = _loanApplicationPresentationService.BuildApplicationOutcome(rejectionReason);
            return Ok(result);
        }
    }
}
