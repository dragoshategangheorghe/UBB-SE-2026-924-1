using BankApp.Models.Features.Chat;
using BankApp.Server.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankApp.Server.DataAccess.Implementations;

public class ChatDAO : IChatDAO
{
    private readonly AppDbContext db;

    public ChatDAO(AppDbContext db)
    {
        this.db = db;
    }

    public List<ChatSession> GetByUserId(int userId)
    {
        return db.ChatSessions
            .Include(s => s.User)
            .Where(s => EF.Property<int>(s, "UserId") == userId)
            .OrderByDescending(s => s.StartedAt)
            .AsNoTracking()
            .ToList();
    }

    public ChatSession? GetById(int id)
    {
        ChatSession? session = db.ChatSessions
            .Include(s => s.User)
            .FirstOrDefault(s => s.Id == id);
        return session;
    }

    public int Create(ChatSession session)
    {
        db.ChatSessions.Add(session);
        db.SaveChanges();
        return session.Id;
    }

    public bool UpdateStatus(int id, string status)
    {
        var rowsAffected = db.ChatSessions
            .Where(cs => cs.Id == id)
            .ExecuteUpdate(s => s.SetProperty(cs => cs.SessionStatus, status));

        return rowsAffected > 0;
    }

    public bool SaveFeedback(int id, int rating, string feedback)
    {
        var rowsAffected = db.ChatSessions
            .Where(cs => cs.Id == id)
            .ExecuteUpdate(s => s
                .SetProperty(cs => cs.Rating, rating)
                .SetProperty(cs => cs.Feedback, feedback));

        return rowsAffected > 0;
    }
}
