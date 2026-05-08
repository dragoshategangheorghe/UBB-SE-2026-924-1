using BankApp.Models.Features.Chat;

namespace BankApp.Server.DataAccess.Interfaces;

public interface IChatMessageDAO
{
    List<ChatMessage> GetBySessionId(int sessionId);
    int Create(ChatMessage message);
    List<ChatAttachment> GetAttachmentsByMessageId(int messageId);
    int CreateAttachment(ChatAttachment attachment);
}
