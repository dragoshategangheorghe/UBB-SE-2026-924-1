using BankApp.Models.Entities;
using BankApp.Server.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankApp.Server.DataAccess.Implementations
{
    public class CardDAO : ICardDAO
    {
        private readonly AppDbContext dbContext;

        public CardDAO(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Loads a card with its related account and user navigation properties.
        /// </summary>
        public Card? FindById(int id)
        {
            return dbContext.Cards
                .Include(c => c.Account)
                .Include(c => c.User)
                .FirstOrDefault(c => c.Id == id);
        }

        /// <summary>
        /// Loads the user's cards through the User navigation property.
        /// </summary>
        public List<Card> FindByUserId(int userId)
        {
            return dbContext.Cards
                .Include(c => c.Account)
                .Include(c => c.User)
                .Where(c => c.User.Id == userId)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.CreatedAt)
                .ToList();
        }

        public bool UpdateStatus(int cardId, string status)
        {
            var rowsAffected = dbContext.Cards
                .Where(c => c.Id == cardId)
                .ExecuteUpdate(s => s.SetProperty(c => c.Status, status));

            return rowsAffected > 0;
        }

        public bool UpdateSettings(int cardId, decimal? spendingLimit, bool isOnlinePaymentsEnabled, bool isContactlessPaymentsEnabled)
        {
            var rowsAffected = dbContext.Cards
                .Where(c => c.Id == cardId)
                .ExecuteUpdate(s => s
                    .SetProperty(c => c.MonthlySpendingCap, spendingLimit)
                    .SetProperty(c => c.IsOnlineEnabled, isOnlinePaymentsEnabled)
                    .SetProperty(c => c.IsContactlessEnabled, isContactlessPaymentsEnabled));

            return rowsAffected > 0;
        }

        public Card Insert(Card card)
        {
            if (card == null) throw new ArgumentNullException(nameof(card));
            if (card.CreatedAt == default) card.CreatedAt = DateTime.UtcNow;
            var entry = dbContext.Cards.Add(card);
            dbContext.SaveChanges();
            return entry.Entity;
        }
    }
}
