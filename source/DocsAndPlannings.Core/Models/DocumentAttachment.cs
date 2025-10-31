using System.ComponentModel.DataAnnotations;

namespace DocsAndPlannings.Core.Models;

/// <summary>
/// Represents a file attachment for a document
/// </summary>
public class DocumentAttachment
{
    /// <summary>
    /// Gets or sets the unique identifier of the attachment
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the document ID this attachment belongs to
    /// </summary>
    public int DocumentId { get; set; }

    /// <summary>
    /// Gets or sets the document this attachment belongs to
    /// </summary>
    public Document Document { get; set; } = null!;

    /// <summary>
    /// Gets or sets the original filename
    /// </summary>
    [MaxLength(255)]
    public required string FileName { get; set; }

    /// <summary>
    /// Gets or sets the stored filename (with unique identifier)
    /// </summary>
    [MaxLength(255)]
    public required string StoredFileName { get; set; }

    /// <summary>
    /// Gets or sets the MIME type of the file
    /// </summary>
    [MaxLength(100)]
    public required string ContentType { get; set; }

    /// <summary>
    /// Gets or sets the file size in bytes
    /// </summary>
    public long FileSizeBytes { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the attachment was uploaded
    /// </summary>
    public DateTime UploadedAt { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user who uploaded the attachment
    /// </summary>
    public int UploadedById { get; set; }

    /// <summary>
    /// Gets or sets the user who uploaded the attachment
    /// </summary>
    public User UploadedBy { get; set; } = null!;
}
