using BankApp.Models.Features.Chat;
using BankApp.Server.Repositories.Implementations;
using BankApp.Server.Repositories.Interfaces;
using BankApp.Server.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic.FileIO;
using System.Net.Mail;
using System.Text;

namespace BankApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private const int MaxAttachmentSizeBytes = 10 * 1024 * 1024;
        protected static readonly Dictionary<string, string> DefaultChatbotResponses = new Dictionary<string, string>
        {
            ["How do I reset my password?"] =
                "You can reset your password from the login screen by choosing Forgot password and following the verification steps.",
            ["Why was my card declined?"] =
                "A card can be declined because of insufficient funds, an expired card, a blocked card, or a merchant validation issue. Please check the card status in the app first.",
            ["How long does a transfer take?"] =
                "Internal transfers are usually immediate, while interbank transfers can take up to one business day depending on the destination bank.",
            ["How do I upload documents for support?"] =
                "Use the Attach File button in this chat after contacting the team. Your selected file will be included with the support request summary.",
            ["I found a technical problem in the app."] =
                "Please contact the team from this chat and include a short description of what happened. Screenshots or PDFs can help the team investigate faster.",
        };

        private readonly IChatRepository _chatRepository;
        private readonly ChatMessageRepository _chatMessageRepository;

        public ChatController(IChatRepository chatRepository, ChatMessageRepository chatMessageRepository)
        {
            this._chatRepository = chatRepository;
            this._chatMessageRepository = chatMessageRepository;
        }

        private int GetAuthenticatedUserId() => (int)this.HttpContext.Items["UserId"] !;

        [HttpGet("sessions")]
        public IActionResult GetSessions()
        {
            return Ok(_chatRepository.GetByUserId(GetAuthenticatedUserId()));
        }

        [HttpGet("sessions/{sessionId:int}")]
        public ActionResult<ChatSession> GetSession(int sessionId)
        {
            return _chatRepository.GetById(sessionId);
        }

        [HttpPost("sessions")]
        public IActionResult CreateSession([FromBody] CreateChatSessionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.IssueCategory))
            {
                return BadRequest(new { message = "Issue category is required." });
            }

            int sessionId = _chatRepository.Create(new ChatSession
            {
                IssueCategory = request.IssueCategory,
                SessionStatus = "Open",
                StartedAt = DateTime.UtcNow,
                Id = GetAuthenticatedUserId()
            });
            return sessionId > 0 ? Ok(new { success = true, sessionId }) : BadRequest(new { success = false });
        }

        [HttpPut("sessions/{sessionId:int}/status")]
        public IActionResult UpdateStatus(int sessionId, [FromBody] UpdateChatSessionStatusRequest request)
        {
            ChatSession? session = _chatRepository.GetById(sessionId);
            if (session == null || session.Id != GetAuthenticatedUserId())
            {
                return NotFound();
            }

            bool success = _chatRepository.UpdateStatus(sessionId, request.Status);
            return success ? Ok(new { success = true }) : BadRequest(new { success = false });
        }

        [HttpPost("sessions/{sessionId:int}/feedback")]
        public IActionResult SaveFeedback(int sessionId, [FromBody] SaveChatFeedbackRequest request)
        {
            ChatSession? session = _chatRepository.GetById(sessionId);
            if (session == null || session.Id != GetAuthenticatedUserId())
            {
                return NotFound();
            }

            bool success = _chatRepository.SaveFeedback(sessionId, request.Rating, request.Feedback ?? string.Empty);
            return success ? Ok(new { success = true }) : BadRequest(new { success = false });
        }

        [HttpGet("sessions/{sessionId:int}/messages")]
        public IActionResult GetMessages(int sessionId)
        {
            ChatSession? session = _chatRepository.GetById(sessionId);
            if (session == null || session.Id != GetAuthenticatedUserId())
            {
                return NotFound();
            }

            return Ok(_chatMessageRepository.GetBySessionId(sessionId));
        }

        [HttpPost("sessions/{sessionId:int}/messages")]
        public IActionResult CreateMessage(int sessionId, [FromBody] CreateChatMessageRequest request)
        {
            ChatSession? session = _chatRepository.GetById(sessionId);
            if (session == null || session.Id != GetAuthenticatedUserId())
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(request.Content))
            {
                return BadRequest(new { message = "Message content is required." });
            }

            string senderType = string.IsNullOrWhiteSpace(request.SenderType) ? "User" : request.SenderType;

            // In chatbot mode, user input is restricted to preset questions only.
            if (senderType.Equals("User", StringComparison.OrdinalIgnoreCase) &&
                !session.SessionStatus.Equals("Escalated", StringComparison.OrdinalIgnoreCase))
            {
                bool isPresetQuestion = DefaultChatbotResponses.Keys
                    .Any(question => request.Content.Trim().Equals(question, StringComparison.OrdinalIgnoreCase));
                if (!isPresetQuestion)
                {
                    return BadRequest(new { message = "Only preset chatbot questions are allowed in chatbot mode." });
                }
            }

            var message = new ChatMessage
            {
                Id = sessionId,
                SenderType = senderType,
                Content = request.Content,
                SentAt = DateTime.UtcNow
            };

            int messageId = _chatMessageRepository.Create(message);
            if (messageId <= 0)
            {
                return BadRequest(new { success = false });
            }

            // Automated support (chatbot) before live agent.
            if (senderType.Equals("User", StringComparison.OrdinalIgnoreCase) &&
                !session.SessionStatus.Equals("Escalated", StringComparison.OrdinalIgnoreCase) &&
                !session.SessionStatus.Equals("Closed", StringComparison.OrdinalIgnoreCase))
            {
                string botReply = BuildBotReply(request.Content, out bool shouldEscalate);

                message = new ChatMessage
                {
                    Id = sessionId,
                    SenderType = "Bot",
                    Content = botReply,
                    SentAt = DateTime.UtcNow
                };

                _chatMessageRepository.Create(message);

                if (shouldEscalate)
                {
                    _chatRepository.UpdateStatus(sessionId, "Escalated");
                    message = new ChatMessage
                    {
                        Id = sessionId,
                        SenderType = "System",
                        Content = request.Content,
                        SentAt = DateTime.UtcNow
                    };
                    _chatMessageRepository.Create(message);
                }
            }

            return Ok(new { success = true, messageId });
        }

        [HttpPost("messages/{messageId:int}/attachments")]
        public IActionResult CreateAttachment(int messageId, [FromBody] CreateChatAttachmentRequest request)
        {
            if (request.FileSizeBytes <= 0 || request.FileSizeBytes > MaxAttachmentSizeBytes)
            {
                return BadRequest(new { message = "Attachment must be between 1 byte and 10 MB." });
            }

            if (!IsSupportedAttachmentType(request.FileType, request.AttachmentName))
            {
                return BadRequest(new { message = "Only image and PDF attachments are supported." });
            }

            var chatAttachment = new ChatAttachment
            {
                Id = messageId,
                AttachmentName = request.AttachmentName,
                FileType = request.FileType,
                FileSizeBytes = request.FileSizeBytes,
                StorageUrl = request.StorageUrl
            };

            int attachmentId = _chatMessageRepository.CreateAttachment(chatAttachment);

            return attachmentId > 0 ? Ok(new { success = true, attachmentId }) : BadRequest(new { success = false });
        }

        [HttpPost("sessions/{sessionId:int}/transcript/email")]
        public IActionResult EmailTranscript(int sessionId, [FromBody] EmailTranscriptRequest request)
        {
            ChatSession? session = _chatRepository.GetById(sessionId);
            if (session == null || session.Id != GetAuthenticatedUserId())
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new { message = "A destination email is required." });
            }

            List<ChatMessage> messages = _chatMessageRepository.GetBySessionId(sessionId);
            string transcript = BuildTranscript(session, messages);
            // In this implementation the transcript is prepared and ready for outbound email integration.
            return Ok(new { success = true, message = "Transcript prepared for email delivery.", transcriptLength = transcript.Length });
        }

        public class CreateChatSessionRequest
        {
            public string IssueCategory { get; set; } = string.Empty;
        }

        public class UpdateChatSessionStatusRequest
        {
            public string Status { get; set; } = string.Empty;
        }

        public class SaveChatFeedbackRequest
        {
            public int Rating { get; set; }
            public string? Feedback { get; set; }
        }

        public class CreateChatMessageRequest
        {
            public string SenderType { get; set; } = "User";
            public string Content { get; set; } = string.Empty;
        }

        public class CreateChatAttachmentRequest
        {
            public string AttachmentName { get; set; } = string.Empty;
            public string FileType { get; set; } = string.Empty;
            public int FileSizeBytes { get; set; }
            public string? StorageUrl { get; set; }
        }

        public class EmailTranscriptRequest
        {
            public string Email { get; set; } = string.Empty;
        }

        private static bool IsSupportedAttachmentType(string fileType, string attachmentName)
        {
            string lowerType = fileType.ToLowerInvariant();
            if (lowerType.Contains("pdf") || lowerType.Contains("image"))
            {
                return true;
            }

            string lowerName = attachmentName.ToLowerInvariant();
            return lowerName.EndsWith(".pdf") || lowerName.EndsWith(".png") || lowerName.EndsWith(".jpg") || lowerName.EndsWith(".jpeg");
        }

        private static string BuildBotReply(string userMessage, out bool shouldEscalate)
        {
        foreach (KeyValuePair<string, string> pair in DefaultChatbotResponses)
        {
            if (userMessage.Trim().Equals(pair.Key, StringComparison.OrdinalIgnoreCase))
            {
                shouldEscalate = false;
                return pair.Value;
            }
        }

            string lower = userMessage.ToLowerInvariant();
            shouldEscalate = false;

            if (lower.Contains("balance"))
            {
                return "For balance checks, open Dashboard where account balances and recent activity are shown.";
            }

            if (lower.Contains("freeze") || lower.Contains("card"))
            {
                return "For card actions, open Cards and use Freeze/Unfreeze or Settings for spending controls.";
            }

            if (lower.Contains("transfer") || lower.Contains("transaction"))
            {
                return "For transfer or transaction status, open Transfer History and filter by date/status to locate the operation.";
            }

            if (lower.Contains("agent") || lower.Contains("human") || lower.Contains("support"))
            {
                shouldEscalate = true;
                return "Understood. I will escalate this chat to a live support agent.";
            }

            shouldEscalate = true;
            return "I can help with common banking questions, but this one needs live support. Escalating now.";
        }

        private static string BuildTranscript(ChatSession session, IReadOnlyCollection<ChatMessage> messages)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"Session #{session.Id} ({session.IssueCategory})");
            builder.AppendLine($"Status: {session.SessionStatus}");
            builder.AppendLine($"Started: {session.StartedAt:u}");
            builder.AppendLine();

            foreach (ChatMessage message in messages)
            {
                builder.AppendLine($"[{message.SentAt:u}] {message.SenderType}: {message.Content}");
            }

            return builder.ToString();
        }
    }
}
