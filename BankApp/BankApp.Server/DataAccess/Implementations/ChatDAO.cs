using BankApp.Models.Features.Chat;
using BankApp.Server.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankApp.Server.DataAccess.Implementations;

public class ChatDAO : IChatDAO
{
    private readonly AppDbContext _dbContext;

    public ChatDAO(AppDbContext db)
    {
        this._dbContext = db;
    }

    public List<ChatSession> GetByUserId(int userId)
    {

        List<ChatSession> sessions = _dbContext.ChatSessions.Where(s => s.Id == userId).OrderByDescending(s => s.StartedAt).ToList();
        return sessions;
    }

    public ChatSession? GetById(int id)
    {

        ChatSession? session = _dbContext.ChatSessions.FirstOrDefault(s => s.Id == id);
        return session;
    }

    public int Create(ChatSession session)
    {
        _dbContext.ChatSessions.Add(session);
        return _dbContext.ChatSessions
         .Where(cs => cs.Id == session.Id)
         .OrderByDescending(cs => cs.Id)
         .Select(cs => cs.Id)
         .FirstOrDefault();
    }

    public bool UpdateStatus(int id, string status)
    {
        var rowsAffected = _dbContext.ChatSessions
       .Where(cs => cs.Id == id)
       .ExecuteUpdate(s =>
           s.SetProperty(cs => cs.SessionStatus, status)
       );

        return rowsAffected > 0;
    }

    public bool SaveFeedback(int id, int rating, string feedback)
    {
        var rowsAffected = _dbContext.ChatSessions
        .Where(cs => cs.Id == id)
        .ExecuteUpdate(s => s
        .SetProperty(cs => cs.Rating, rating)
        .SetProperty(cs => cs.Feedback, feedback)
     );

        return rowsAffected > 0;
    }

}
