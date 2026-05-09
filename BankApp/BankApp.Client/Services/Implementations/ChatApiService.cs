using BankApp.Client.Services.Interfaces;
using BankApp.Client.Utilities;
using BankApp.Models.Features.Chat;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankApp.Client.Services.Implementations
{
    public class ChatApiService : IChatApiService
    {
        private readonly ApiService _apiService;

        public ChatApiService(ApiService apiService)
        {
            this._apiService = apiService;
        }

        public Task<List<ChatSession>?> GetSessionsAsync()
        {
            return _apiService.GetAsync<List<ChatSession>>("/api/chat/sessions");
        }

        public Task<ChatSession?> GetSessionAsync(int sessionId)
        {
            return _apiService.GetAsync<ChatSession>($"/api/chat/sessions/{sessionId}");
        }

        public Task<CreateChatSessionResponse?> CreateSessionAsync(string issueCategory)
        {
            return _apiService.PostAsync<object, CreateChatSessionResponse>("/api/chat/sessions", new { issueCategory });
        }

        public Task<List<ChatMessage>?> GetMessagesAsync(int sessionId)
        {
            return _apiService.GetAsync<List<ChatMessage>>($"/api/chat/sessions/{sessionId}/messages");
        }

        public Task<CreateChatMessageResponse?> CreateMessageAsync(int sessionId, string senderType, string content)
        {
            return _apiService.PostAsync<object, CreateChatMessageResponse>(
                $"/api/chat/sessions/{sessionId}/messages",
                new { senderType, content });
        }

        public Task<CreateChatAttachmentResponse?> CreateAttachmentAsync(int messageId, CreateChatAttachmentRequest request)
        {
            return _apiService.PostAsync<CreateChatAttachmentRequest, CreateChatAttachmentResponse>(
                $"/api/chat/messages/{messageId}/attachments",
                request);
        }

        public Task<OperationResponse?> UpdateSessionStatusAsync(int sessionId, string status)
        {
            return _apiService.PutAsync<object, OperationResponse>($"/api/chat/sessions/{sessionId}/status", new { status });
        }

        public Task<OperationResponse?> SaveFeedbackAsync(int sessionId, int rating, string feedback)
        {
            return _apiService.PostAsync<object, OperationResponse>($"/api/chat/sessions/{sessionId}/feedback", new { rating, feedback });
        }

        public Task<OperationResponse?> EmailTranscriptAsync(int sessionId, string email)
        {
            return _apiService.PostAsync<object, OperationResponse>($"/api/chat/sessions/{sessionId}/transcript/email", new { email });
        }
    }
}
