using BankApp.Models.Features.Chat;
using BankApp.Server.Repositories.Implementations;
using BankApp.Server.Repositories.Interfaces;
using BankApp.Server.Services.Interfaces;

namespace BankApp.Server.Services.Implementations;

public class ChatService : IApiService
{
    private readonly IChatRepository chatRepository;
    private readonly ChatMessageRepository chatMessageRepository;

    public ChatService(IChatRepository chatRepository, ChatMessageRepository chatMessageRepository)
    {
        this.chatRepository = chatRepository;
        this.chatMessageRepository = chatMessageRepository;
    }

    public List<ChatSession> GetSessionsByUserId(int userId)
    {
        return chatRepository.GetByUserId(userId);
    }

    public ChatSession? GetSessionById(int sessionId)
    {
        return chatRepository.GetById(sessionId);
    }

    public int CreateSession(int userId, string issueCategory)
    {
        var session = new ChatSession
        {
            Id = userId,
            IssueCategory = issueCategory,
            SessionStatus = "Open",
            StartedAt = DateTime.UtcNow
        };
        return chatRepository.Create(session);
    }

    public bool UpdateSessionStatus(int sessionId, string status)
    {
        return chatRepository.UpdateStatus(sessionId, status);
    }

    public bool SaveSessionFeedback(int sessionId, int rating, string feedback)
    {
        return chatRepository.SaveFeedback(sessionId, rating, feedback);
    }

    public List<ChatMessage> GetMessagesBySessionId(int sessionId)
    {
        return chatMessageRepository.GetBySessionId(sessionId);
    }

    public int CreateMessage(int sessionId, string senderType, string content)
    {
        var message = new ChatMessage
        {
            Id = sessionId,
            SenderType = senderType,
            Content = content,
            SentAt = DateTime.UtcNow
        };
        return chatMessageRepository.Create(message);
    }

    public List<ChatAttachment> GetAttachmentsByMessageId(int messageId)
    {
        return chatMessageRepository.GetAttachmentsByMessageId(messageId);
    }

    public int CreateAttachment(int messageId, string attachmentName, string fileType, int fileSizeBytes, string storageUrl)
    {
        var attachment = new ChatAttachment
        {
            Id = messageId,
            AttachmentName = attachmentName,
            FileType = fileType,
            FileSizeBytes = fileSizeBytes,
            StorageUrl = storageUrl
        };
        return chatMessageRepository.CreateAttachment(attachment);
    }
}
