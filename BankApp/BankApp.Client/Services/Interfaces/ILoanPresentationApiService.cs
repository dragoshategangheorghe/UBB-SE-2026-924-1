using BankApp.Models.Features.Loans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApp.Client.Services.Interfaces
{
    public interface ILoanPresentationApiService
    {
        Task<decimal> GetRepaymentProgress(Loan loan);
    }
}
