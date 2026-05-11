using System.Collections.Generic;
using System.Threading.Tasks;
using BankApp.Models.DTOs.Chat;
using BankApp.Models.Features.Chat;

namespace BankApp.Client.RepoProxies.Interfaces
{
    public interface IChatRepoProxy
    {
        Task<List<ChatSession>?> GetSessionsAsync();

        Task<ChatSession?> GetSessionAsync(int sessionId);

        Task<CreateChatSessionResponse?> CreateSessionAsync(string issueCategory);

        Task<List<ChatMessage>?> GetMessagesAsync(int sessionId);

        Task<CreateChatMessageResponse?> CreateMessageAsync(int sessionId, string senderType, string content);

        Task<CreateChatAttachmentResponse?> CreateAttachmentAsync(int messageId, CreateChatAttachmentRequest request);

        Task<OperationResponse?> UpdateSessionStatusAsync(int sessionId, string status);

        Task<OperationResponse?> SaveFeedbackAsync(int sessionId, int rating, string feedback);

        Task<OperationResponse?> EmailTranscriptAsync(int sessionId, string email);
    }
}
