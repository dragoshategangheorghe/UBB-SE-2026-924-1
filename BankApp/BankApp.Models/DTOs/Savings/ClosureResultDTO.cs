// <copyright file="ClosureResultDTO.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

namespace BankApp.Models.DTOs.Savings;

using System;

/// <summary>
/// Response payload for closing a savings account.
/// </summary>
public class ClosureResultDto
{
    /// <summary>Gets or sets a value indicating whether closure succeeded.</summary>
    public bool Success { get; set; }

    /// <summary>Gets or sets the amount transferred out on closure.</summary>
    public decimal TransferredAmount { get; set; }

    /// <summary>Gets or sets the penalty applied during closure.</summary>
    public decimal PenaltyApplied { get; set; }

    /// <summary>Gets or sets a user-facing closure message.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Gets or sets the closure processing timestamp.</summary>
    public DateTime ClosedAt { get; set; }
}