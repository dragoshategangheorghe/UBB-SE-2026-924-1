namespace BankApp.Models.Entities;

using System;

public class InvestmentTransaction
{
    public int IdentificationNumber { get; set; }

    public int InvestmentHoldingIdentificationNumber { get; set; }

    // Relationships must be modeled through classes
    public virtual InvestmentHolding InvestmentHolding { get; set; } = null!;

    public string Ticker { get; set; } = string.Empty;

    public string ActionType { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    public decimal PricePerUnit { get; set; }

    public decimal Fees { get; set; }

    public string OrderType { get; set; } = string.Empty;

    public DateTime ExecutedAt { get; set; }
}