using BankApp.Models.Entities;
using BankApp.Server.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankApp.Server.DataAccess.Implementations
{
    public class AccountDAO : IAccountDAO
    {
        private readonly AppDbContext _dbContext;

        public AccountDAO(AppDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        /// <summary>
        /// Loads an account together with its owning user through the EF navigation mapping.
        /// </summary>
        public Account? FindById(int accountId)
        {
            return _dbContext.Accounts
                .Include(account => account.User)
                .Include(account => account.Cards)
                .Include(account => account.Transactions)
                .FirstOrDefault(account => account.Id == accountId);
        }

        /// <summary>
        /// Loads all accounts for a user through the Account.User navigation property.
        /// </summary>
        public List<Account> FindByUserId(int userId)
        {
            return _dbContext.Accounts
                .Include(account => account.User)
                .Where(account => account.User.Id == userId)
                .ToList();
        }
    }
}
