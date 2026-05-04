// <copyright file="CreateSavingsAccountDto.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

namespace BankApp.Models.DTOs.Savings;

using System;
using BankApp.Models.Enums;

/// <summary>
/// Request payload for creating a new savings account.
/// </summary>
public class CreateSavingsAccountDto
{
    /// <summary>Gets or sets the user identifier.</summary>
    public int UserIdentificationNumber { get; set; }

    /// <summary>Gets or sets the selected savings type.</summary>
    public string SavingsType { get; set; } = string.Empty;

    /// <summary>Gets or sets a user-defined account name.</summary>
    public string AccountName { get; set; } = string.Empty;

    /// <summary>Gets or sets the opening deposit amount.</summary>
    public decimal InitialDeposit { get; set; }

    /// <summary>Gets or sets the source account identifier.</summary>
    public int FundingAccountId { get; set; }

    /// <summary>Gets or sets an optional goal amount.</summary>
    public decimal? TargetAmount { get; set; }

    /// <summary>Gets or sets an optional goal completion date.</summary>
    public DateTime? TargetDate { get; set; }

    /// <summary>Gets or sets an optional maturity date.</summary>
    public DateTime? MaturityDate { get; set; }

    /// <summary>Gets or sets an optional recurring deposit frequency.</summary>
    public DepositFrequency? DepositFrequency { get; set; }
}