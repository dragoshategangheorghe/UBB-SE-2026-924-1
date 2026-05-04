// <copyright file="DepositFrequency.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

namespace BankApp.Models.Enums;

/// <summary>
/// Defines recurrence intervals for scheduled deposits.
/// </summary>
public enum DepositFrequency
{
    /// <summary>Execute every day.</summary>
    Daily,

    /// <summary>Execute once per week.</summary>
    Weekly,

    /// <summary>Execute once per month.</summary>
    Monthly,
}