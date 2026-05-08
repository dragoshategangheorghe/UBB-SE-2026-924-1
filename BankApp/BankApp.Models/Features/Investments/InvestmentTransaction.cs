// <copyright file="InvestmentTransaction.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace BankApp.Models.Features.Investments;

using System;

/// <summary>
/// Represents an executed buy/sell transaction for an investment holding.
/// </summary>
public class InvestmentTransaction
{
    /// <summary>
    /// Gets or sets the transaction identifier.
    /// </summary>
    [Key]
    public int Id { get; set; }

    public int HoldingId { get; set; }

    /// <summary>
    /// Gets or sets the associated holding.
    /// </summary>
    public virtual InvestmentHolding Holding { get; set; } = null!;

    /// <summary>
    /// Gets or sets the traded symbol.
    /// </summary>
    public string Ticker { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the action type (buy/sell).
    /// </summary>
    public string ActionType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the traded quantity.
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// Gets or sets the execution price per unit.
    /// </summary>
    public decimal PricePerUnit { get; set; }

    /// <summary>
    /// Gets or sets any fees applied to the trade.
    /// </summary>
    public decimal Fees { get; set; }

    /// <summary>
    /// Gets or sets the order type used for execution.
    /// </summary>
    public string OrderType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the transaction was executed.
    /// </summary>
    public DateTime ExecutedAt { get; set; }
}