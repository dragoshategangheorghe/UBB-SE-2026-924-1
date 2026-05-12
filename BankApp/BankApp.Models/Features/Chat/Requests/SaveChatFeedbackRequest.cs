using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApp.Models.Features.Chat.Requests
{
    public class SaveChatFeedbackRequest
    {
        public int Rating { get; set; }
        public string? Feedback { get; set; }
    }
}
