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
        private readonly AppDbContext db;

        public InvestmentRepository(AppDbContext db) => this.db = db;

        public Portfolio GetPortfolio(int userId)
        {
            return this.db.Set<Portfolio>()
                .AsNoTracking()
                .Include(p => p.Holdings)
                .FirstOrDefault(p => p.UserId == userId)
                ?? new Portfolio { UserId = userId };
        }

        public async Task<List<InvestmentTransaction>> GetInvestmentLogsAsync(int portfolioId, DateTime? startDate, DateTime? endDate, string? ticker)
        {
            var query = this.db.Set<InvestmentTransaction>()
                .AsNoTracking()
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
            await using var transaction = await this.db.Database.BeginTransactionAsync();
            try
            {
                var holding = await this.db.Set<InvestmentHolding>()
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
                    this.db.Set<InvestmentHolding>().Add(holding);
                    await this.db.SaveChangesAsync();
                }

                this.db.Set<InvestmentTransaction>().Add(new InvestmentTransaction
                {
                    HoldingId = holding.IdentificationNumber,
                    Ticker = ticker,
                    ActionType = actionType.ToUpperInvariant(),
                    Quantity = quantity,
                    PricePerUnit = pricePerUnit,
                    Fees = fees,
                    ExecutedAt = DateTime.UtcNow
                });

                await this.db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}