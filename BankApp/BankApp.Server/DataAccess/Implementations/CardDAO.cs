using BankApp.Models.Entities;
using BankApp.Server.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankApp.Server.DataAccess.Implementations
{
    public class CardDAO : ICardDAO
    {
        private readonly AppDbContext _dbContext;

        public CardDAO(AppDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        /// <summary>
        /// Loads a card with its related account and user navigation properties.
        /// </summary>
        public Card? FindById(int cardId)
        {
            return _dbContext.Cards
                .Include(card => card.Account)
                .Include(card => card.User)
                .FirstOrDefault(card => card.Id == cardId);
        }

        /// <summary>
        /// Loads the user's cards through the User navigation property.
        /// </summary>
        public List<Card> FindByUserId(int userId)
        {
            return _dbContext.Cards
                .Include(card => card.Account)
                .Include(card => card.User)
                .Where(card => card.User.Id == userId)
                .OrderBy(card => card.SortOrder)
                .ThenBy(card => card.CreatedAt)
                .ToList();
        }

        public bool UpdateStatus(int cardId, string newStatus)
        {
            var rowsAffected = _dbContext.Cards
                .Where(card => card.Id == cardId)
                .ExecuteUpdate(setters => setters.SetProperty(card => card.Status, newStatus));
            return rowsAffected > 0;
        }

        public bool UpdateSettings(int cardId, decimal? spendingLimit, bool isOnlinePaymentsEnabled, bool isContactlessPaymentsEnabled)
        {
            var rowsAffected = _dbContext.Cards
                .Where(card => card.Id == cardId)
                .ExecuteUpdate(setters => setters
                    .SetProperty(card => card.MonthlySpendingCap, spendingLimit)
                    .SetProperty(card => card.IsOnlineEnabled, isOnlinePaymentsEnabled)
                    .SetProperty(card => card.IsContactlessEnabled, isContactlessPaymentsEnabled));

            return rowsAffected > 0;
        }
    }
}
