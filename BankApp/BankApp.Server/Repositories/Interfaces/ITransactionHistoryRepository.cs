using BankApp.Models.DTOs.Transactions;
using BankApp.Models.Entities;

namespace BankApp.Server.Repositories.Interfaces
{
    public interface ITransactionHistoryRepository
    {
        List<TransactionHistoryItemDto> GetTransactionsByUserId(int userId);
        TransactionHistoryItemDto? GetTransactionById(int userId, int transactionId);
        List<Account> GetAccountsByUserId(int userId);
        List<Card> GetCardsByUserId(int userId);
    }
}
