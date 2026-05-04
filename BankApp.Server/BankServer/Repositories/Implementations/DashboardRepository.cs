using BankApp.Models.Entities;
using BankApp.Server.Repositories.Interfaces;
using BankApp.Server.DataAccess.Interfaces;

namespace BankApp.Server.Repositories.Implementations
{
	public class DashboardRepository : IDashboardRepository
	{
		private readonly IAccountDAO accountDAO;
		private readonly ICardDAO cardDAO;
		private readonly ITransactionDAO transactionDAO;
		private readonly INotificationDAO notificationDAO;

		public DashboardRepository(IAccountDAO accountDAO, ICardDAO cardDAO, ITransactionDAO transactionDAO, INotificationDAO notificationDAO)
		{
			this.accountDAO = accountDAO;
			this.cardDAO = cardDAO;
			this.transactionDAO = transactionDAO;
			this.notificationDAO = notificationDAO;
		}

		public List<Account> GetAccountsByUser(int userId)
		{
			return accountDAO.FindByUserId(userId);
		}
		public List<Card> GetCardsByUser(int userId)
		{
			return cardDAO.FindByUserId(userId);
		}
		public List<Transaction> GetRecentTransactions(int accountId, int limit = 10)
		{
			return transactionDAO.FindRecentByAccountId(accountId, limit);
		}
		public int GetUnreadNotificationCount(int userId)
		{
			return notificationDAO.CountUnreadByUserId(userId);
		}
	}
}