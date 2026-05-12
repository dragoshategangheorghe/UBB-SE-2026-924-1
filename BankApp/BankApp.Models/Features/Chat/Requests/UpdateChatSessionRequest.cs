using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApp.Models.Features.Chat.Requests
{
    public class UpdateChatSessionStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }
}
