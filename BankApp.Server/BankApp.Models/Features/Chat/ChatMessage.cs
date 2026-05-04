// <copyright file="ChatMessage.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

namespace BankApp.Models.Features.Chat;

using System;

/// <summary>
/// Represents a message belonging to a support chat session.
/// </summary>
public class ChatMessage
{
    /// <summary>
    /// Gets or sets the unique message identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the parent session identifier.
    /// </summary>
    public int SessionId { get; set; }

    /// <summary>
    /// Gets or sets the sender role (for example user or bot).
    /// </summary>
    public string SenderType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the textual message body.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the UTC timestamp when the message was sent.
    /// </summary>
    public DateTime SentAt { get; set; }

    /// <summary>
    /// Gets the human-readable formatted sent timestamp.
    /// </summary>
    public string DisplaySentAt => this.SentAt.ToString("g");
}