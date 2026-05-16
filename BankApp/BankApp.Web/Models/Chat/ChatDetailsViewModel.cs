using BankApp.Models.Features.Chat;

namespace BankApp.Web.Models.Chat
{
    public class ChatDetailsViewModel
    {

        public ChatSession Session {  get; set; }
        public List<ChatMessage> Messages { get; set; } = new();
        public string? NewMessageContent { get; set; }

        public List<string> PresetQuestions { get; set; } = new()
        {
            "How do I reset my password?",
            "Why was my card declined?",
            "How long does a transfer take?",
            "How do I upload documents for support?",
            "I found a technical problem in the app."
        };
    }
}
