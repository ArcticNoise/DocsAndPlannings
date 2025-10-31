using System.ComponentModel.DataAnnotations;

namespace DocsAndPlannings.Core.DTOs.Documents;

/// <summary>
/// Request model for updating an existing document
/// </summary>
public sealed record UpdateDocumentRequest
{
    /// <summary>
    /// Gets the updated title of the document
    /// </summary>
    [MaxLength(200)]
    public string? Title { get; init; }

    /// <summary>
    /// Gets the updated markdown content of the document
    /// </summary>
    public string? Content { get; init; }

    /// <summary>
    /// Gets the updated parent document ID
    /// </summary>
    public int? ParentDocumentId { get; init; }

    /// <summary>
    /// Gets the updated list of tag IDs
    /// </summary>
    public IReadOnlyList<int>? TagIds { get; init; }

    /// <summary>
    /// Gets the updated publication status
    /// </summary>
    public bool? IsPublished { get; init; }
}
