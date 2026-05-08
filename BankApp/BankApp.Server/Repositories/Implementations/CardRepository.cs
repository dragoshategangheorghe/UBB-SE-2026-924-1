using BankApp.Models.Entities;
using BankApp.Server.DataAccess.Interfaces;
using BankApp.Server.Repositories.Interfaces;

namespace BankApp.Server.Repositories.Implementations
{
    public class CardRepository : ICardRepository
    {
        private readonly ICardDAO cardDao;
        private readonly IAccountDAO accountDao;
        private readonly IUserCardPreferenceDAO userCardPreferenceDao;

        public CardRepository(ICardDAO cardDao, IAccountDAO accountDao, IUserCardPreferenceDAO userCardPreferenceDao)
        {
            this.cardDao = cardDao;
            this.accountDao = accountDao;
            this.userCardPreferenceDao = userCardPreferenceDao;
        }

        public List<Card> GetCardsByUserId(int userId)
        {
            return cardDao.FindByUserId(userId);
        }

        public Card? GetCardById(int cardId)
        {
            return cardDao.FindById(cardId);
        }

        public Account? GetAccountById(int accountId)
        {
            return accountDao.FindById(accountId);
        }

        public UserCardPreference? GetSortPreference(int userId)
        {
            return userCardPreferenceDao.FindByUserId(userId);
        }

        public bool SaveSortPreference(int userId, string sortOption)
        {
            return userCardPreferenceDao.Upsert(userId, sortOption);
        }

        public bool UpdateStatus(int cardId, string status)
        {
            return cardDao.UpdateStatus(cardId, status);
        }

        public bool UpdateSettings(int cardId, decimal? spendingLimit, bool isOnlinePaymentsEnabled, bool isContactlessPaymentsEnabled)
        {
            return cardDao.UpdateSettings(cardId, spendingLimit, isOnlinePaymentsEnabled, isContactlessPaymentsEnabled);
        }
    }
}
