using BankApp.Models.DTOs.Transactions;
using BankApp.Models.Entities;
using BankApp.Server.DataAccess.Interfaces;
using BankApp.Server.Repositories.Interfaces;

namespace BankApp.Server.Repositories.Implementations
{
    public class TransactionHistoryRepository : ITransactionHistoryRepository
    {
        private readonly ITransactionDAO _transactionDao;
        private readonly IAccountDAO _accountDao;
        private readonly ICardDAO _cardDao;

        public TransactionHistoryRepository(ITransactionDAO transactionDao, IAccountDAO accountDao, ICardDAO cardDao)
        {
            this._transactionDao = transactionDao;
            this._accountDao = accountDao;
            this._cardDao = cardDao;
        }

        public List<TransactionHistoryItemDto> GetTransactionsByUserId(int userId)
        {
            return _transactionDao.FindByUserId(userId);
        }

        public TransactionHistoryItemDto? GetTransactionById(int userId, int transactionId)
        {
            return _transactionDao.FindById(userId, transactionId);
        }

        public List<Account> GetAccountsByUserId(int userId)
        {
            return _accountDao.FindByUserId(userId);
        }

        public List<Card> GetCardsByUserId(int userId)
        {
            return _cardDao.FindByUserId(userId);
        }
    }
}
