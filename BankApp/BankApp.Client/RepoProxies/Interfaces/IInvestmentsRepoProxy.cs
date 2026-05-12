namespace BankApp.Client.RepoProxies.Interfaces
{
    using System.Threading.Tasks;
    using BankApp.Models.Entities;

    public interface IInvestmentsRepoProxy
    {
        Task<TResponse?> GetAsync<TResponse>(string endpoint);
        Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data);
    }
}