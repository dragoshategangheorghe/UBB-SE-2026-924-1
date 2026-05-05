using BankApp.Models.Features.Chat;
using BankApp.Server.DataAccess.Interfaces;

namespace BankApp.Server.Repositories.Implementations;

public class ChatMessageRepository
{
    private readonly IChatMessageDAO chatMessageDao;

    public ChatMessageRepository(IChatMessageDAO chatMessageDao)
    {
        this.chatMessageDao = chatMessageDao;
    }

    public List<ChatMessage> GetBySessionId(int sessionId)
    {
        return chatMessageDao.GetBySessionId(sessionId);
    }

    public int Create(ChatMessage message)
    {
        return chatMessageDao.Create(message);
    }

    public List<ChatAttachment> GetAttachmentsByMessageId(int messageId)
    {
        return chatMessageDao.GetAttachmentsByMessageId(messageId);
    }

    public int CreateAttachment(ChatAttachment attachment)
    {
        return chatMessageDao.CreateAttachment(attachment);
    }
}