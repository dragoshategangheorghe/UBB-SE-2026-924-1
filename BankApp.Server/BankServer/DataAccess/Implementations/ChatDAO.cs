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
        //var sql = @"SELECT Id, UserId, IssueCategory, SessionStatus, Rating, Feedback, StartedAt, EndedAt
        //            FROM ChatSession
        //            WHERE UserId = @p0
        //            ORDER BY StartedAt DESC";

        //var sessions = new List<ChatSession>();
        //using var reader = db.ExecuteQuery(sql, new object[] { userId });
        //while (reader.Read())
        //{
        //    sessions.Add(MapSession(reader));
        //}

        //return sessions;

        List<ChatSession> sessions = db.ChatSessions.Where(s => s.UserId == userId).OrderByDescending(s => s.StartedAt).ToList();
        return sessions;
    }

    public ChatSession? GetById(int id)
    {
        //var sql = @"SELECT Id, UserId, IssueCategory, SessionStatus, Rating, Feedback, StartedAt, EndedAt
        //            FROM ChatSession
        //            WHERE Id = @p0";

        //using var reader = db.ExecuteQuery(sql, new object[] { id });
        //if (!reader.Read())
        //{
        //    return null;
        //}

        //return MapSession(reader);

        ChatSession? session = db.ChatSessions.FirstOrDefault(s => s.Id == id);
        return session;
    }

    public int Create(ChatSession session)
    {
        //var sql = @"INSERT INTO ChatSession (UserId, IssueCategory, SessionStatus, Rating, Feedback, StartedAt, EndedAt)
        //            VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6)";

        //var rows = db.ExecuteNonQuery(sql, new object[]
        //{
        //    session.UserId,
        //    session.IssueCategory,
        //    session.SessionStatus,
        //    session.Rating,
        //    string.IsNullOrWhiteSpace(session.Feedback) ? DBNull.Value : session.Feedback,
        //    session.StartedAt == default ? DateTime.UtcNow : session.StartedAt,
        //    session.EndedAt == default ? DBNull.Value : session.EndedAt
        //});

        //if (rows <= 0)
        //{
        //    return 0;
        //}

        //var idSql = "SELECT TOP 1 Id FROM ChatSession WHERE UserId = @p0 ORDER BY Id DESC";
        //using var reader = db.ExecuteQuery(idSql, new object[] { session.UserId });
        //return reader.Read() ? reader.GetInt32(reader.GetOrdinal("Id")) : 0;

        db.ChatSessions.Add(session);
        return db.ChatSessions
         .Where(cs => cs.UserId == session.UserId)
         .OrderByDescending(cs => cs.Id)
         .Select(cs => cs.Id)
         .FirstOrDefault();
    }

    public bool UpdateStatus(int id, string status)
    {
        //var sql = "UPDATE ChatSession SET SessionStatus = @p0 WHERE Id = @p1";
        //return db.ExecuteNonQuery(sql, new object[] { status, id }) > 0;

        var rowsAffected = db.ChatSessions
       .Where(cs => cs.Id == id)
       .ExecuteUpdate(s =>
           s.SetProperty(cs => cs.SessionStatus, status)
       );

        return rowsAffected > 0;
    }

    public bool SaveFeedback(int id, int rating, string feedback)
    {
        //var sql = "UPDATE ChatSession SET Rating = @p0, Feedback = @p1 WHERE Id = @p2";
        //return db.ExecuteNonQuery(sql, new object[] { rating, feedback, id }) > 0;

        var rowsAffected = db.ChatSessions
        .Where(cs => cs.Id == id)
        .ExecuteUpdate(s => s
        .SetProperty(cs => cs.Rating, rating)
        .SetProperty(cs => cs.Feedback, feedback)
     );

        return rowsAffected > 0;
    }

    //private static ChatSession MapSession(System.Data.IDataReader reader)
    //{
    //    var endedAtOrdinal = reader.GetOrdinal("EndedAt");
    //    var feedbackOrdinal = reader.GetOrdinal("Feedback");

    //    return new ChatSession
    //    {
    //        Id = reader.GetInt32(reader.GetOrdinal("Id")),
    //        UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
    //        IssueCategory = reader.IsDBNull(reader.GetOrdinal("IssueCategory")) ? string.Empty : reader.GetString(reader.GetOrdinal("IssueCategory")),
    //        SessionStatus = reader.IsDBNull(reader.GetOrdinal("SessionStatus")) ? string.Empty : reader.GetString(reader.GetOrdinal("SessionStatus")),
    //        Rating = reader.IsDBNull(reader.GetOrdinal("Rating")) ? 0 : reader.GetInt32(reader.GetOrdinal("Rating")),
    //        Feedback = reader.IsDBNull(feedbackOrdinal) ? string.Empty : reader.GetString(feedbackOrdinal),
    //        StartedAt = reader.GetDateTime(reader.GetOrdinal("StartedAt")),
    //        EndedAt = reader.IsDBNull(endedAtOrdinal) ? default : reader.GetDateTime(endedAtOrdinal)
    //    };
    //}
}
