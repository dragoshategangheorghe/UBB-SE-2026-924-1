namespace BankApp.Server.Repositories.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using BankApp.Models.Features.Investments;
    using BankApp.Server.DataAccess;
    using BankApp.Server.Repositories.Interfaces;
    using Microsoft.EntityFrameworkCore;

    public class InvestmentRepository : IInvestmentRepository
    {
        private readonly AppDbContext _db;
        private const string AssetTypeCrypto = "Crypto";
        private const string OrderTypeMarket = "Market";

        public InvestmentRepository(AppDbContext db)
        {
            _db = db;
        }

        public Portfolio GetPortfolio(int userId)
        {
            return _db.Portfolios
                .AsNoTracking()
                .Include(p => p.Holdings)
                .FirstOrDefault(p => EF.Property<int>(p, "UserId") == userId)
                ?? new Portfolio { UserIdentificationNumber = userId };
        }

        public async Task RecordCryptoTradeAsync(int portfolioId, string ticker, string actionType, decimal quantity,
            decimal pricePerUnit, decimal fees, decimal finalQuantity, decimal finalAveragePrice)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var holding = await _db.InvestmentHoldings.FirstOrDefaultAsync(h => EF.Property<int>(h, "PortfolioId") == portfolioId && h.Ticker == ticker);

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
                        Ticker = ticker,
                        AssetType = AssetTypeCrypto,
                        Quantity = finalQuantity,
                        AveragePurchasePrice = finalAveragePrice,
                        CurrentPrice = pricePerUnit,
                        UnrealizedGainLoss = 0m,
                    };

                    _db.InvestmentHoldings.Add(holding);
                    await _db.SaveChangesAsync();
                }

                _db.Set<InvestmentTransaction>().Add(new InvestmentTransaction
                {
                    HoldingIdentificationNumber = holding.IdentificationNumber,
                    Ticker = ticker,
                    ActionType = actionType.ToUpperInvariant(),
                    Quantity = quantity,
                    PricePerUnit = pricePerUnit,
                    Fees = fees,
                    OrderType = OrderTypeMarket,
                    ExecutedAt = DateTime.UtcNow,
                });

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<InvestmentTransaction>> GetInvestmentLogsAsync(int portfolioId, DateTime? startDate, DateTime? endDate, string? ticker)
        {
            var query = _db.Set<InvestmentTransaction>().AsNoTracking()
                .Where(x => EF.Property<int>(x, "PortfolioId") == portfolioId);

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
    }
}