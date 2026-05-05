using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApp.Client.Services.Interfaces
{
    public interface ILoanDialogStateApiService
    {
        Task<bool> GetShouldComputeEstimate(double desiredAmount, int preferredTermMonths, string purpose);
    }
}
