using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApp.Client.Services.Interfaces
{
    public interface ILoanApplicationPresentationApiService
    {
        Task<BuildApplicationOutcomeResponse?> GetBuildApplicationOutcome(string? rejectionReason);
    }

    public class BuildApplicationOutcomeResponse
    {
        public bool IsApproved { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
