namespace BankApp.Client.Services.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BankApp.Client.RepoProxies.Interfaces;
    using BankApp.Client.Services.Interfaces;
    using BankApp.Models.Entities;

    public class AccountService : IAccountService
    {
        private readonly IAccountRepoProxy repo;
        private readonly IAuthService authService;

        public AccountService(IAccountRepoProxy repo, IAuthService authService)
        {
            this.repo = repo;
            this.authService = authService;
        }

        public async Task<IEnumerable<Account>> GetAccountsAsync()
        {
            EnsureAuthenticatedSession();
            return await this.repo.GetAuthenticatedAccountsAsync();
        }

        private void EnsureAuthenticatedSession()
        {
            if (!this.authService.IsAuthenticated())
            {
                throw new UnauthorizedAccessException("An authenticated session is required.");
            }
        }
    }
}