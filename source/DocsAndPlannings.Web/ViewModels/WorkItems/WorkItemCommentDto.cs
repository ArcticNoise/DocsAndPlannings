namespace DocsAndPlannings.Web.ViewModels.WorkItems;

/// <summary>
/// Data transfer object for work item comments
/// </summary>
public sealed record WorkItemCommentDto
{
    /// <summary>
    /// Gets the unique identifier of the comment
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets the ID of the work item
    /// </summary>
    public required int WorkItemId { get; init; }

    /// <summary>
    /// Gets the comment content
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Gets the ID of the comment author
    /// </summary>
    public required int AuthorId { get; init; }

    /// <summary>
    /// Gets the name of the comment author
    /// </summary>
    public required string AuthorName { get; init; }

    /// <summary>
    /// Gets whether the comment has been edited
    /// </summary>
    public required bool IsEdited { get; init; }

    /// <summary>
    /// Gets the timestamp when the comment was created
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Gets the timestamp when the comment was last updated
    /// </summary>
    public required DateTime UpdatedAt { get; init; }
}
