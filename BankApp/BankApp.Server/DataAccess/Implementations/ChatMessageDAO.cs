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
        var messages = db.ChatMessages
             .Where(m => m.SessionId == sessionId)
             .OrderBy(m => m.SentAt)
             .ToList();
        return messages;
    }

    public int Create(ChatMessage message)
    {
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
        var attachments = db.ChatAttachments
            .Where(a => a.MessageId == messageId)
            .ToList();

        return attachments;
    }

    public int CreateAttachment(ChatAttachment attachment)
    {
        db.ChatAttachments.Add(attachment);
        var rows = db.SaveChanges();

        if (rows <= 0)
        {
            return 0;
        }

        return attachment.Id;
    }
}
