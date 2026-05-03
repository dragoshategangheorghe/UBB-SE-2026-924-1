using BankApp.Models.Entities;

namespace BankApp.Server.DataAccess.Interfaces
{
    public interface ICardDAO
    {
        List<Card> FindByUserId(int userId);
        Card? FindById(int id);
        bool UpdateStatus(int cardId, string status);
        bool UpdateSettings(int cardId, decimal? spendingLimit, bool isOnlinePaymentsEnabled, bool isContactlessPaymentsEnabled);
    }
}
