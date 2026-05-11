using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BankApp.Models.Entities;
using BankApp.Models.Enums;

namespace BankApp.Models.Features.Loans
{
    public class Loan
    {
        /// <summary>
        /// Gets or sets the unique loan identifier.
        /// </summary>
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the owning user.
        /// </summary>
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// Gets or sets the loan product type.
        /// </summary>
        public LoanType LoanType { get; set; }

        /// <summary>
        /// Gets or sets the original principal amount.
        /// </summary>
        public decimal Principal { get; set; }

        /// <summary>
        /// Gets or sets the remaining unpaid balance.
        /// </summary>
        public decimal OutstandingBalance { get; set; }

        /// <summary>
        /// Gets or sets the annual interest rate.
        /// </summary>
        public decimal InterestRate { get; set; }

        /// <summary>
        /// Gets or sets the required monthly installment amount.
        /// </summary>
        public decimal MonthlyInstallment { get; set; }

        /// <summary>
        /// Gets or sets the number of unpaid months remaining.
        /// </summary>
        public int RemainingMonths { get; set; }

        /// <summary>
        /// Gets or sets the current lifecycle status of the loan.
        /// </summary>
        public LoanStatus LoanStatus { get; set; }

        /// <summary>
        /// Gets or sets the original duration in months.
        /// </summary>
        public int TermInMonths { get; set; }

        /// <summary>
        /// Gets or sets the date when the loan became active.
        /// </summary>
        public DateTime StartDate { get; set; }

        // Navigation Properties
        public virtual ICollection<AmortizationRow> AmortizationRows { get; set; } = new List<AmortizationRow>();
    }
}
