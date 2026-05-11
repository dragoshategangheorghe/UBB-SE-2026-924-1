namespace BankApp.Client.RepoProxies.Interfaces
{
    using System.Threading.Tasks;
    using BankApp.Models.Entities;

    public interface IInvestmentsRepoProxy
    {
        Task<Portfolio?> GetPortfolioAsync(int userId);

        Task<Portfolio?> GetPortfolioForCurrentUserAsync();
    }
}