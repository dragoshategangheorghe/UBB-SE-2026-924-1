namespace BankApp.Client.RepoProxies.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BankApp.Client.RepoProxies.Interfaces;
    using BankApp.Models.Entities;

    public class AccountRepoProxy : IAccountRepoProxy
    {
        private readonly ApiService api;

        public AccountRepoProxy(ApiService api) => this.api = api;

        public async Task<IEnumerable<Account>> GetAuthenticatedAccountsAsync()
        {
            int? userId = this.api.GetCurrentUserId();
            if (!userId.HasValue)
            {
                throw new UnauthorizedAccessException("A valid authenticated session is required to access accounts.");
            }

            return await this.api.GetAsync<List<Account>>($"api/accounts/user/{userId.Value}");
        }
    }
}