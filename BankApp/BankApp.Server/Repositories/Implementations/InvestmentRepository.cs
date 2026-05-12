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
            return this._dbContext.Set<Portfolio>()
                .AsNoTracking()
                .Include(portfolio => portfolio.Holdings)
                .FirstOrDefault(portfolio => portfolio.UserId == userId)
                ?? new Portfolio { UserId = userId };
        }

        public async Task<List<InvestmentTransaction>> GetInvestmentLogsAsync(int portfolioId, DateTime? startDate, DateTime? endDate, string? ticker)
        {
            var query = this._dbContext.Set<InvestmentTransaction>()
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
                var holding = await this._dbContext.Set<InvestmentHolding>()
                    .FirstOrDefaultAsync(investmentHolding => investmentHolding.PortfolioId == portfolioId && investmentHolding.Ticker == ticker);

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
                    this._dbContext.Set<InvestmentHolding>().Add(holding);
                    await this._dbContext.SaveChangesAsync();
                }

                this._dbContext.Set<InvestmentTransaction>().Add(new InvestmentTransaction
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
            catch (Exception exception) when (
            exception is OperationCanceledException
            || exception is DbUpdateException
            || exception is DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}