using BankApp.Models.Features.Chat;

namespace BankApp.Server.DataAccess.Interfaces;

public interface IChatDAO
{
    List<ChatSession> GetByUserId(int userId);
    ChatSession? GetById(int id);
    int Create(ChatSession session);
    bool UpdateStatus(int id, string status);
    bool SaveFeedback(int id, int rating, string feedback);
}
