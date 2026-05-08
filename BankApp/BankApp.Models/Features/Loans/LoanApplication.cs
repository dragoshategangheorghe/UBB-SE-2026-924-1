using System.ComponentModel.DataAnnotations;
using BankApp.Models.Entities;
using BankApp.Models.Enums;

namespace BankApp.Models.Features.Loans
{
    public class LoanApplication
    {
        /// <summary>
        /// Gets or sets the unique application identifier.
        /// </summary>
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the applicant user.
        /// </summary>
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// Gets or sets the requested loan type.
        /// </summary>
        public LoanType LoanType { get; set; }

        /// <summary>
        /// Gets or sets the requested loan amount.
        /// </summary>
        public decimal DesiredAmount { get; set; }

        /// <summary>
        /// Gets or sets the preferred repayment term in months.
        /// </summary>
        public int PreferredTermMonths { get; set; }

        /// <summary>
        /// Gets or sets the business or personal purpose for the request.
        /// </summary>
        public required string Purpose { get; set; }

        /// <summary>
        /// Gets or sets the current review status of the application.
        /// </summary>
        public LoanApplicationStatus ApplicationStatus { get; set; }

        /// <summary>
        /// Gets or sets the rejection reason when the application is denied.
        /// </summary>
        public string? RejectionReason { get; set; }
    }
}