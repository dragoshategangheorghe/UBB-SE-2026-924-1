using BankApp.Client.RepositoryProxies.Interfaces;
using BankApp.Client.Utilities;
using BankApp.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApp.Client.RepositoryProxies.Implementations
{
    public class DashboardRepositoryProxy : IDashboardRepositoryProxy
    {
        private readonly ApiService apiService;

        public DashboardRepositoryProxy(ApiService apiService)
        {
            this.apiService = apiService;
        }

        public Task<List<Card>?> GetCardsByUserAsync()
        {
            return this.apiService.GetAsync<List<Card>>($"/api/dashboard/cards");
        }

        public Task<List<Transaction>?> GetRecentTransactionsAsync()
        {
            return this.apiService.GetAsync<List<Transaction>>($"/api/dashboard/recentTransactions");
        }

        public Task<int> GetUnreadNotificationsCountAsync()
        {
            return this.apiService.GetAsync<int>($"/api/dashboard/unreadNotificationCount");
        }

        public Task<List<Account>?> GetAccountsByUserAsync()
        {
            return this.apiService.GetAsync<List<Account>>($"/api/dashboard/accounts");
        }
    }
}
