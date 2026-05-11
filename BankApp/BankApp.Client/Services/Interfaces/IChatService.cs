using System.Collections.Generic;
using System.Threading.Tasks;
using BankApp.Models.DTOs.Chat;
using BankApp.Models.Features.Chat;

namespace BankApp.Client.Services.Interfaces
{
    public interface IChatService
    {
        Task<List<ChatSession>?> GetSessionsAsync();
        Task<ChatSession?> GetSessionAsync(int sessionId);
        Task<CreateChatSessionResponse?> CreateSessionAsync(string issueCategory);
        Task<bool> UpdateSessionStatusAsync(int sessionId, string status);
        Task<bool> SaveFeedbackAsync(int sessionId, int rating, string feedback);
        Task<List<ChatMessage>?> GetMessagesAsync(int sessionId);
        Task<CreateChatMessageResponse?> CreateMessageAsync(int sessionId, string senderType, string content);
        Task<CreateChatAttachmentResponse?> CreateAttachmentAsync(int messageId, CreateChatAttachmentRequest request);
        Task<bool> EmailTranscriptAsync(int sessionId, string email);
    }
}

