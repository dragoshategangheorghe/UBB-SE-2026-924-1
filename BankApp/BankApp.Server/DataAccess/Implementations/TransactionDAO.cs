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
                .Include(transaction => transaction.Account)
                .Include(transaction => transaction.Card)
                .Include(transaction => transaction.Category)
                .Where(transaction => transaction.Account.Id == accountId)
                .OrderByDescending(transaction => transaction.CreatedAt)
                .ThenByDescending(transaction => transaction.Id)
                .Take(limit)
                .ToList();
        }

        public List<Transaction> FindRecentByUserId(int userId, int limit = 10)
        {
            return _dbContext.Transactions
                .Include(transaction => transaction.Account)
                .Include(transaction => transaction.Card)
                .Include(transaction => transaction.Category)
                .Where(transaction => transaction.Account.UserId == userId)
                .OrderByDescending(transaction => transaction.CreatedAt)
                .ThenByDescending(transaction => transaction.Id)
                .Take(limit)
                .ToList();
        }

        public List<TransactionHistoryItemDto> FindByUserId(int userId)
        {
            return _dbContext.Transactions
                .Include(transaction => transaction.Account)
                .Include(transaction => transaction.Card)
                .Include(transaction => transaction.Category)
                .Where(transaction => transaction.Account.UserId == userId)
                .OrderByDescending(transaction => transaction.CreatedAt)
                .ThenByDescending(transaction => transaction.Id)
                .Select(transaction => new TransactionHistoryItemDto
                {
                    Id = transaction.Id,
                    AccountId = transaction.Account.Id,
                    CardId = transaction.Card != null ? transaction.Card.Id : null,
                    AccountName = transaction.Account.AccountName,
                    AccountIban = transaction.Account.IBAN,
                    CardLabel = transaction.Card != null && !string.IsNullOrWhiteSpace(transaction.Card.CardNumber) && transaction.Card.CardNumber.Length >= 4
                        ? transaction.Card.CardNumber
                        : null,
                    Timestamp = transaction.CreatedAt,
                    TransactionType = transaction.Type,
                    ReferenceNumber = transaction.TransactionRef,
                    Description = transaction.Description,
                    CounterpartyOrMerchant = transaction.MerchantName ?? transaction.CounterpartyName ?? string.Empty,
                    MerchantName = transaction.MerchantName,
                    CounterpartyName = transaction.CounterpartyName,
                    SourceAccountIban = transaction.Account.IBAN,
                    DestinationAccountIban = transaction.CounterpartyIBAN,
                    Amount = transaction.Amount,
                    Currency = transaction.Currency,
                    Direction = transaction.Direction,
                    RunningBalanceAfterTransaction = transaction.BalanceAfter,
                    Status = transaction.Status,
                    Fee = transaction.Fee,
                    ExchangeRate = transaction.ExchangeRate,
                    CategoryName = transaction.Category != null && !string.IsNullOrEmpty(transaction.Category.Name) ? transaction.Category.Name : "Uncategorized"
                })
                .ToList();
        }

        public TransactionHistoryItemDto? FindById(int userId, int transactionId)
        {
            var row = _dbContext.Transactions
                .Include(transaction => transaction.Account)
                .Include(transaction => transaction.Card)
                .Include(transaction => transaction.Category)
                .Where(transaction => transaction.Account.User.Id == userId && transaction.Id == transactionId)
                .Select(transaction => new TransactionHistoryItemDto
                {
                    Id = transaction.Id,
                    AccountId = transaction.Account.Id,
                    CardId = transaction.Card != null ? transaction.Card.Id : null,
                    AccountName = transaction.Account.AccountName,
                    AccountIban = transaction.Account.IBAN,
                    CardLabel = transaction.Card != null && !string.IsNullOrWhiteSpace(transaction.Card.CardNumber) && transaction.Card.CardNumber.Length >= 4
                        ? $"**** {transaction.Card.CardNumber.Substring(transaction.Card.CardNumber.Length - 4)}"
                        : null,
                    Timestamp = transaction.CreatedAt,
                    TransactionType = transaction.Type,
                    ReferenceNumber = transaction.TransactionRef,
                    Description = transaction.Description,
                    CounterpartyOrMerchant = transaction.MerchantName ?? transaction.CounterpartyName ?? string.Empty,
                    MerchantName = transaction.MerchantName,
                    CounterpartyName = transaction.CounterpartyName,
                    SourceAccountIban = transaction.Account.IBAN,
                    DestinationAccountIban = transaction.CounterpartyIBAN,
                    Amount = transaction.Amount,
                    Currency = transaction.Currency,
                    Direction = transaction.Direction,
                    RunningBalanceAfterTransaction = transaction.BalanceAfter,
                    Status = transaction.Status,
                    Fee = transaction.Fee,
                    ExchangeRate = transaction.ExchangeRate,
                    CategoryName = transaction.Category != null && !string.IsNullOrEmpty(transaction.Category.Name) ? transaction.Category.Name : "Uncategorized"
                })
                .FirstOrDefault();

            return row;
        }
    }
}
