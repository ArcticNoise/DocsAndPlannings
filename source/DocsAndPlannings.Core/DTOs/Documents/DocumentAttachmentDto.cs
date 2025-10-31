namespace DocsAndPlannings.Core.DTOs.Documents;

/// <summary>
/// Data transfer object for a document attachment
/// </summary>
public sealed record DocumentAttachmentDto
{
    /// <summary>
    /// Gets the unique identifier of the attachment
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets the document ID this attachment belongs to
    /// </summary>
    public required int DocumentId { get; init; }

    /// <summary>
    /// Gets the original filename
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// Gets the MIME type of the file
    /// </summary>
    public required string ContentType { get; init; }

    /// <summary>
    /// Gets the file size in bytes
    /// </summary>
    public required long FileSizeBytes { get; init; }

    /// <summary>
    /// Gets the timestamp when the attachment was uploaded
    /// </summary>
    public required DateTime UploadedAt { get; init; }

    /// <summary>
    /// Gets the name of the user who uploaded the attachment
    /// </summary>
    public required string UploadedByName { get; init; }
}
