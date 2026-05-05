using BankApp.Models.Features.Chat;

namespace BankApp.Server.Services.Interfaces;

public interface IApiService
{
    List<ChatSession> GetSessionsByUserId(int userId);
    ChatSession? GetSessionById(int sessionId);
    int CreateSession(int userId, string issueCategory);
    bool UpdateSessionStatus(int sessionId, string status);
    bool SaveSessionFeedback(int sessionId, int rating, string feedback);
    List<ChatMessage> GetMessagesBySessionId(int sessionId);
    int CreateMessage(int sessionId, string senderType, string content);
    List<ChatAttachment> GetAttachmentsByMessageId(int messageId);
    int CreateAttachment(int messageId, string attachmentName, string fileType, int fileSizeBytes, string storageUrl);
}
