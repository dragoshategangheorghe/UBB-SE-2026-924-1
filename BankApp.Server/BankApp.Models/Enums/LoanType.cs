// <copyright file="LoanType.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

/// <summary>
/// Defines supported loan product categories.
/// </summary>
namespace BankApp.Models.Enums;
public enum LoanType
{
    /// <summary>Unsecured personal loan.</summary>
    Personal,

    /// <summary>Vehicle financing loan.</summary>
    Auto,

    /// <summary>Real-estate mortgage loan.</summary>
    Mortgage,

    /// <summary>Education-focused student loan.</summary>
    Student,
}