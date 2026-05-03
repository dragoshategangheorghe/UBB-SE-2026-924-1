// <copyright file="SelectedAttachment.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

namespace BankApp.Models.Features.Chat;

/// <summary>
/// Represents a locally selected file before chat attachment upload.
/// </summary>
public class SelectedAttachment
{
    private const long BytesPerKilobyte = 1024;
    private const long BytesPerMegabyte = BytesPerKilobyte * BytesPerKilobyte;
    private const string SizePrecisionFormat = "F2";

    /// <summary>
    /// Gets or sets the original file name.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the absolute local file path.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the content/MIME type.
    /// </summary>
    public string FileType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file size in bytes.
    /// </summary>
    public long FileSizeBytes { get; set; }

    /// <summary>
    /// Gets a human-readable size label derived from <see cref="FileSizeBytes"/>.
    /// </summary>
    public string FileSizeDisplay
    {
        get
        {
            if (this.FileSizeBytes < BytesPerKilobyte)
            {
                return $"{this.FileSizeBytes} B";
            }

            if (this.FileSizeBytes < BytesPerMegabyte)
            {
                double sizeInKilobytes = this.FileSizeBytes / (double)BytesPerKilobyte;
                return $"{sizeInKilobytes.ToString(SizePrecisionFormat)} KB";
            }

            double sizeInMegabytes = this.FileSizeBytes / (double)BytesPerMegabyte;
            return $"{sizeInMegabytes.ToString(SizePrecisionFormat)} MB";
        }
    }
}