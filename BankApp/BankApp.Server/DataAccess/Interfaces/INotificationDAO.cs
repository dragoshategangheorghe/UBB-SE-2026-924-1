using BankApp.Models.Entities;
namespace BankApp.Server.DataAccess.Interfaces
{
    public interface INotificationDAO
    {
        List<Notification> FindByUserId(int userId);
        int CountUnreadByUserId(int userId);
    }
}