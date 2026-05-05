namespace BankApp.Server.Repositories.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BankApp.Models.Features.Investments;
    using BankApp.Server.DataAccess; // Reference to team's DbContext
    using BankApp.Server.Repositories.Interfaces;

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
            var userPortfolio = new Portfolio { UserIdentificationNumber = userId };

            // Using team's ExecuteQuery which uses @p0, @p1 parameters
            string portfolioSql = "SELECT id, totalValue, totalGainLoss, gainLossPercent FROM Portfolio WHERE userId = @p0";
            using (var reader = _db.ExecuteQuery(portfolioSql, new object[] { userId }))
            {
                if (reader.Read())
                {
                    userPortfolio.IdentificationNumber = reader.GetInt32(0);
                    userPortfolio.TotalValue = reader.GetDecimal(1);
                    userPortfolio.TotalGainLoss = reader.GetDecimal(2);
                    userPortfolio.GainLossPercent = reader.GetDecimal(3);
                }
                else return userPortfolio;
            }

            string holdingsSql = "SELECT id, ticker, assetType, quantity, avgPurchasePrice, currentPrice, unrealizedGainLoss FROM InvestmentHolding WHERE portfolioId = @p0";
            using (var reader = _db.ExecuteQuery(holdingsSql, new object[] { userPortfolio.IdentificationNumber }))
            {
                while (reader.Read())
                {
                    userPortfolio.Holdings.Add(new InvestmentHolding
                    {
                        IdentificationNumber = reader.GetInt32(0),
                        Ticker = reader.GetString(1),
                        AssetType = reader.GetString(2),
                        Quantity = reader.GetDecimal(3),
                        AveragePurchasePrice = reader.GetDecimal(4),
                        CurrentPrice = reader.GetDecimal(5),
                        UnrealizedGainLoss = reader.GetDecimal(6)
                    });
                }
            }

            return userPortfolio;
        }

        public async Task RecordCryptoTradeAsync(int portfolioId, string ticker, string actionType, decimal quantity,
            decimal pricePerUnit, decimal fees, decimal finalQuantity, decimal finalAveragePrice)
        {
            _db.BeginTransaction();
            try
            {
                // 1. Check/Update Holding
                string checkSql = "SELECT id FROM InvestmentHolding WHERE portfolioId = @p0 AND ticker = @p1";
                int? holdingId = null;
                using (var reader = _db.ExecuteQuery(checkSql, new object[] { portfolioId, ticker }))
                {
                    if (reader.Read()) holdingId = reader.GetInt32(0);
                }

                if (holdingId.HasValue)
                {
                    string updateSql = "UPDATE InvestmentHolding SET quantity = @p0, avgPurchasePrice = @p1 WHERE id = @p2";
                    _db.ExecuteNonQuery(updateSql, new object[] { finalQuantity, finalAveragePrice, holdingId.Value });
                }
                else
                {
                    string insertHoldingSql = "INSERT INTO InvestmentHolding (portfolioId, ticker, assetType, quantity, avgPurchasePrice, currentPrice, unrealizedGainLoss) VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6)";
                    _db.ExecuteNonQuery(insertHoldingSql, new object[] { portfolioId, ticker, AssetTypeCrypto, finalQuantity, finalAveragePrice, pricePerUnit, 0m });

                    // Get the new ID (Simplified for merge)
                    using var reader = _db.ExecuteQuery(checkSql, new object[] { portfolioId, ticker });
                    if (reader.Read()) holdingId = reader.GetInt32(0);
                }

                // 2. Insert Transaction
                string logSql = "INSERT INTO InvestmentTransaction (InvestmentHoldingIdentificationNumber, Ticker, ActionType, Quantity, PricePerUnit, Fees, OrderType, ExecutedAt) VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7)";
                _db.ExecuteNonQuery(logSql, new object[] { holdingId!.Value, ticker, actionType.ToUpper(), quantity, pricePerUnit, fees, OrderTypeMarket, DateTime.Now });

                _db.CommitTransaction();
                await Task.CompletedTask;
            }
            catch
            {
                _db.RollbackTransaction();
                throw;
            }
        }

        public async Task<List<InvestmentTransaction>> GetInvestmentLogsAsync(int portfolioId, DateTime? startDate, DateTime? endDate, string? ticker)
        {
            var logs = new List<InvestmentTransaction>();
            string sql = "SELECT t.id, t.InvestmentHoldingIdentificationNumber, t.Ticker, t.ActionType, t.Quantity, t.PricePerUnit, t.Fees, t.OrderType, t.ExecutedAt FROM InvestmentTransaction t INNER JOIN InvestmentHolding h ON t.InvestmentHoldingIdentificationNumber = h.id WHERE h.portfolioId = @p0";

            // Note: For merging, we use the base query. Filtering logic can be added by the next teammate.
            using (var reader = _db.ExecuteQuery(sql, new object[] { portfolioId }))
            {
                while (reader.Read())
                {
                    logs.Add(new InvestmentTransaction
                    {
                        //IdentificationNumber = reader.GetInt32(0),
                        //InvestmentHoldingIdentificationNumber = reader.GetInt32(1),
                        //Ticker = reader.GetString(2),
                        //ActionType = reader.GetString(3),
                        //Quantity = reader.GetDecimal(4),
                        //PricePerUnit = reader.GetDecimal(5),
                        //Fees = reader.GetDecimal(6),
                        //OrderType = reader.GetString(7),
                        //ExecutedAt = reader.GetDateTime(8)
                    });
                }
            }
            return await Task.FromResult(logs);
        }
    }
}