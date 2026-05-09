using BankApp.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankApp.Client.RepositoryProxies.Interfaces
{
    public interface IDashboardRepositoryProxy
    {
        Task<List<Card>?> GetCardsByUserAsync();
        Task<List<Transaction>?> GetRecentTransactionsAsync();
        Task<int> GetUnreadNotificationsCountAsync();
        Task<List<Account>?> GetAccountsByUserAsync();
    }
}
