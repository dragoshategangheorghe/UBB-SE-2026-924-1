// <copyright file="LoanApplicationRequest.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

/// <summary>
/// Request payload used to submit a new loan application.
/// </summary>
namespace BankApp.Models.DTOs.Loans;
using BankApp.Models.Enums;

public class LoanApplicationRequest
{
    /// <summary>
    /// Gets or sets the request identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the applicant user identifier.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the requested loan type.
    /// </summary>
    public LoanType LoanType { get; set; }

    /// <summary>
    /// Gets or sets the requested principal amount.
    /// </summary>
    public decimal DesiredAmount { get; set; }

    /// <summary>
    /// Gets or sets the preferred repayment term in months.
    /// </summary>
    public int PreferredTermMonths { get; set; }

    /// <summary>
    /// Gets or sets the reason for applying.
    /// </summary>
    public required string Purpose { get; set; }
}