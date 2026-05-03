using BankApp.Models.DTOs.Transactions;
using BankApp.Models.Entities;

namespace BankApp.Server.DataAccess.Interfaces
{
    public interface ITransactionDAO
    {
        List<Transaction> FindRecentByAccountId(int accountId, int limit = 10);
        List<TransactionHistoryItemDto> FindByUserId(int userId);
        TransactionHistoryItemDto? FindById(int userId, int transactionId);
    }
}
