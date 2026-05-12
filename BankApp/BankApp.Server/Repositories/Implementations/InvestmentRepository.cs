namespace BankApp.Server.Repositories.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using BankApp.Models.Entities;
    using BankApp.Server.DataAccess;
    using BankApp.Server.Repositories.Interfaces;
    using Microsoft.EntityFrameworkCore;

    public class InvestmentRepository : IInvestmentRepository
    {
        private readonly AppDbContext _dbContext;

        public InvestmentRepository(AppDbContext dbContext) => this._dbContext = dbContext;

        public Portfolio GetPortfolio(int userId)
        {
            // We search for the portfolio
            var portfolio = this.db.Portfolios
                .Include(p => p.Holdings)
                .FirstOrDefault(p => p.UserId == userId);

            // If it doesn't exist, we create AND SAVE it immediately.
            // This prevents "Foreign Key" crashes when adding holdings later.
            if (portfolio == null)
            {
                portfolio = new Portfolio { UserId = userId };
                this.db.Portfolios.Add(portfolio);
                this.db.SaveChanges();
            }

            return portfolio;
        }

        public async Task<List<InvestmentTransaction>> GetInvestmentLogsAsync(int portfolioId, DateTime? startDate, DateTime? endDate, string? ticker)
        {
            var query = this.db.InvestmentTransactions
                .AsNoTracking()
                .Where(investmentTransaction => investmentTransaction.Holding.PortfolioId == portfolioId);

            if (startDate.HasValue)
            {
                query = query.Where(investmentTransaction => investmentTransaction.ExecutedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(investmentTransaction => investmentTransaction.ExecutedAt <= endDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(ticker))
            {
                query = query.Where(investmentTransaction => investmentTransaction.Ticker == ticker);
            }

            return await query.OrderByDescending(investmentTransaction => investmentTransaction.ExecutedAt).ToListAsync();
        }

        public async Task RecordCryptoTradeAsync(int portfolioId, string ticker, string actionType, decimal quantity,
            decimal pricePerUnit, decimal fees, decimal finalQuantity, decimal finalAveragePrice)
        {
            await using var transaction = await this._dbContext.Database.BeginTransactionAsync();
            try
            {
                var holding = await this.db.InvestmentHoldings
                    .FirstOrDefaultAsync(h => h.PortfolioId == portfolioId && h.Ticker == ticker);

                if (holding != null)
                {
                    holding.Quantity = finalQuantity;
                    holding.AveragePurchasePrice = finalAveragePrice;
                    holding.CurrentPrice = pricePerUnit;
                }
                else
                {
                    holding = new InvestmentHolding
                    {
                        PortfolioId = portfolioId,
                        Ticker = ticker,
                        AssetType = "Crypto",
                        Quantity = finalQuantity,
                        AveragePurchasePrice = finalAveragePrice,
                        CurrentPrice = pricePerUnit
                    };
                    this.db.InvestmentHoldings.Add(holding);
                    await this.db.SaveChangesAsync();
                }

                this.db.InvestmentTransactions.Add(new InvestmentTransaction
                {
                    HoldingId = holding.IdentificationNumber,
                    Ticker = ticker,
                    ActionType = actionType.ToUpperInvariant(),
                    Quantity = quantity,
                    PricePerUnit = pricePerUnit,
                    Fees = fees,
                    ExecutedAt = DateTime.UtcNow
                });

                await this._dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}