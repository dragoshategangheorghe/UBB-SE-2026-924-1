namespace BankApp.Models.Entities;

using System.Collections.Generic;

public class Portfolio
{
    public int IdentificationNumber { get; set; }
    public int UserIdentificationNumber { get; set; }

    // Links must be modeled through classes
    public List<InvestmentHolding> Holdings { get; set; } = new ();

    public decimal TotalValue { get; set; }
    public decimal TotalGainLoss { get; set; }
    public decimal GainLossPercent { get; set; }
}