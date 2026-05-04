// <copyright file="Portfolio.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

namespace BankApp.Models.Features.Investments;

using System.Collections.Generic;

/// <summary>
/// Represents an aggregated view of a user's investment holdings.
/// </summary>
public class Portfolio
{
    /// <summary>
    /// Gets or sets the portfolio identifier.
    /// </summary>
    public int IdentificationNumber { get; set; }

    /// <summary>
    /// Gets or sets the owning user identifier.
    /// </summary>
    public int UserIdentificationNumber { get; set; }

    /// <summary>
    /// Gets or sets the total market value of all holdings.
    /// </summary>
    public decimal TotalValue { get; set; }

    /// <summary>
    /// Gets or sets the absolute gain or loss amount.
    /// </summary>
    public decimal TotalGainLoss { get; set; }

    /// <summary>
    /// Gets or sets the gain or loss percentage.
    /// </summary>
    public decimal GainLossPercent { get; set; }

    /// <summary>
    /// Gets or sets the collection of constituent holdings.
    /// </summary>
    public List<InvestmentHolding> Holdings { get; set; } = new ();
}