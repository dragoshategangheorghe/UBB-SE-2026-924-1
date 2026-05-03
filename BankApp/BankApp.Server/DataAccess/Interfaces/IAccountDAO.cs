using BankApp.Models.Entities;
namespace BankApp.Server.DataAccess.Interfaces
{
    public interface IAccountDAO
    {
        List<Account> FindByUserId(int userId);
        Account? FindById(int id);
    }
}