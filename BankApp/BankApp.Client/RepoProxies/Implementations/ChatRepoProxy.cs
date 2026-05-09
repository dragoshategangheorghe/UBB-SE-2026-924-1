using System.Collections.Generic;
using System.Threading.Tasks;
using BankApp.Client.RepoProxies;
using BankApp.Client.RepoProxies.Interfaces;
using BankApp.Models.DTOs.Chat;
using BankApp.Models.Features.Chat;

namespace BankApp.Client.RepoProxies.Implementations
{
    public class ChatRepoProxy : IChatRepoProxy
    {
        private readonly ApiService apiService;

        public ChatRepoProxy(ApiService apiService)
        {
            this.apiService = apiService;
        }

        public Task<List<ChatSession>?> GetSessionsAsync()
        {
            return apiService.GetAsync<List<ChatSession>>("/api/chat/sessions");
        }

        public Task<ChatSession?> GetSessionAsync(int sessionId)
        {
            return apiService.GetAsync<ChatSession>($"/api/chat/sessions/{sessionId}");
        }

        public Task<CreateChatSessionResponse?> CreateSessionAsync(string issueCategory)
        {
            return apiService.PostAsync<object, CreateChatSessionResponse>("/api/chat/sessions", new { issueCategory });
        }

        public Task<List<ChatMessage>?> GetMessagesAsync(int sessionId)
        {
            return apiService.GetAsync<List<ChatMessage>>($"/api/chat/sessions/{sessionId}/messages");
        }

        public Task<CreateChatMessageResponse?> CreateMessageAsync(int sessionId, string senderType, string content)
        {
            return apiService.PostAsync<object, CreateChatMessageResponse>(
                $"/api/chat/sessions/{sessionId}/messages",
                new { senderType, content });
        }

        public Task<CreateChatAttachmentResponse?> CreateAttachmentAsync(int messageId, CreateChatAttachmentRequest request)
        {
            return apiService.PostAsync<CreateChatAttachmentRequest, CreateChatAttachmentResponse>(
                $"/api/chat/messages/{messageId}/attachments",
                request);
        }

        public Task<OperationResponse?> UpdateSessionStatusAsync(int sessionId, string status)
        {
            return apiService.PutAsync<object, OperationResponse>($"/api/chat/sessions/{sessionId}/status", new { status });
        }

        public Task<OperationResponse?> SaveFeedbackAsync(int sessionId, int rating, string feedback)
        {
            return apiService.PostAsync<object, OperationResponse>($"/api/chat/sessions/{sessionId}/feedback", new { rating, feedback });
        }

        public Task<OperationResponse?> EmailTranscriptAsync(int sessionId, string email)
        {
            return apiService.PostAsync<object, OperationResponse>($"/api/chat/sessions/{sessionId}/transcript/email", new { email });
        }
    }
}
