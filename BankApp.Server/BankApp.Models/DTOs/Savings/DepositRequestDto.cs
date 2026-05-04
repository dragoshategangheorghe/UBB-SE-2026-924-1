// <copyright file="DepositRequestDto.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

namespace BankApp.Models.DTOs.Savings;

/// <summary>
/// Request payload for depositing funds into a savings account.
/// </summary>
public class DepositRequestDto
{
    /// <summary>Gets or sets the target account identifier.</summary>
    public int AccountId { get; set; }

    /// <summary>Gets or sets the deposit amount.</summary>
    public decimal Amount { get; set; }

    /// <summary>Gets or sets the deposit source description.</summary>
    public string Source { get; set; } = string.Empty;
}