namespace BankApp.Client.RepoProxies.Implementations
{
    using System.Threading.Tasks;
    using BankApp.Client.RepoProxies.Interfaces;
    using BankApp.Models.Entities;

    public class InvestmentsRepoProxy : IInvestmentsRepoProxy
    {
        private readonly ApiService _api;

        public InvestmentsRepoProxy(ApiService api)
        {
            this._api = api;
        }

        public async Task<TResponse?> GetAsync<TResponse>(string endpoint)
        {
            return await this._api.GetAsync<TResponse>(endpoint);
        }

        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            return await this._api.PostAsync<TRequest, TResponse>(endpoint, data);
        }
    }
}