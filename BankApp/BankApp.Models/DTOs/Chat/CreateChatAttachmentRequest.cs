namespace BankApp.Models.DTOs.Chat
{
    public class CreateChatAttachmentRequest
    {
        public string AttachmentName { get; set; } = string.Empty;

        public string FileType { get; set; } = string.Empty;

        public int FileSizeBytes { get; set; }

        public string StorageUrl { get; set; } = string.Empty;
    }
}
