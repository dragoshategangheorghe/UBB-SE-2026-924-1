using BankApp.Models.Features.Chat;
using BankApp.Server.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankApp.Server.DataAccess.Implementations;

public class ChatDAO : IChatDAO
{
    private readonly AppDbContext _dbContext;

    public ChatDAO(AppDbContext dbContext)
    {
        this._dbContext = dbContext;
    }

    public List<ChatSession> GetByUserId(int userId)
    {
        return _dbContext.ChatSessions
            .Include(session => session.User)
            .Where(session => EF.Property<int>(session, "UserId") == userId)
            .OrderByDescending(session => session.StartedAt)
            .AsNoTracking()
            .ToList();
    }

    public ChatSession? GetById(int chatSessionId)
    {
        ChatSession? chatSession = _dbContext.ChatSessions
            .Include(chatSession => chatSession.User)
            .FirstOrDefault(chatSession => chatSession.Id == chatSessionId);
        return chatSession;
    }

    public int Create(ChatSession chatSession)
    {
        _dbContext.ChatSessions.Add(chatSession);
        _dbContext.SaveChanges();
        return chatSession.Id;
    }

    public bool UpdateStatus(int chatSessionId, string status)
    {
        var rowsAffected = _dbContext.ChatSessions
            .Where(chatSession => chatSession.Id == chatSessionId)
            .ExecuteUpdate(setters => setters.SetProperty(chatSession => chatSession.SessionStatus, status));

        return rowsAffected > 0;
    }

    public bool SaveFeedback(int chatSessionId, int rating, string feedback)
    {
        var rowsAffected = _dbContext.ChatSessions
            .Where(chatSession => chatSession.Id == chatSessionId)
            .ExecuteUpdate(setters => setters
                .SetProperty(chatSession => chatSession.Rating, rating)
                .SetProperty(chatSession => chatSession.Feedback, feedback));

        return rowsAffected > 0;
    }
}
