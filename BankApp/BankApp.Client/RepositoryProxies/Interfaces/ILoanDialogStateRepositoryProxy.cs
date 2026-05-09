using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApp.Client.RepositoryProxies.Interfaces
{
    public interface ILoanDialogStateRepositoryProxy
    {
        Task<bool> GetShouldComputeEstimateAsync(double desiredAmount, int preferredTermMonths, string purpose);
    }
}
