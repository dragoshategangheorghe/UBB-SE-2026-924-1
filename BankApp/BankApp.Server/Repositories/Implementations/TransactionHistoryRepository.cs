using BankApp.Models.DTOs.Transactions;
using BankApp.Models.Entities;
using BankApp.Server.DataAccess.Interfaces;
using BankApp.Server.Repositories.Interfaces;

namespace BankApp.Server.Repositories.Implementations
{
    public class TransactionHistoryRepository : ITransactionHistoryRepository
    {
        private readonly ITransactionDAO transactionDao;
        private readonly IAccountDAO accountDao;
        private readonly ICardDAO cardDao;

        public TransactionHistoryRepository(ITransactionDAO transactionDao, IAccountDAO accountDao, ICardDAO cardDao)
        {
            this.transactionDao = transactionDao;
            this.accountDao = accountDao;
            this.cardDao = cardDao;
        }

        public List<TransactionHistoryItemDto> GetTransactionsByUserId(int userId)
        {
            return transactionDao.FindByUserId(userId);
        }

        public TransactionHistoryItemDto? GetTransactionById(int userId, int transactionId)
        {
            return transactionDao.FindById(userId, transactionId);
        }

        public List<Account> GetAccountsByUserId(int userId)
        {
            return accountDao.FindByUserId(userId);
        }

        public List<Card> GetCardsByUserId(int userId)
        {
            return cardDao.FindByUserId(userId);
        }
    }
}
