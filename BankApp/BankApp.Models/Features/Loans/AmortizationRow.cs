// <copyright file="AmortizationRow.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Represents a single row from a loan amortization schedule.
/// </summary>
namespace BankApp.Models.Features.Loans
{
    public class AmortizationRow
    {
        /// <summary>
        /// Gets or sets the row identifier.
        /// </summary>
        [Key]

        public int Id { get; set; }
        public int LoanId { get; set; }

        /// <summary>
        /// Gets or sets the associated loan.
        /// </summary>
        public virtual Loan Loan { get; set; } = null!;

        /// <summary>
        /// Gets or sets the installment sequence number.
        /// </summary>
        public int InstallmentNumber { get; set; }

        /// <summary>
        /// Gets or sets the due date of the installment.
        /// </summary>
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Gets or sets the principal component of the payment.
        /// </summary>
        public decimal PrincipalPortion { get; set; }

        /// <summary>
        /// Gets or sets the interest component of the payment.
        /// </summary>
        public decimal InterestPortion { get; set; }

        /// <summary>
        /// Gets or sets the remaining balance after this payment.
        /// </summary>
        public decimal RemainingBalance { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this row is the current installment.
        /// </summary>
        [NotMapped]
        public bool IsCurrent { get; set; }
    }
}
