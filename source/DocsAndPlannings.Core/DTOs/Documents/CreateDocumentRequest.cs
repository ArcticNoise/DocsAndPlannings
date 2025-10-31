using System.ComponentModel.DataAnnotations;

namespace DocsAndPlannings.Core.DTOs.Documents;

/// <summary>
/// Request model for creating a new document
/// </summary>
public sealed record CreateDocumentRequest
{
    /// <summary>
    /// Gets the title of the document
    /// </summary>
    [Required]
    [MaxLength(200)]
    public required string Title { get; init; }

    /// <summary>
    /// Gets the markdown content of the document
    /// </summary>
    [Required]
    public required string Content { get; init; }

    /// <summary>
    /// Gets the ID of the parent document, if this is a child document
    /// </summary>
    public int? ParentDocumentId { get; init; }

    /// <summary>
    /// Gets the list of tag IDs to assign to this document
    /// </summary>
    public IReadOnlyList<int> TagIds { get; init; } = [];

    /// <summary>
    /// Gets whether the document should be published immediately
    /// </summary>
    public bool IsPublished { get; init; } = false;
}
