namespace BankApp.Client.Services.Implementations
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BankApp.Client.RepoProxies.Implementations;
    using BankApp.Client.RepoProxies.Interfaces;
    using BankApp.Client.Services.Interfaces;
    using BankApp.Models.Entities;

    public class AccountService : IAccountService
    {
        private readonly IAccountRepoProxy repo;

        public AccountService(IAccountRepoProxy repo) => this.repo = repo;

        public async Task<IEnumerable<Account>> GetUserAccountsAsync(int userId)
        {
            return await this.repo.GetByUserIdAsync(userId);
        }
    }
}