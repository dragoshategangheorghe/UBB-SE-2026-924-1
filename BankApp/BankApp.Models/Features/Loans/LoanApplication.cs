// <copyright file="LoanApplication.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

/// <summary>
/// Represents a user's request for a new loan product.
/// </summary>

using BankApp.Models.Enums;

namespace BankApp.Models.Features.Loans
{
    public class LoanApplication
    {
        /// <summary>
        /// Gets or sets the unique application identifier.
        /// </summary>
        public int IdentificationNumber { get; set; }

        /// <summary>
        /// Gets or sets the applicant user identifier.
        /// </summary>
        public int UserIdentificationNumber { get; set; }

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