using BankApp.Models.Features.Chat;
using BankApp.Server.DataAccess.Interfaces;
using BankApp.Server.Repositories.Interfaces;

namespace BankApp.Server.Repositories.Implementations;

public class ChatRepository : IChatRepository
{
    private readonly IChatDAO chatDao;

    public ChatRepository(IChatDAO chatDao)
    {
        this.chatDao = chatDao;
    }

    public List<ChatSession> GetByUserId(int userId)
    {
        return chatDao.GetByUserId(userId);
    }

    public ChatSession? GetById(int id)
    {
        return chatDao.GetById(id);
    }

    public int Create(ChatSession session)
    {
        return chatDao.Create(session);
    }

    public bool UpdateStatus(int id, string status)
    {
        return chatDao.UpdateStatus(id, status);
    }

    public bool SaveFeedback(int id, int rating, string feedback)
    {
        return chatDao.SaveFeedback(id, rating, feedback);
    }
}