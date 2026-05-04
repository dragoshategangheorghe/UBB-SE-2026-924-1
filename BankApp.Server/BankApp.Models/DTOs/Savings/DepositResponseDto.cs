// <copyright file="DepositResponseDto.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

namespace BankApp.Models.DTOs.Savings;

using System;

/// <summary>
/// Response payload returned after a successful deposit.
/// </summary>
public class DepositResponseDto
{
    /// <summary>Gets or sets the resulting account balance.</summary>
    public decimal NewBalance { get; set; }

    /// <summary>Gets or sets the created transaction identifier.</summary>
    public int TransactionId { get; set; }

    /// <summary>Gets or sets when the operation was processed.</summary>
    public DateTime Timestamp { get; set; }
}