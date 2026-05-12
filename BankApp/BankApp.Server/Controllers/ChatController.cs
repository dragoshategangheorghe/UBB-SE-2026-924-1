using System.Text;
using BankApp.Models.Entities;
using BankApp.Models.Features.Chat;
using BankApp.Models.Features.Chat.Requests;
using BankApp.Server.Repositories.Implementations;
using BankApp.Server.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private const int TenMB = 10 * 1024 * 1024;
        private const int MaxAttachmentSizeBytes = TenMB;

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

        private readonly IChatRepository chatRepository;
        private readonly ChatMessageRepository chatMessageRepository;
        private readonly IUserRepository userRepository;

        public ChatController(
            IChatRepository chatRepository,
            ChatMessageRepository chatMessageRepository,
            IUserRepository userRepository)
        {
            this.chatRepository = chatRepository;
            this.chatMessageRepository = chatMessageRepository;
            this.userRepository = userRepository;
        }

        private int GetAuthenticatedUserId() => (int)HttpContext.Items["UserId"] !;

        private bool IsMissingOrNotOwnedSession(ChatSession? session)
        {
            int userId = GetAuthenticatedUserId();
            return session == null || session.User?.Id != userId;
        }

        [HttpGet("sessions")]
        public IActionResult GetSessions()
        {
            return Ok(chatRepository.GetByUserId(GetAuthenticatedUserId()));
        }

        [HttpGet("sessions/{sessionId:int}")]
        public IActionResult GetSession(int sessionId)
        {
            ChatSession? session = chatRepository.GetById(sessionId);
            if (IsMissingOrNotOwnedSession(session))
            {
                return NotFound();
            }

            return Ok(session);
        }

        [HttpPost("sessions")]
        public IActionResult CreateSession([FromBody] CreateChatSessionRequest? request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.IssueCategory))
            {
                return BadRequest(new { message = "Issue category is required." });
            }

            int userId = GetAuthenticatedUserId();
            User? user = userRepository.FindById(userId);
            if (user == null)
            {
                return Unauthorized();
            }

            var session = new ChatSession
            {
                User = user,
                IssueCategory = request.IssueCategory,
                SessionStatus = "Open",
                StartedAt = DateTime.UtcNow
            };
            int sessionId = chatRepository.Create(session);
            return sessionId > 0 ? Ok(new { success = true, sessionId }) : BadRequest(new { success = false });
        }

        [HttpPut("sessions/{sessionId:int}/status")]
        public IActionResult UpdateStatus(int sessionId, [FromBody] UpdateChatSessionStatusRequest request)
        {
            ChatSession? session = chatRepository.GetById(sessionId);
            if (IsMissingOrNotOwnedSession(session))
            {
                return NotFound();
            }

            bool success = chatRepository.UpdateStatus(sessionId, request.Status);
            return success ? Ok(new { success = true }) : BadRequest(new { success = false });
        }

        [HttpPost("sessions/{sessionId:int}/feedback")]
        public IActionResult SaveFeedback(int sessionId, [FromBody] SaveChatFeedbackRequest request)
        {
            ChatSession? session = chatRepository.GetById(sessionId);
            if (IsMissingOrNotOwnedSession(session))
            {
                return NotFound();
            }

            bool success = chatRepository.SaveFeedback(sessionId, request.Rating, request.Feedback ?? string.Empty);
            return success ? Ok(new { success = true }) : BadRequest(new { success = false });
        }

        [HttpGet("sessions/{sessionId:int}/messages")]
        public IActionResult GetMessages(int sessionId)
        {
            ChatSession? session = chatRepository.GetById(sessionId);
            if (IsMissingOrNotOwnedSession(session))
            {
                return NotFound();
            }

            return Ok(chatMessageRepository.GetBySessionId(sessionId));
        }

        [HttpPost("sessions/{sessionId:int}/messages")]
        public IActionResult CreateMessage(int sessionId, [FromBody] CreateChatMessageRequest? request)
        {
            ChatSession? session = chatRepository.GetById(sessionId);
            if (IsMissingOrNotOwnedSession(session))
            {
                return NotFound();
            }

            ChatSession ownedSession = session!;

            if (request == null || string.IsNullOrWhiteSpace(request.Content))
            {
                return BadRequest(new { message = "Message content is required." });
            }

            string senderType = string.IsNullOrWhiteSpace(request.SenderType) ? "User" : request.SenderType;

            if (senderType.Equals("User", StringComparison.OrdinalIgnoreCase) &&
                !ownedSession.SessionStatus.Equals("Escalated", StringComparison.OrdinalIgnoreCase))
            {
                bool isPresetQuestion = DefaultChatbotResponses.Keys
                    .Any(question => request.Content.Trim().Equals(question, StringComparison.OrdinalIgnoreCase));
                if (!isPresetQuestion)
                {
                    return BadRequest(new { message = "Only preset chatbot questions are allowed in chatbot mode." });
                }
            }

            int messageId = chatMessageRepository.Create(new ChatMessage
            {
                SessionId = sessionId,
                SenderType = senderType,
                Content = request.Content,
                SentAt = DateTime.UtcNow
            });
            if (messageId <= 0)
            {
                return BadRequest(new { success = false });
            }

            if (senderType.Equals("User", StringComparison.OrdinalIgnoreCase) &&
                !ownedSession.SessionStatus.Equals("Escalated", StringComparison.OrdinalIgnoreCase) &&
                !ownedSession.SessionStatus.Equals("Closed", StringComparison.OrdinalIgnoreCase))
            {
                string botReply = BuildBotReply(request.Content, out bool shouldEscalate);
                chatMessageRepository.Create(new ChatMessage
                {
                    SessionId = sessionId,
                    SenderType = "Bot",
                    Content = botReply,
                    SentAt = DateTime.UtcNow
                });

                if (shouldEscalate)
                {
                    chatRepository.UpdateStatus(sessionId, "Escalated");
                    chatMessageRepository.Create(new ChatMessage
                    {
                        SessionId = sessionId,
                        SenderType = "System",
                        Content = "I could not fully resolve this request. You have been escalated to a support agent and your chat context is preserved.",
                        SentAt = DateTime.UtcNow
                    });
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

            int attachmentId = chatMessageRepository.CreateAttachment(new ChatAttachment
            {
                MessageId = messageId,
                AttachmentName = request.AttachmentName,
                FileType = request.FileType,
                FileSizeBytes = request.FileSizeBytes,
                StorageUrl = request.StorageUrl ?? string.Empty
            });

            return attachmentId > 0 ? Ok(new { success = true, attachmentId }) : BadRequest(new { success = false });
        }

        [HttpPost("sessions/{sessionId:int}/transcript/email")]
        public IActionResult EmailTranscript(int sessionId, [FromBody] EmailTranscriptRequest request)
        {
            ChatSession? session = chatRepository.GetById(sessionId);
            if (IsMissingOrNotOwnedSession(session))
            {
                return NotFound();
            }

            if (request == null || string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new { message = "A destination email is required." });
            }

            List<ChatMessage> messages = chatMessageRepository.GetBySessionId(sessionId);
            string transcript = BuildTranscript(session!, messages);
            // In this implementation the transcript is prepared and ready for outbound email integration.
            return Ok(new { success = true, message = "Transcript prepared for email delivery.", transcriptLength = transcript.Length });
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
