using BankApp.Models.Features.Chat;
using BankApp.Server.DataAccess.Interfaces;

namespace BankApp.Server.DataAccess.Implementations;

public class ChatMessageDAO : IChatMessageDAO
{
    private readonly AppDbContext db;

    public ChatMessageDAO(AppDbContext db)
    {
        this.db = db;
    }

    public List<ChatMessage> GetBySessionId(int sessionId)
    {
        //var sql = @"SELECT Id, SessionId, SenderType, Content, SentAt
        //            FROM ChatMessage
        //            WHERE SessionId = @p0
        //            ORDER BY SentAt ASC";

        //var messages = new List<ChatMessage>();
        //using var reader = db.ExecuteQuery(sql, new object[] { sessionId });
        //while (reader.Read())
        //{
        //    messages.Add(MapMessage(reader));
        //}

        //return messages;

        var messages = db.ChatMessages
             .Where(m => m.SessionId == sessionId)
             .OrderBy(m => m.SentAt)
             .ToList();
        return messages;
    }

    public int Create(ChatMessage message)
    {
        //var sql = @"INSERT INTO ChatMessage (SessionId, SenderType, Content, SentAt)
        //            VALUES (@p0, @p1, @p2, @p3)";

        //var rows = db.ExecuteNonQuery(sql, new object[]
        //{
        //    message.SessionId,
        //    message.SenderType,
        //    message.Content,
        //    message.SentAt == default ? DateTime.UtcNow : message.SentAt
        //});

        //if (rows <= 0)
        //{
        //    return 0;
        //}

        //var idSql = "SELECT TOP 1 Id FROM ChatMessage WHERE SessionId = @p0 ORDER BY Id DESC";
        //using var reader = db.ExecuteQuery(idSql, new object[] { message.SessionId });
        //return reader.Read() ? reader.GetInt32(reader.GetOrdinal("Id")) : 0;

        message.SentAt = message.SentAt == default
        ? DateTime.UtcNow
        : message.SentAt;

        db.ChatMessages.Add(message);
        var rows = db.SaveChanges();

        if (rows <= 0)
        {
            return 0;
        }

        return message.Id;

    }

    public List<ChatAttachment> GetAttachmentsByMessageId(int messageId)
    {
        //var sql = @"SELECT Id, MessageId, AttachmentName, FileType, FileSizeBytes, StorageUrl
        //            FROM ChatAttachment
        //            WHERE MessageId = @p0";

        //var attachments = new List<ChatAttachment>();
        //using var reader = db.ExecuteQuery(sql, new object[] { messageId });
        //while (reader.Read())
        //{
        //    attachments.Add(new ChatAttachment
        //    {
        //        Id = reader.GetInt32(reader.GetOrdinal("Id")),
        //        MessageId = reader.GetInt32(reader.GetOrdinal("MessageId")),
        //        AttachmentName = reader.IsDBNull(reader.GetOrdinal("AttachmentName")) ? string.Empty : reader.GetString(reader.GetOrdinal("AttachmentName")),
        //        FileType = reader.IsDBNull(reader.GetOrdinal("FileType")) ? string.Empty : reader.GetString(reader.GetOrdinal("FileType")),
        //        FileSizeBytes = reader.IsDBNull(reader.GetOrdinal("FileSizeBytes")) ? 0 : reader.GetInt32(reader.GetOrdinal("FileSizeBytes")),
        //        StorageUrl = reader.IsDBNull(reader.GetOrdinal("StorageUrl")) ? string.Empty : reader.GetString(reader.GetOrdinal("StorageUrl"))
        //    });
        //}

        //return attachments;

        var attachments = db.ChatAttachments
            .Where(a => a.MessageId == messageId)
            .ToList();

        return attachments;

    }

    public int CreateAttachment(ChatAttachment attachment)
    {
        //var sql = @"INSERT INTO ChatAttachment (MessageId, AttachmentName, FileType, FileSizeBytes, StorageUrl)
        //            VALUES (@p0, @p1, @p2, @p3, @p4)";

        //var rows = db.ExecuteNonQuery(sql, new object[]
        //{
        //    attachment.MessageId,
        //    attachment.AttachmentName,
        //    attachment.FileType,
        //    attachment.FileSizeBytes,
        //    attachment.StorageUrl
        //});

        //if (rows <= 0)
        //{
        //    return 0;
        //}

        //var idSql = "SELECT TOP 1 Id FROM ChatAttachment WHERE MessageId = @p0 ORDER BY Id DESC";
        //using var reader = db.ExecuteQuery(idSql, new object[] { attachment.MessageId });
        //return reader.Read() ? reader.GetInt32(reader.GetOrdinal("Id")) : 0;
        db.ChatAttachments.Add(attachment);
        var rows = db.SaveChanges();

        if (rows <= 0)
        {
            return 0;
        }

        return attachment.Id;

    }

    //private static ChatMessage MapMessage(System.Data.IDataReader reader)
    //{
    //    return new ChatMessage
    //    {
    //        Id = reader.GetInt32(reader.GetOrdinal("Id")),
    //        SessionId = reader.GetInt32(reader.GetOrdinal("SessionId")),
    //        SenderType = reader.IsDBNull(reader.GetOrdinal("SenderType")) ? string.Empty : reader.GetString(reader.GetOrdinal("SenderType")),
    //        Content = reader.IsDBNull(reader.GetOrdinal("Content")) ? string.Empty : reader.GetString(reader.GetOrdinal("Content")),
    //        SentAt = reader.GetDateTime(reader.GetOrdinal("SentAt"))
    //    };
    //}
}
