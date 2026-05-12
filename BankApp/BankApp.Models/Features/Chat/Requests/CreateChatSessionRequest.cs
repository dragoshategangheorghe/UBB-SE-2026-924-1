using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApp.Models.Features.Chat.Requests
{
    public class CreateChatSessionRequest
    {
        public string IssueCategory { get; set; } = string.Empty;
    }
}
