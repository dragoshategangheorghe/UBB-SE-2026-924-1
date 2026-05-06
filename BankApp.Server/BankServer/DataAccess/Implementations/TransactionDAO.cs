using BankApp.Models.DTOs.Transactions;
using BankApp.Models.Entities;
using BankApp.Server.DataAccess.Interfaces;
using System.Data;

namespace BankApp.Server.DataAccess.Implementations
{
    public class TransactionDAO : ITransactionDAO
    {
        private readonly AppDbContext dbContext;

        public TransactionDAO(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public List<Transaction> FindRecentByAccountId(int accountId, int limit = 10)
        {
            //List<Transaction> transactions = new ();
            //const string query = @"
            //    SELECT TOP (@p1) *
            //    FROM [Transaction]
            //    WHERE AccountId = @p0
            //    ORDER BY CreatedAt DESC";

            //using var reader = dbContext.ExecuteQuery(query, new object[] { accountId, limit });
            //while (reader.Read())
            //{
            //    transactions.Add(MapToTransaction(reader));
            //}

            //return transactions;

            List<Transaction> transactions = dbContext.Transactions
                .Where(t => t.AccountId == accountId)
                .OrderByDescending(t => t.CreatedAt)
                .ThenByDescending(t => t.Id)
                .Take(limit)
                .ToList();

            return transactions;
        }

        public List<TransactionHistoryItemDto> FindByUserId(int userId)
        {
            //List<TransactionHistoryItemDto> transactions = new ();
            //const string query = @"
            //    SELECT
            //        t.Id,
            //        t.AccountId,
            //        t.CardId,
            //        a.AccountName,
            //        a.IBAN AS AccountIban,
            //        c.CardNumber,
            //        t.CreatedAt,
            //        t.[Type],
            //        t.TransactionRef,
            //        t.[Description],
            //        COALESCE(t.MerchantName, t.CounterpartyName, '') AS CounterpartyOrMerchant,
            //        t.MerchantName,
            //        t.CounterpartyName,
            //        a.IBAN AS SourceAccountIban,
            //        t.CounterpartyIBAN AS DestinationAccountIban,
            //        t.Amount,
            //        t.Currency,
            //        t.Direction,
            //        t.BalanceAfter,
            //        t.Status,
            //        t.Fee,
            //        t.ExchangeRate,
            //        COALESCE(cat.Name, 'Uncategorized') AS CategoryName
            //    FROM [Transaction] t
            //    INNER JOIN Account a ON a.Id = t.AccountId
            //    LEFT JOIN Card c ON c.Id = t.CardId
            //    LEFT JOIN Category cat ON cat.Id = t.CategoryId
            //    WHERE a.UserId = @p0
            //    ORDER BY t.CreatedAt DESC, t.Id DESC";

            //using var reader = dbContext.ExecuteQuery(query, new object[] { userId });
            //while (reader.Read())
            //{
            //    transactions.Add(MapToTransactionHistoryItem(reader));
            //}

            //return transactions;

            List<TransactionHistoryItemDto> transactions =
(
    from t in dbContext.Transactions
    join a in dbContext.Accounts on t.AccountId equals a.Id
    join c in dbContext.Cards on t.CardId equals c.Id into cardJoin
    from c in cardJoin.DefaultIfEmpty()
    join cat in dbContext.Categories on t.CategoryId equals cat.Id into catJoin
    from cat in catJoin.DefaultIfEmpty()

    where a.UserId == userId

    orderby t.CreatedAt descending, t.Id descending

    select new TransactionHistoryItemDto
    {
        Id = t.Id,
        AccountId = t.AccountId,
        CardId = t.CardId,

        AccountName = a.AccountName,
        AccountIban = a.IBAN,

        CardLabel =
            c != null && !string.IsNullOrWhiteSpace(c.CardNumber) && c.CardNumber.Length >= 4
                ? c.CardNumber
                : null,

        Timestamp = t.CreatedAt,
        TransactionType = t.Type,
        ReferenceNumber = t.TransactionRef,
        Description = t.Description,

        CounterpartyOrMerchant =
            t.MerchantName ?? t.CounterpartyName ?? string.Empty,

        MerchantName = t.MerchantName,
        CounterpartyName = t.CounterpartyName,

        SourceAccountIban = a.IBAN,
        DestinationAccountIban = t.CounterpartyIBAN,

        Amount = t.Amount,
        Currency = t.Currency,
        Direction = t.Direction,
        RunningBalanceAfterTransaction = t.BalanceAfter,
        Status = t.Status,
        Fee = t.Fee,
        ExchangeRate = t.ExchangeRate,

        CategoryName =
            cat != null && !string.IsNullOrEmpty(cat.Name)
                ? cat.Name
                : "Uncategorized"
    }
).ToList();

            return transactions;
        }

        public TransactionHistoryItemDto? FindById(int userId, int transactionId)
        {
            //const string query = @"
            //    SELECT
            //        t.Id,
            //        t.AccountId,
            //        t.CardId,
            //        a.AccountName,
            //        a.IBAN AS AccountIban,
            //        c.CardNumber,
            //        t.CreatedAt,
            //        t.[Type],
            //        t.TransactionRef,
            //        t.[Description],
            //        COALESCE(t.MerchantName, t.CounterpartyName, '') AS CounterpartyOrMerchant,
            //        t.MerchantName,
            //        t.CounterpartyName,
            //        a.IBAN AS SourceAccountIban,
            //        t.CounterpartyIBAN AS DestinationAccountIban,
            //        t.Amount,
            //        t.Currency,
            //        t.Direction,
            //        t.BalanceAfter,
            //        t.Status,
            //        t.Fee,
            //        t.ExchangeRate,
            //        COALESCE(cat.Name, 'Uncategorized') AS CategoryName
            //    FROM [Transaction] t
            //    INNER JOIN Account a ON a.Id = t.AccountId
            //    LEFT JOIN Card c ON c.Id = t.CardId
            //    LEFT JOIN Category cat ON cat.Id = t.CategoryId
            //    WHERE a.UserId = @p0 AND t.Id = @p1";

            //using var reader = dbContext.ExecuteQuery(query, new object[] { userId, transactionId });
            //return reader.Read() ? MapToTransactionHistoryItem(reader) : null;

            var row = (
                            from tr in dbContext.Transactions
                            join acc in dbContext.Accounts on tr.AccountId equals acc.Id
                            join car in dbContext.Cards on tr.CardId equals car.Id into carJoin
                            from card in carJoin.DefaultIfEmpty()
                            join cat in dbContext.Categories on tr.CategoryId equals cat.Id into catJoin
                            from category in catJoin.DefaultIfEmpty()

                            where acc.UserId == userId && tr.Id == transactionId

                            select new
                            {
                                tr,
                                acc,
                                card,
                                category
                            }
                        ).FirstOrDefault();


            if (row == null)
                return null;

            var trn = row.tr;
            var accn = row.acc;
            var crd = row.card;
            var catg = row.category;

            return new TransactionHistoryItemDto
            {
                Id = trn.Id,
                AccountId = trn.AccountId,
                CardId = trn.CardId,

                AccountName = accn.AccountName,
                AccountIban = accn.IBAN,

                CardLabel =
                    crd != null && !string.IsNullOrWhiteSpace(crd.CardNumber) && crd.CardNumber.Length >= 4
                        ? $"**** {crd.CardNumber.Substring(crd.CardNumber.Length - 4)}"
                        : null,

                Timestamp = trn.CreatedAt,
                TransactionType = trn.Type,
                ReferenceNumber = trn.TransactionRef,
                Description = trn.Description,

                CounterpartyOrMerchant =
                    trn.MerchantName ?? trn.CounterpartyName ?? string.Empty,

                MerchantName = trn.MerchantName,
                CounterpartyName = trn.CounterpartyName,

                SourceAccountIban = accn.IBAN,
                DestinationAccountIban = trn.CounterpartyIBAN,

                Amount = trn.Amount,
                Currency = trn.Currency,
                Direction = trn.Direction,
                RunningBalanceAfterTransaction = trn.BalanceAfter,
                Status = trn.Status,
                Fee = trn.Fee,
                ExchangeRate = trn.ExchangeRate,

                CategoryName =
                    catg != null && !string.IsNullOrEmpty(catg.Name)
                        ? catg.Name
                        : "Uncategorized"
            };
        }

        //private static Transaction MapToTransaction(IDataReader reader)
        //{
        //    return new Transaction
        //    {
        //        Id = reader.GetInt32(reader.GetOrdinal("Id")),
        //        AccountId = reader.GetInt32(reader.GetOrdinal("AccountId")),
        //        CardId = reader.IsDBNull(reader.GetOrdinal("CardId")) ? null : reader.GetInt32(reader.GetOrdinal("CardId")),
        //        TransactionRef = reader.GetString(reader.GetOrdinal("TransactionRef")),
        //        Type = reader.GetString(reader.GetOrdinal("Type")),
        //        Direction = reader.GetString(reader.GetOrdinal("Direction")),
        //        Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
        //        Currency = reader.GetString(reader.GetOrdinal("Currency")),
        //        BalanceAfter = reader.GetDecimal(reader.GetOrdinal("BalanceAfter")),
        //        CounterpartyName = reader.IsDBNull(reader.GetOrdinal("CounterpartyName")) ? null : reader.GetString(reader.GetOrdinal("CounterpartyName")),
        //        CounterpartyIBAN = reader.IsDBNull(reader.GetOrdinal("CounterpartyIBAN")) ? null : reader.GetString(reader.GetOrdinal("CounterpartyIBAN")),
        //        MerchantName = reader.IsDBNull(reader.GetOrdinal("MerchantName")) ? null : reader.GetString(reader.GetOrdinal("MerchantName")),
        //        CategoryId = reader.IsDBNull(reader.GetOrdinal("CategoryId")) ? null : reader.GetInt32(reader.GetOrdinal("CategoryId")),
        //        Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
        //        Fee = reader.GetDecimal(reader.GetOrdinal("Fee")),
        //        ExchangeRate = reader.IsDBNull(reader.GetOrdinal("ExchangeRate")) ? null : reader.GetDecimal(reader.GetOrdinal("ExchangeRate")),
        //        Status = reader.GetString(reader.GetOrdinal("Status")),
        //        RelatedEntityType = reader.IsDBNull(reader.GetOrdinal("RelatedEntityType")) ? null : reader.GetString(reader.GetOrdinal("RelatedEntityType")),
        //        RelatedEntityId = reader.IsDBNull(reader.GetOrdinal("RelatedEntityId")) ? null : reader.GetInt32(reader.GetOrdinal("RelatedEntityId")),
        //        CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
        //    };
        //}

        //private static TransactionHistoryItemDto MapToTransactionHistoryItem(IDataReader reader)
        //{
        //    string? cardNumber = reader.IsDBNull(reader.GetOrdinal("CardNumber")) ? null : reader.GetString(reader.GetOrdinal("CardNumber"));

        //    return new TransactionHistoryItemDto
        //    {
        //        Id = reader.GetInt32(reader.GetOrdinal("Id")),
        //        AccountId = reader.GetInt32(reader.GetOrdinal("AccountId")),
        //        CardId = reader.IsDBNull(reader.GetOrdinal("CardId")) ? null : reader.GetInt32(reader.GetOrdinal("CardId")),
        //        AccountName = reader.IsDBNull(reader.GetOrdinal("AccountName")) ? string.Empty : reader.GetString(reader.GetOrdinal("AccountName")),
        //        AccountIban = reader.GetString(reader.GetOrdinal("AccountIban")),
        //        CardLabel = string.IsNullOrWhiteSpace(cardNumber) || cardNumber.Length < 4 ? null : $"**** {cardNumber[^4..]}",
        //        Timestamp = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
        //        TransactionType = reader.GetString(reader.GetOrdinal("Type")),
        //        ReferenceNumber = reader.GetString(reader.GetOrdinal("TransactionRef")),
        //        Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
        //        CounterpartyOrMerchant = reader.GetString(reader.GetOrdinal("CounterpartyOrMerchant")),
        //        MerchantName = reader.IsDBNull(reader.GetOrdinal("MerchantName")) ? null : reader.GetString(reader.GetOrdinal("MerchantName")),
        //        CounterpartyName = reader.IsDBNull(reader.GetOrdinal("CounterpartyName")) ? null : reader.GetString(reader.GetOrdinal("CounterpartyName")),
        //        SourceAccountIban = reader.GetString(reader.GetOrdinal("SourceAccountIban")),
        //        DestinationAccountIban = reader.IsDBNull(reader.GetOrdinal("DestinationAccountIban")) ? null : reader.GetString(reader.GetOrdinal("DestinationAccountIban")),
        //        Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
        //        Currency = reader.GetString(reader.GetOrdinal("Currency")),
        //        Direction = reader.GetString(reader.GetOrdinal("Direction")),
        //        RunningBalanceAfterTransaction = reader.GetDecimal(reader.GetOrdinal("BalanceAfter")),
        //        Status = reader.GetString(reader.GetOrdinal("Status")),
        //        Fee = reader.GetDecimal(reader.GetOrdinal("Fee")),
        //        ExchangeRate = reader.IsDBNull(reader.GetOrdinal("ExchangeRate")) ? null : reader.GetDecimal(reader.GetOrdinal("ExchangeRate")),
        //        CategoryName = reader.GetString(reader.GetOrdinal("CategoryName"))
        //    };
        //}
    }
}
