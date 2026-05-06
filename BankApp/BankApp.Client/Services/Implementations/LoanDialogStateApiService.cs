using BankApp.Client.Services.Interfaces;
using BankApp.Client.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApp.Client.Services.Implementations
{
    public class LoanDialogStateApiService : ILoanDialogStateApiService
    {
        private readonly ApiService _apiService;

        public LoanDialogStateApiService(ApiService apiService)
        {
            _apiService = apiService;
        }

        public Task<bool> GetShouldComputeEstimate(double desiredAmount, int preferredTermMonths, string purpose)
        {
            return _apiService.GetAsync<bool>($"api/loans/should-compute-estimate?desiredAmount={desiredAmount}&preferredTermMonths={preferredTermMonths}&purpose={Uri.EscapeDataString(purpose)}");
        }
    }
}
