using System.Collections.Generic;
using System.Threading.Tasks;
using BankApp.Models.Features.Chat;

namespace BankApp.Client.Services.Interfaces
{
    public interface IChatApiService
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

    public class CreateChatSessionResponse
    {
        public bool Success { get; set; }
        public int SessionId { get; set; }
    }

    public class CreateChatMessageResponse
    {
        public bool Success { get; set; }
        public int MessageId { get; set; }
    }

    public class CreateChatAttachmentRequest
    {
        public string AttachmentName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public int FileSizeBytes { get; set; }
        public string StorageUrl { get; set; } = string.Empty;
    }

    public class CreateChatAttachmentResponse
    {
        public bool Success { get; set; }
        public int AttachmentId { get; set; }
    }

    public class OperationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
