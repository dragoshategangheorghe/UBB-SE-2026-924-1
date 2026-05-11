using BankApp.Client.RepoProxies.Interfaces;
using BankApp.Client.Services.Implementations;
using BankApp.Models.DTOs.Chat;

using Moq;
using System.Threading.Tasks;
using Xunit;

namespace BankApp.Client.Tests.Services
{
    public class ChatServiceTests
    {
        private readonly Mock<IChatRepoProxy> mockChatRepoProxy;
        private readonly ChatService chatService;

        public ChatServiceTests()
        {
            mockChatRepoProxy = new Mock<IChatRepoProxy>();
            chatService = new ChatService(mockChatRepoProxy.Object);
        }

        [Fact]
        public async Task UpdateSessionStatusAsync_SuccessfulUpdate_ReturnsTrue()
        {
            int targetSessionIdentification = 1;
            string newStatusLabel = "Closed";

            OperationResponse successfulModificationResponse = new OperationResponse { Success = true };

            mockChatRepoProxy.Setup(proxy => proxy.UpdateSessionStatusAsync(targetSessionIdentification, newStatusLabel))
                .ReturnsAsync(successfulModificationResponse);

            bool updateSuccessfulResult = await chatService.UpdateSessionStatusAsync(targetSessionIdentification, newStatusLabel);

            Assert.True(updateSuccessfulResult);
        }

        [Fact]
        public async Task SaveFeedbackAsync_FailedFeedbackSubmission_ReturnsFalse()
        {
            int targetSessionIdentification = 1;
            int providedRating = 5;
            string providedFeedbackText = "Great support";

            OperationResponse failedModificationResponse = new OperationResponse { Success = false };

            mockChatRepoProxy.Setup(proxy => proxy.SaveFeedbackAsync(targetSessionIdentification, providedRating, providedFeedbackText))
                .ReturnsAsync(failedModificationResponse);

            bool feedbackSuccessfulResult = await chatService.SaveFeedbackAsync(targetSessionIdentification, providedRating, providedFeedbackText);

            Assert.False(feedbackSuccessfulResult);
        }

        [Fact]
        public async Task EmailTranscriptAsync_NullResponseReturned_ReturnsFalse()
        {
            int targetSessionIdentification = 1;
            string destinationEmailAddress = "user@test.com";

            mockChatRepoProxy.Setup(proxy => proxy.EmailTranscriptAsync(targetSessionIdentification, destinationEmailAddress))
                .ReturnsAsync((OperationResponse?)null);

            bool emailSuccessfulResult = await chatService.EmailTranscriptAsync(targetSessionIdentification, destinationEmailAddress);

            Assert.False(emailSuccessfulResult);
        }
    }
}