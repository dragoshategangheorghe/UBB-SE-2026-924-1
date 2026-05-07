using BankApp.Client.Services.Interfaces;
using BankApp.Client.Utilities;
using BankApp.Models.DTOs.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApp.Client.Services.Implementations
{
    public class LoanApplicationPresentationApiService : ILoanApplicationPresentationApiService
    {
        private readonly ApiService _apiService;

        public LoanApplicationPresentationApiService(ApiService apiService)
        {
            _apiService = apiService;
        }

        public Task<BuildApplicationOutcomeResponse?> GetBuildApplicationOutcome(string? rejectionReason)
        {
            return _apiService.GetAsync<BuildApplicationOutcomeResponse>($"/api/loans/loan-application-presentation-outcome?rejectionReason={Uri.EscapeDataString(rejectionReason ?? string.Empty)}");
        }
    }
}
