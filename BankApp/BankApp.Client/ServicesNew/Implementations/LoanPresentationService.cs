using BankApp.Models.Features.Loans;
using BankApp.Server.Utilities;

namespace BankApp.Server.Services.Implementations
{
    public class LoanPresentationService
    {
        public double GetRepaymentProgress(Loan loan)
        {
            return proxyLoanPresentationRepository.GetRepaymentProgress(loan);
        }
    }
}