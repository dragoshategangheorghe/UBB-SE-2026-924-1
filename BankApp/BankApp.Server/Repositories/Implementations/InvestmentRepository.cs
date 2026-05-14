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
        private readonly AppDbContext _db;

        public InvestmentRepository(AppDbContext db)
        {
            this._db = db;
        }

        public Portfolio GetPortfolio(int userId)
        {
            // We search for the portfolio
            var portfolio = this._db.Portfolios
                .Include(p => p.Holdings)
                .FirstOrDefault(p => p.UserId == userId);

            // If it doesn't exist, we create AND SAVE it immediately.
            if (portfolio == null)
            {
                portfolio = new Portfolio { UserId = userId };
                this._db.Portfolios.Add(portfolio);
                this._db.SaveChanges();
            }

            return portfolio;
        }

        public async Task<List<InvestmentTransaction>> GetInvestmentLogsAsync(int portfolioId, DateTime? startDate, DateTime? endDate, string? ticker)
        {
            // Use _db.InvestmentTransactions explicitly to ensure the type is known
            var query = this._db.InvestmentTransactions
                .AsNoTracking()
                .Include(x => x.Holding)
                .Where(x => x.Holding.PortfolioId == portfolioId);

            if (startDate.HasValue)
            {
                query = query.Where(x => x.ExecutedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(x => x.ExecutedAt <= endDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(ticker))
            {
                query = query.Where(x => x.Ticker == ticker);
            }

            return await query.OrderByDescending(x => x.ExecutedAt).ToListAsync();
        }

        public async Task RecordCryptoTradeAsync(int portfolioId, string ticker, string actionType, decimal quantity,
            decimal pricePerUnit, decimal fees, decimal finalQuantity, decimal finalAveragePrice)
        {
            await using var transaction = await this._db.Database.BeginTransactionAsync();
            try
            {
                var holding = await this._db.InvestmentHoldings
                    .FirstOrDefaultAsync(h => h.PortfolioId == portfolioId && h.Ticker == ticker);

                if (holding != null)
                {
                    holding.Quantity = finalQuantity;
                    holding.AvgPurchasePrice = finalAveragePrice;
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
                        AvgPurchasePrice = finalAveragePrice,
                        CurrentPrice = pricePerUnit
                    };
                    this._db.InvestmentHoldings.Add(holding);
                    await this._db.SaveChangesAsync();
                }

                this._db.InvestmentTransactions.Add(new InvestmentTransaction
                {
                    HoldingId = holding.Id,
                    Ticker = ticker,
                    ActionType = actionType.ToUpperInvariant(),
                    Quantity = quantity,
                    PricePerUnit = pricePerUnit,
                    Fees = fees,
                    ExecutedAt = DateTime.UtcNow
                });

                await this._db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}