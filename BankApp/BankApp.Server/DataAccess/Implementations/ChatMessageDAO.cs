using BankApp.Models.Features.Chat;
using BankApp.Server.DataAccess.Interfaces;

namespace BankApp.Server.DataAccess.Implementations;

public class ChatMessageDAO : IChatMessageDAO
{
    private readonly AppDbContext _dbContext;

    public ChatMessageDAO(AppDbContext dbContext)
    {
        this._dbContext = dbContext;
    }

    public List<ChatMessage> GetBySessionId(int sessionId)
    {
        var messages = _dbContext.ChatMessages
             .Where(message => message.SessionId == sessionId)
             .OrderBy(message => message.SentAt)
             .ToList();
        return messages;
    }

    public int Create(ChatMessage message)
    {
        message.SentAt = message.SentAt == default
            ? DateTime.UtcNow
            : message.SentAt;

        _dbContext.ChatMessages.Add(message);
        var rows = _dbContext.SaveChanges();

        if (rows <= 0)
        {
            return 0;
        }

        return message.Id;
    }

    public List<ChatAttachment> GetAttachmentsByMessageId(int messageId)
    {
        var attachments = _dbContext.ChatAttachments
            .Where(attachment => attachment.MessageId == messageId)
            .ToList();

        return attachments;
    }

    public int CreateAttachment(ChatAttachment attachment)
    {
        _dbContext.ChatAttachments.Add(attachment);
        var rows = _dbContext.SaveChanges();

        if (rows <= 0)
        {
            return 0;
        }

        return attachment.Id;
    }
}
