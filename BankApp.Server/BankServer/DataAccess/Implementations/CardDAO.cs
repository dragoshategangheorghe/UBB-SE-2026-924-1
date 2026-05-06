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

        public Card? FindById(int id)
        {
            //{
            //    const string query = @"SELECT * FROM Card WHERE Id = @p0";
            //    using var reader = dbContext.ExecuteQuery(query, new object[] { id });
            //    return reader.Read() ? MapToCard(reader) : null;

            Card? card = dbContext.Cards.FirstOrDefault(c => c.Id == id);
            return card;
        }

        public List<Card> FindByUserId(int userId)
        {
            //List<Card> cards = new ();
            //const string query = @"SELECT * FROM Card WHERE UserId = @p0 ORDER BY SortOrder, CreatedAt";

            //using var reader = dbContext.ExecuteQuery(query, new object[] { userId });
            //while (reader.Read())
            //{
            //    cards.Add(MapToCard(reader));
            //}

            //return cards;

            List<Card> cards = dbContext.Cards.Where(c => c.UserId == userId).OrderBy(c => c.SortOrder).ThenBy(c => c.CreatedAt).ToList();
            return cards;
        }

        public bool UpdateStatus(int cardId, string status)
        {
            //const string query = @"
            //    UPDATE Card
            //    SET Status = @p1
            //    WHERE Id = @p0";

            //return dbContext.ExecuteNonQuery(query, new object[] { cardId, status }) > 0;

            var rowsAffected = dbContext.Cards
                .Where(c => c.Id == cardId)
                .ExecuteUpdate(s => s.SetProperty(c => c.Status, status));

            return rowsAffected > 0;
        }

        public bool UpdateSettings(int cardId, decimal? spendingLimit, bool isOnlinePaymentsEnabled, bool isContactlessPaymentsEnabled)
        {
            //const string query = @"
            //    UPDATE Card
            //    SET MonthlySpendingCap = @p1,
            //        IsOnlineEnabled = @p2,
            //        IsContactlessEnabled = @p3
            //    WHERE Id = @p0";

            //return dbContext.ExecuteNonQuery(query, new object[] { cardId, spendingLimit, isOnlinePaymentsEnabled, isContactlessPaymentsEnabled }) > 0;

            var rowsAffected = dbContext.Cards
                .Where(c => c.Id == cardId)
                .ExecuteUpdate(s => s
                    .SetProperty(c => c.MonthlySpendingCap, spendingLimit)
                    .SetProperty(c => c.IsOnlineEnabled, isOnlinePaymentsEnabled)
                    .SetProperty(c => c.IsContactlessEnabled, isContactlessPaymentsEnabled));

            return rowsAffected > 0;
        }

        //private static Card MapToCard(System.Data.IDataReader reader)
        //{
        //    return new Card
        //    {
        //        Id = reader.GetInt32(reader.GetOrdinal("Id")),
        //        AccountId = reader.GetInt32(reader.GetOrdinal("AccountId")),
        //        UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
        //        CardNumber = reader.GetString(reader.GetOrdinal("CardNumber")),
        //        CardholderName = reader.GetString(reader.GetOrdinal("CardholderName")),
        //        ExpiryDate = reader.GetDateTime(reader.GetOrdinal("ExpiryDate")),
        //        CVV = reader.GetString(reader.GetOrdinal("CVV")),
        //        CardType = reader.GetString(reader.GetOrdinal("CardType")),
        //        CardBrand = reader.IsDBNull(reader.GetOrdinal("CardBrand")) ? null : reader.GetString(reader.GetOrdinal("CardBrand")),
        //        Status = reader.GetString(reader.GetOrdinal("Status")),
        //        DailyTransactionLimit = reader.IsDBNull(reader.GetOrdinal("DailyTransactionLimit")) ? null : reader.GetDecimal(reader.GetOrdinal("DailyTransactionLimit")),
        //        MonthlySpendingCap = reader.IsDBNull(reader.GetOrdinal("MonthlySpendingCap")) ? null : reader.GetDecimal(reader.GetOrdinal("MonthlySpendingCap")),
        //        AtmWithdrawalLimit = reader.IsDBNull(reader.GetOrdinal("AtmWithdrawalLimit")) ? null : reader.GetDecimal(reader.GetOrdinal("AtmWithdrawalLimit")),
        //        ContactlessLimit = reader.IsDBNull(reader.GetOrdinal("ContactlessLimit")) ? null : reader.GetDecimal(reader.GetOrdinal("ContactlessLimit")),
        //        IsContactlessEnabled = reader.GetBoolean(reader.GetOrdinal("IsContactlessEnabled")),
        //        IsOnlineEnabled = reader.GetBoolean(reader.GetOrdinal("IsOnlineEnabled")),
        //        SortOrder = reader.GetInt32(reader.GetOrdinal("SortOrder")),
        //        CancelledAt = reader.IsDBNull(reader.GetOrdinal("CancelledAt")) ? null : reader.GetDateTime(reader.GetOrdinal("CancelledAt")),
        //        CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
        //    };
        //}
    }
}
