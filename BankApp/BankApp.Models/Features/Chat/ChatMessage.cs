namespace BankApp.Models.Features.Chat;

using System;

public class ChatMessage
{
    public int Id { get; set; }

    public int SessionId { get; set; }

    public virtual ChatSession Session { get; set; } = null!;

    public string SenderType { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTime SentAt { get; set; }

    public string DisplaySentAt => this.SentAt.ToString("g");
}