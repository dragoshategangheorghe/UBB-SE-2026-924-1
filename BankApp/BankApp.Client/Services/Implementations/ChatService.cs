using System.Collections.Generic;
using System.Threading.Tasks;
using BankApp.Client.RepoProxies.Interfaces;
using BankApp.Client.Services.Interfaces;
using BankApp.Models.DTOs.Chat;
using BankApp.Models.Features.Chat;

namespace BankApp.Client.Services.Implementations
{
    public class ChatService : IChatService
    {
        private readonly IChatRepoProxy _repoProxy;

        public ChatService(IChatRepoProxy repoProxy)
        {
            _repoProxy = repoProxy;
        }

        public Task<List<ChatSession>?> GetSessionsAsync() => _repoProxy.GetSessionsAsync();
        public Task<ChatSession?> GetSessionAsync(int sessionId) => _repoProxy.GetSessionAsync(sessionId);
        public Task<CreateChatSessionResponse?> CreateSessionAsync(string issueCategory) => _repoProxy.CreateSessionAsync(issueCategory);
        public async Task<bool> UpdateSessionStatusAsync(int sessionId, string status)
        {
            var result = await _repoProxy.UpdateSessionStatusAsync(sessionId, status);
            return result?.Success == true;
        }
        public async Task<bool> SaveFeedbackAsync(int sessionId, int rating, string feedback)
        {
            var result = await _repoProxy.SaveFeedbackAsync(sessionId, rating, feedback);
            return result?.Success == true;
        }
        public Task<List<ChatMessage>?> GetMessagesAsync(int sessionId) => _repoProxy.GetMessagesAsync(sessionId);
        public Task<CreateChatMessageResponse?> CreateMessageAsync(int sessionId, string senderType, string content) => _repoProxy.CreateMessageAsync(sessionId, senderType, content);
        public Task<CreateChatAttachmentResponse?> CreateAttachmentAsync(int messageId, CreateChatAttachmentRequest request) => _repoProxy.CreateAttachmentAsync(messageId, request);
        public async Task<bool> EmailTranscriptAsync(int sessionId, string email)
        {
            var result = await _repoProxy.EmailTranscriptAsync(sessionId, email);
            return result?.Success == true;
        }
    }
}

