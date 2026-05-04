// <copyright file="InvestmentHolding.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

namespace BankApp.Models.Features.Investments;

using System.ComponentModel;
using System.Runtime.CompilerServices;

/// <summary>
/// Represents an individual asset position within a portfolio.
/// </summary>
public class InvestmentHolding : INotifyPropertyChanged
{
    private decimal currentPrice;
    private decimal unrealizedGainLoss;

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets or sets the holding identifier.
    /// </summary>
    public int IdentificationNumber { get; set; }

    /// <summary>
    /// Gets or sets the parent portfolio identifier.
    /// </summary>
    public int PortfolioIdentificationNumber { get; set; }

    /// <summary>
    /// Gets or sets the market ticker symbol.
    /// </summary>
    public string Ticker { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the asset class/type.
    /// </summary>
    public string AssetType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the held units or shares.
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// Gets or sets the average entry price.
    /// </summary>
    public decimal AveragePurchasePrice { get; set; }

    /// <summary>
    /// Gets or sets the latest market price.
    /// </summary>
    public decimal CurrentPrice
    {
        get => this.currentPrice;
        set
        {
            this.currentPrice = value;
            this.OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the unrealized gain or loss amount.
    /// </summary>
    public decimal UnrealizedGainLoss
    {
        get => this.unrealizedGainLoss;
        set
        {
            this.unrealizedGainLoss = value;
            this.OnPropertyChanged();
        }
    }

    /// <summary>
    /// Raises property changed notifications for bound UI.
    /// </summary>
    /// <param name="propertyName">The property that changed.</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}