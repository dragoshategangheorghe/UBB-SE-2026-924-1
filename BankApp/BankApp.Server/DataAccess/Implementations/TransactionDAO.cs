using BankApp.Models.DTOs.Transactions;
using BankApp.Models.Entities;
using BankApp.Server.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankApp.Server.DataAccess.Implementations
{
    public class TransactionDAO : ITransactionDAO
    {
        private readonly AppDbContext _dbContext;

        public TransactionDAO(AppDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public List<Transaction> FindRecentByAccountId(int accountId, int limit = 10)
        {
            return _dbContext.Transactions
                .Include(t => t.Account)
                .Include(t => t.Card)
                .Include(t => t.Category)
                .Where(t => t.Account.Id == accountId)
                .OrderByDescending(t => t.CreatedAt)
                .ThenByDescending(t => t.Id)
                .Take(limit)
                .ToList();
        }

        public List<TransactionHistoryItemDto> FindByUserId(int userId)
        {
            return _dbContext.Transactions
                .Include(t => t.Account)
                .Include(t => t.Card)
                .Include(t => t.Category)
                .Where(t => t.Account.User.Id == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ThenByDescending(t => t.Id)
                .Select(t => new TransactionHistoryItemDto
                {
                    Id = t.Id,
                    AccountId = t.Account.Id,
                    CardId = t.Card != null ? t.Card.Id : null,
                    AccountName = t.Account.AccountName,
                    AccountIban = t.Account.IBAN,
                    CardLabel = t.Card != null && !string.IsNullOrWhiteSpace(t.Card.CardNumber) && t.Card.CardNumber.Length >= 4
                        ? t.Card.CardNumber
                        : null,
                    Timestamp = t.CreatedAt,
                    TransactionType = t.Type,
                    ReferenceNumber = t.TransactionRef,
                    Description = t.Description,
                    CounterpartyOrMerchant = t.MerchantName ?? t.CounterpartyName ?? string.Empty,
                    MerchantName = t.MerchantName,
                    CounterpartyName = t.CounterpartyName,
                    SourceAccountIban = t.Account.IBAN,
                    DestinationAccountIban = t.CounterpartyIBAN,
                    Amount = t.Amount,
                    Currency = t.Currency,
                    Direction = t.Direction,
                    RunningBalanceAfterTransaction = t.BalanceAfter,
                    Status = t.Status,
                    Fee = t.Fee,
                    ExchangeRate = t.ExchangeRate,
                    CategoryName = t.Category != null && !string.IsNullOrEmpty(t.Category.Name) ? t.Category.Name : "Uncategorized"
                })
                .ToList();
        }

        public TransactionHistoryItemDto? FindById(int userId, int transactionId)
        {
            var row = _dbContext.Transactions
                .Include(t => t.Account)
                .Include(t => t.Card)
                .Include(t => t.Category)
                .Where(t => t.Account.User.Id == userId && t.Id == transactionId)
                .Select(t => new TransactionHistoryItemDto
                {
                    Id = t.Id,
                    AccountId = t.Account.Id,
                    CardId = t.Card != null ? t.Card.Id : null,
                    AccountName = t.Account.AccountName,
                    AccountIban = t.Account.IBAN,
                    CardLabel = t.Card != null && !string.IsNullOrWhiteSpace(t.Card.CardNumber) && t.Card.CardNumber.Length >= 4
                        ? $"**** {t.Card.CardNumber.Substring(t.Card.CardNumber.Length - 4)}"
                        : null,
                    Timestamp = t.CreatedAt,
                    TransactionType = t.Type,
                    ReferenceNumber = t.TransactionRef,
                    Description = t.Description,
                    CounterpartyOrMerchant = t.MerchantName ?? t.CounterpartyName ?? string.Empty,
                    MerchantName = t.MerchantName,
                    CounterpartyName = t.CounterpartyName,
                    SourceAccountIban = t.Account.IBAN,
                    DestinationAccountIban = t.CounterpartyIBAN,
                    Amount = t.Amount,
                    Currency = t.Currency,
                    Direction = t.Direction,
                    RunningBalanceAfterTransaction = t.BalanceAfter,
                    Status = t.Status,
                    Fee = t.Fee,
                    ExchangeRate = t.ExchangeRate,
                    CategoryName = t.Category != null && !string.IsNullOrEmpty(t.Category.Name) ? t.Category.Name : "Uncategorized"
                })
                .FirstOrDefault();

            return row;
        }
    }
}
