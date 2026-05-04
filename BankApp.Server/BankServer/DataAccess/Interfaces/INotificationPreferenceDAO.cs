using BankApp.Models.Entities;
namespace BankApp.Server.DataAccess.Interfaces
{
    public interface INotificationPreferenceDAO
    {
        bool Create(int userId, string category);
        List<NotificationPreference> FindByUserId(int userId);
        bool Update(int userId, List<NotificationPreference> prefs);
    }
}