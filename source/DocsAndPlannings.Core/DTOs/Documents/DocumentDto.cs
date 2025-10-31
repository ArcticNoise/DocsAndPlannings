namespace DocsAndPlannings.Core.DTOs.Documents;

/// <summary>
/// Data transfer object for a complete document with all details
/// </summary>
public sealed record DocumentDto
{
    /// <summary>
    /// Gets the unique identifier of the document
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets the title of the document
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Gets the markdown content of the document
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Gets the ID of the parent document, if this is a child document
    /// </summary>
    public int? ParentDocumentId { get; init; }

    /// <summary>
    /// Gets the ID of the author who created this document
    /// </summary>
    public required int AuthorId { get; init; }

    /// <summary>
    /// Gets the username of the author
    /// </summary>
    public required string AuthorName { get; init; }

    /// <summary>
    /// Gets the current version number of the document
    /// </summary>
    public required int CurrentVersion { get; init; }

    /// <summary>
    /// Gets the timestamp when the document was created
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Gets the timestamp when the document was last updated
    /// </summary>
    public required DateTime UpdatedAt { get; init; }

    /// <summary>
    /// Gets whether the document is published
    /// </summary>
    public required bool IsPublished { get; init; }

    /// <summary>
    /// Gets the list of tags assigned to this document
    /// </summary>
    public required IReadOnlyList<TagDto> Tags { get; init; }
}
