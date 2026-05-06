using BankApp.Models.Entities;
using BankApp.Server.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankApp.Server.DataAccess.Implementations
{
    public class AccountDAO : IAccountDAO
    {
        private readonly AppDbContext dbContext;

        public AccountDAO(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Loads an account together with its owning user through the EF navigation mapping.
        /// </summary>
        public Account? FindById(int id)
        {
            return dbContext.Accounts
                .Include(a => a.User)
                .Include(a => a.Cards)
                .Include(a => a.Transactions)
                .FirstOrDefault(a => a.Id == id);
        }

        /// <summary>
        /// Loads all accounts for a user through the Account.User navigation property.
        /// </summary>
        public List<Account> FindByUserId(int userId)
        {
            return dbContext.Accounts
                .Include(a => a.User)
                .Where(a => a.User.Id == userId)
                .ToList();
        }
    }
}
