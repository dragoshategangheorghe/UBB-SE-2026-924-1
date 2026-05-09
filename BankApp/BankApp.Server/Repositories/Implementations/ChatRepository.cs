using BankApp.Models.Features.Chat;
using BankApp.Server.DataAccess.Interfaces;
using BankApp.Server.Repositories.Interfaces;

namespace BankApp.Server.Repositories.Implementations;

public class ChatRepository : IChatRepository
{
    private readonly IChatDAO _chatDao;

    public ChatRepository(IChatDAO chatDao)
    {
        this._chatDao = chatDao;
    }

    public List<ChatSession> GetByUserId(int userId)
    {
        return _chatDao.GetByUserId(userId);
    }

    public ChatSession? GetById(int id)
    {
        return _chatDao.GetById(id);
    }

    public int Create(ChatSession session)
    {
        return _chatDao.Create(session);
    }

    public bool UpdateStatus(int id, string status)
    {
        return _chatDao.UpdateStatus(id, status);
    }

    public bool SaveFeedback(int id, int rating, string feedback)
    {
        return _chatDao.SaveFeedback(id, rating, feedback);
    }
}