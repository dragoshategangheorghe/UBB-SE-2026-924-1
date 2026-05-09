using BankApp.Models.Entities;
using BankApp.Server.DataAccess.Interfaces;
using BankApp.Server.Repositories.Interfaces;

namespace BankApp.Server.Repositories.Implementations
{
    public class CardRepository : ICardRepository
    {
        private readonly ICardDAO _cardDao;
        private readonly IAccountDAO _accountDao;
        private readonly IUserCardPreferenceDAO _userCardPreferenceDao;

        public CardRepository(ICardDAO cardDao, IAccountDAO accountDao, IUserCardPreferenceDAO userCardPreferenceDao)
        {
            this._cardDao = cardDao;
            this._accountDao = accountDao;
            this._userCardPreferenceDao = userCardPreferenceDao;
        }

        public List<Card> GetCardsByUserId(int userId)
        {
            return _cardDao.FindByUserId(userId);
        }

        public Card? GetCardById(int cardId)
        {
            return _cardDao.FindById(cardId);
        }

        public Account? GetAccountById(int accountId)
        {
            return _accountDao.FindById(accountId);
        }

        public UserCardPreference? GetSortPreference(int userId)
        {
            return _userCardPreferenceDao.FindByUserId(userId);
        }

        public bool SaveSortPreference(int userId, string sortOption)
        {
            return _userCardPreferenceDao.Upsert(userId, sortOption);
        }

        public bool UpdateStatus(int cardId, string status)
        {
            return _cardDao.UpdateStatus(cardId, status);
        }

        public bool UpdateSettings(int cardId, decimal? spendingLimit, bool isOnlinePaymentsEnabled, bool isContactlessPaymentsEnabled)
        {
            return _cardDao.UpdateSettings(cardId, spendingLimit, isOnlinePaymentsEnabled, isContactlessPaymentsEnabled);
        }
    }
}
