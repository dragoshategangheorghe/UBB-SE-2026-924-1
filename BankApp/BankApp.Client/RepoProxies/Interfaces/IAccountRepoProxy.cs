namespace BankApp.Client.RepoProxies.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BankApp.Models.Entities;

    /// <summary>
    /// Proxy interface for Account repository operations via HTTP.
    /// </summary>
    public interface IAccountRepoProxy
    {
        Task<IEnumerable<Account>> GetByUserIdAsync(int userId);
    }
}