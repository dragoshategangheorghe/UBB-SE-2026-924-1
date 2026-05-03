// <copyright file="LoanApplicationStatus.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

/// <summary>
/// Represents the review status of a loan application.
/// </summary>
namespace BankApp.Models.Enums;
public enum LoanApplicationStatus
{
    /// <summary>The application is awaiting review.</summary>
    Pending,

    /// <summary>The application has been approved.</summary>
    Approved,

    /// <summary>The application has been rejected.</summary>
    Rejected,
}