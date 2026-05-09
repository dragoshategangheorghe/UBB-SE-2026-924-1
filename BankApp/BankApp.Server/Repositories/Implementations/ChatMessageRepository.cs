using BankApp.Models.Features.Chat;
using BankApp.Server.DataAccess.Interfaces;

namespace BankApp.Server.Repositories.Implementations;

public class ChatMessageRepository
{
    private readonly IChatMessageDAO _chatMessageDao;

    public ChatMessageRepository(IChatMessageDAO chatMessageDao)
    {
        this._chatMessageDao = chatMessageDao;
    }

    public List<ChatMessage> GetBySessionId(int sessionId)
    {
        return _chatMessageDao.GetBySessionId(sessionId);
    }

    public int Create(ChatMessage message)
    {
        return _chatMessageDao.Create(message);
    }

    public List<ChatAttachment> GetAttachmentsByMessageId(int messageId)
    {
        return _chatMessageDao.GetAttachmentsByMessageId(messageId);
    }

    public int CreateAttachment(ChatAttachment attachment)
    {
        return _chatMessageDao.CreateAttachment(attachment);
    }
}