using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApp.Models.Features.Chat.Requests
{
    public class CreateChatMessageRequest
    {
        public string SenderType { get; set; } = "User";
        public string Content { get; set; } = string.Empty;
    }
}
