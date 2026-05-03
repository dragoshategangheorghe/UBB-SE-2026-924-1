using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BankApp.Models.Enums;
using BankApp.Models.DTOs.Loans;

namespace BankApp.Models.Features.Loans
{
    /// <summary>
    /// Provides validation rules for incoming loan application requests.
    /// </summary>
    public class LoanApplicationValidator
    {
        private const decimal MinimumDesiredAmountExclusive = 0m;
        private const int MinimumTermMonthsExclusive = 0;

        private const string RequestCannotBeNullMessage = "Request cannot be null";
        private const string DesiredAmountInvalidMessage = "Desired amount must be greater than zero";
        private const string InvalidLoanTypeMessage = "Invalid Loan Type";
        private const string TermInvalidMessage = "Term must be greater than zero";
        private const string PurposeRequiredMessage = "Purpose is required";

        /// <summary>
        /// Validates a loan application request and throws when invalid.
        /// </summary>
        /// <param name="request">The request to validate.</param>
        public void Validate(LoanApplicationRequest request)
        {
            if (request == null)
            {
                throw new Exception(RequestCannotBeNullMessage);
            }

            if (request.DesiredAmount <= MinimumDesiredAmountExclusive)
            {
                throw new Exception(DesiredAmountInvalidMessage);
            }

            if (!Enum.IsDefined(typeof(LoanType), request.LoanType))
            {
                throw new Exception(InvalidLoanTypeMessage);
            }

            if (request.PreferredTermMonths <= MinimumTermMonthsExclusive)
            {
                throw new Exception(TermInvalidMessage);
            }

            if (string.IsNullOrWhiteSpace(request.Purpose))
            {
                throw new Exception(PurposeRequiredMessage);
            }
        }
    }
}
