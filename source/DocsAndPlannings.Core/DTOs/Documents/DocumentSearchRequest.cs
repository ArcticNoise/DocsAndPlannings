namespace DocsAndPlannings.Core.DTOs.Documents;

/// <summary>
/// Request model for searching documents
/// </summary>
public sealed record DocumentSearchRequest
{
    /// <summary>
    /// Gets the search query to match against title and content
    /// </summary>
    public string? Query { get; init; }

    /// <summary>
    /// Gets the list of tag IDs to filter by
    /// </summary>
    public IReadOnlyList<int>? TagIds { get; init; }

    /// <summary>
    /// Gets the author ID to filter by
    /// </summary>
    public int? AuthorId { get; init; }

    /// <summary>
    /// Gets whether to include only published documents
    /// </summary>
    public bool? PublishedOnly { get; init; }

    /// <summary>
    /// Gets the parent document ID to filter by (null for root documents)
    /// </summary>
    public int? ParentDocumentId { get; init; }
}
