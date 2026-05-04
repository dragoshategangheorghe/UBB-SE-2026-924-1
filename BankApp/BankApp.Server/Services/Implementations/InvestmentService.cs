namespace BankApp.Server.Services.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using BankApp.Models.Entities; // Use merged entities
    using BankApp.Server.Repositories.Interfaces;
    using BankApp.Server.Services.Interfaces;

    public class InvestmentService : IInvestmentService
    {
        private readonly IInvestmentRepository investmentRepository;

        // Business rules for commissions
        private const decimal CryptoTradeFeePercentage = 0.015m; // 1.5% commission
        private const decimal MinimumTradeFee = 0.50m; // Minimum commission of $0.50
        private const decimal ZeroAmount = 0m;
        private const int MinimumPortfolioIdExclusive = 0;
        private const int CurrencyRoundingDigits = 2;
        private const string ActionBuy = "BUY";
        private const string ActionSell = "SELL";

        public InvestmentService(IInvestmentRepository investmentRepository)
        {
            this.investmentRepository = investmentRepository;
        }

        public async Task<bool> ExecuteCryptoTradeAsync(
            int portfolioIdentificationNumber,
            string ticker,
            string actionType,
            decimal quantity,
            decimal pricePerUnit)
        {
            // Validate input first.
            this.ValidateTradeInputs(ticker, quantity, pricePerUnit, actionType);

            // Compute trade commission.
            decimal tradeValueAmount = quantity * pricePerUnit;
            decimal calculatedFee = Math.Round(tradeValueAmount * CryptoTradeFeePercentage, CurrencyRoundingDigits);

            if (calculatedFee < MinimumTradeFee)
            {
                calculatedFee = MinimumTradeFee;
            }

            // Load portfolio state used by trade rules.
            Portfolio portfolio = this.investmentRepository.GetPortfolio(portfolioIdentificationNumber);
            InvestmentHolding? currentHolding = portfolio.Holdings.FirstOrDefault(holding =>
                holding.Ticker.Equals(ticker, StringComparison.OrdinalIgnoreCase));

            decimal currentQuantity = currentHolding?.Quantity ?? ZeroAmount;
            decimal currentAveragePrice = currentHolding?.AveragePurchasePrice ?? ZeroAmount;

            decimal finalQuantity;
            decimal finalAveragePrice;

            // Apply business logic based on action type.
            if (actionType.Equals(ActionBuy, StringComparison.OrdinalIgnoreCase))
            {
                decimal totalCostIncludingFee = tradeValueAmount + calculatedFee;

                if (portfolio.TotalValue < totalCostIncludingFee)
                {
                    throw new ArgumentException("Insufficient portfolio balance for this trade.");
                }

                // Weighted Average Price Logic
                decimal totalInvestmentCost = (currentQuantity * currentAveragePrice) + tradeValueAmount;
                finalQuantity = currentQuantity + quantity;
                finalAveragePrice = totalInvestmentCost / finalQuantity;
            }
            else
            {
                // Sell Logic Validation
                if (currentHolding == null || currentQuantity < quantity)
                {
                    throw new InvalidOperationException("Insufficient asset quantity to execute this sell order.");
                }

                finalQuantity = currentQuantity - quantity;
                finalAveragePrice = currentAveragePrice; // Purchase price remains unchanged when selling
            }

            // Persist computed values.
            try
            {
                await this.investmentRepository.RecordCryptoTradeAsync(
                    portfolioIdentificationNumber,
                    ticker,
                    actionType,
                    quantity,
                    pricePerUnit,
                    calculatedFee,
                    finalQuantity,
                    finalAveragePrice);

                return true;
            }
            catch (Exception exception)
            {
                throw new Exception($"Trade execution failed: {exception.Message}", exception);
            }
        }

        public Portfolio GetPortfolio(int userIdentificationNumber)
        {
            return this.investmentRepository.GetPortfolio(userIdentificationNumber);
        }

        public async Task<List<InvestmentTransaction>> GetInvestmentLogsAsync(
            int portfolioIdentificationNumber,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? ticker = null)
        {
            if (portfolioIdentificationNumber <= MinimumPortfolioIdExclusive)
            {
                throw new ArgumentException("Invalid portfolio identification number.", nameof(portfolioIdentificationNumber));
            }

            if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
            {
                throw new ArgumentException("Start date cannot be after the end date.");
            }

            return await this.investmentRepository.GetInvestmentLogsAsync(portfolioIdentificationNumber, startDate, endDate, ticker);
        }

        private void ValidateTradeInputs(string ticker, decimal quantity, decimal price, string action)
        {
            if (string.IsNullOrWhiteSpace(ticker))
            {
                throw new ArgumentException("Ticker symbol cannot be empty.", nameof(ticker));
            }

            if (quantity <= ZeroAmount)
            {
                throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
            }

            if (price <= ZeroAmount)
            {
                throw new ArgumentException("Price per unit must be greater than zero.", nameof(price));
            }

            if (!action.Equals(ActionBuy, StringComparison.OrdinalIgnoreCase) &&
                !action.Equals(ActionSell, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Action type must be either 'BUY' or 'SELL'.", nameof(action));
            }
        }
    }
}