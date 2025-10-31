namespace DocsAndPlannings.Core.DTOs.Comments;

/// <summary>
/// Data transfer object for a work item comment
/// </summary>
public sealed record CommentDto
{
    /// <summary>
    /// Gets the unique identifier of the comment
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets the ID of the work item this comment belongs to
    /// </summary>
    public required int WorkItemId { get; init; }

    /// <summary>
    /// Gets the ID of the comment author
    /// </summary>
    public required int AuthorId { get; init; }

    /// <summary>
    /// Gets the name of the comment author
    /// </summary>
    public required string AuthorName { get; init; }

    /// <summary>
    /// Gets the content of the comment
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Gets the timestamp when the comment was created
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Gets the timestamp when the comment was last updated
    /// </summary>
    public required DateTime UpdatedAt { get; init; }

    /// <summary>
    /// Gets whether the comment has been edited
    /// </summary>
    public required bool IsEdited { get; init; }
}
