// <copyright file="LoanStatus.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

/// <summary>
/// Represents the lifecycle state of a loan.
/// </summary>
namespace BankApp.Models.Enums;
public enum LoanStatus
{
    /// <summary>The loan is active and being repaid.</summary>
    Active,

    /// <summary>The loan has passed its planned lifecycle status.</summary>
    Passed,
}