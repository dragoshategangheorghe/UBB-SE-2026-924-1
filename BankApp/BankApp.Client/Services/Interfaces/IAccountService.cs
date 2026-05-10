namespace BankApp.Client.Services.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BankApp.Models.Entities;

    /// <summary>
    /// Interface for the Account business service.
    /// </summary>
    public interface IAccountService
    {
        Task<IEnumerable<Account>> GetUserAccountsAsync(int userId);
    }
}