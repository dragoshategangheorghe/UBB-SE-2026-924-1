// <copyright file="WithdrawResponseDto.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

namespace BankApp.Models.DTOs.Savings;

using System;

/// <summary>
/// Response payload returned after a withdrawal request.
/// </summary>
public class WithdrawResponseDto
{
    /// <summary>Gets or sets a value indicating whether the withdrawal succeeded.</summary>
    public bool Success { get; set; }

    /// <summary>Gets or sets the amount withdrawn.</summary>
    public decimal AmountWithdrawn { get; set; }

    /// <summary>Gets or sets the penalty amount that was applied.</summary>
    public decimal PenaltyApplied { get; set; }

    /// <summary>Gets or sets the resulting balance after processing.</summary>
    public decimal NewBalance { get; set; }

    /// <summary>Gets or sets a user-facing outcome message.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Gets or sets when the withdrawal was processed.</summary>
    public DateTime ProcessedAt { get; set; }
}