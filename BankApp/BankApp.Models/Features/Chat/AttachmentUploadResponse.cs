// <copyright file="AttachmentUploadResponse.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

namespace BankApp.Models.Features.Chat;

/// <summary>
/// Represents metadata returned after uploading a chat attachment.
/// </summary>
public class AttachmentUploadResponse
{
    /// <summary>
    /// Gets or sets the uploaded attachment identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the related message identifier.
    /// </summary>
    public int MessageId { get; set; }

    /// <summary>
    /// Gets or sets the original file name.
    /// </summary>
    public string AttachmentName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the uploaded MIME/content type.
    /// </summary>
    public string FileType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file size in bytes.
    /// </summary>
    public int FileSizeBytes { get; set; }

    /// <summary>
    /// Gets or sets the storage URL of the uploaded file.
    /// </summary>
    public string StorageUrl { get; set; } = string.Empty;
}