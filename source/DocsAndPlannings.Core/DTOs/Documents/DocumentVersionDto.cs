namespace DocsAndPlannings.Core.DTOs.Documents;

/// <summary>
/// Data transfer object for a document version
/// </summary>
public sealed record DocumentVersionDto
{
    /// <summary>
    /// Gets the unique identifier of the version
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets the document ID this version belongs to
    /// </summary>
    public required int DocumentId { get; init; }

    /// <summary>
    /// Gets the version number
    /// </summary>
    public required int VersionNumber { get; init; }

    /// <summary>
    /// Gets the title at this version
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Gets the content at this version
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Gets the timestamp when this version was created
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Gets the ID of the user who created this version
    /// </summary>
    public required int CreatedById { get; init; }

    /// <summary>
    /// Gets the username of the user who created this version
    /// </summary>
    public required string CreatedByName { get; init; }
}
