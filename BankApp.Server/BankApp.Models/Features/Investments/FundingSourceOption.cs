// <copyright file="FundingSourceOption.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

namespace BankApp.Models.Features.Investments;

/// <summary>
/// Represents a selectable funding source for savings operations.
/// </summary>
public class FundingSourceOption
{
    /// <summary>
    /// Gets or sets the option identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the display label shown to users.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
}