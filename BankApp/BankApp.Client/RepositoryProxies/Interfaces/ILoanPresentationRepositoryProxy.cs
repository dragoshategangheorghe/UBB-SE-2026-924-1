using BankApp.Models.Features.Loans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApp.Client.RepositoryProxies.Interfaces
{
    public interface ILoanPresentationRepositoryProxy
    {
        Task<double> GetRepaymentProgressAsync(Loan loan);
    }
}
