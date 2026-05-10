namespace BankApp.Client.RepoProxies.Implementations
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BankApp.Client.RepoProxies.Interfaces;
    using BankApp.Client.Utilities;
    using BankApp.Models.Entities;

    public class AccountRepoProxy : IAccountRepoProxy
    {
        private readonly ApiService api;

        public AccountRepoProxy(ApiService api) => this.api = api;

        public async Task<IEnumerable<Account>> GetByUserIdAsync(int userId)
        {
            return await this.api.GetAsync<List<Account>>($"api/accounts/user/{userId}");
        }
    }
}