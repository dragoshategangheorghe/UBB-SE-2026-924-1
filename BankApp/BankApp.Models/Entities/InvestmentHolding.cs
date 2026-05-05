namespace BankApp.Models.Entities;

public class InvestmentHolding
{
    public int IdentificationNumber { get; set; }
    public int PortfolioIdentificationNumber { get; set; }

    // Navigation property
    public Portfolio Portfolio { get; set; } = null!;

    public string Ticker { get; set; } = string.Empty;
    public string AssetType { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal AveragePurchasePrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal UnrealizedGainLoss { get; set; }
}