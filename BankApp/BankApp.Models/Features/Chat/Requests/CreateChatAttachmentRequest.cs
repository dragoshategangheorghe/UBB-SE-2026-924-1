using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApp.Models.Features.Chat.Requests
{
    public class CreateChatAttachmentRequest
    {
        public string AttachmentName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public int FileSizeBytes { get; set; }
        public string? StorageUrl { get; set; }
    }
}
