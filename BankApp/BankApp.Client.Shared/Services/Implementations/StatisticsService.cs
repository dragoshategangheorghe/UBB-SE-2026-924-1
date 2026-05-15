using System;
using System.Threading.Tasks;
using BankApp.Client.RepoProxies.Interfaces;
using BankApp.Client.Services.Interfaces;
using BankApp.Models.DTOs.Statistics;

namespace BankApp.Client.Services.Implementations
{
    public class StatisticsService : IStatisticsService
    {
        private readonly IStatisticsRepoProxy _repoProxy;
        private readonly IAuthService _authService;

        public StatisticsService(IStatisticsRepoProxy repoProxy, IAuthService authService)
        {
            _repoProxy = repoProxy;
            _authService = authService;
        }

        public Task<SpendingByCategoryResponse?> GetSpendingByCategoryAsync()
        {
            EnsureAuthenticatedSession();
            return _repoProxy.GetSpendingByCategoryAsync();
        }

        public Task<IncomeVsExpensesResponse?> GetIncomeVsExpensesAsync()
        {
            EnsureAuthenticatedSession();
            return _repoProxy.GetIncomeVsExpensesAsync();
        }

        public Task<BalanceTrendsResponse?> GetBalanceTrendsAsync()
        {
            EnsureAuthenticatedSession();
            return _repoProxy.GetBalanceTrendsAsync();
        }

        public Task<TopRecipientsResponse?> GetTopRecipientsAsync()
        {
            EnsureAuthenticatedSession();
            return _repoProxy.GetTopRecipientsAsync();
        }

        private void EnsureAuthenticatedSession()
        {
            if (!_authService.IsAuthenticated())
            {
                throw new UnauthorizedAccessException("An authenticated session is required.");
            }
        }
    }
}

