// <copyright file="ChatAttachment.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

namespace BankApp.Models.Features.Chat;

/// <summary>
/// Represents an attachment linked to a chat message.
/// </summary>
public class ChatAttachment
{
    /// <summary>
    /// Gets or sets the unique attachment identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the related message identifier.
    /// </summary>
    public int MessageId { get; set; }

    /// <summary>
    /// Gets or sets the original attachment file name.
    /// </summary>
    public string AttachmentName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the attachment content type.
    /// </summary>
    public string FileType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file size in bytes.
    /// </summary>
    public int FileSizeBytes { get; set; }

    /// <summary>
    /// Gets or sets the persisted storage location URL.
    /// </summary>
    public string StorageUrl { get; set; } = string.Empty;
}