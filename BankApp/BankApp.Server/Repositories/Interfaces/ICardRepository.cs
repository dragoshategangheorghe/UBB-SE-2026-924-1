using BankApp.Models.Entities;

namespace BankApp.Server.Repositories.Interfaces
{
    public interface ICardRepository
    {
        List<Card> GetCardsByUserId(int userId);
        Card? GetCardById(int cardId);
        Account? GetAccountById(int accountId);
        UserCardPreference? GetSortPreference(int userId);
        bool SaveSortPreference(int userId, string sortOption);
        bool UpdateStatus(int cardId, string status);
        bool UpdateSettings(int cardId, decimal? spendingLimit, bool isOnlinePaymentsEnabled, bool isContactlessPaymentsEnabled);
    }
}
