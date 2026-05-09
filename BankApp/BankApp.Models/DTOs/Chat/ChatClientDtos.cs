namespace BankApp.Models.DTOs.Chat
{
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
