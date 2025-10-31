using DocsAndPlannings.Core.Models;

namespace DocsAndPlannings.Core.DTOs.WorkItems;

/// <summary>
/// Data transfer object for a work item in list view (summary)
/// </summary>
public sealed record WorkItemListItemDto
{
    /// <summary>
    /// Gets the unique identifier of the work item
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets the unique key of the work item
    /// </summary>
    public required string Key { get; init; }

    /// <summary>
    /// Gets the type of work item
    /// </summary>
    public required WorkItemType Type { get; init; }

    /// <summary>
    /// Gets the summary/title of the work item
    /// </summary>
    public required string Summary { get; init; }

    /// <summary>
    /// Gets the name of the assignee
    /// </summary>
    public string? AssigneeName { get; init; }

    /// <summary>
    /// Gets the name of the status
    /// </summary>
    public required string StatusName { get; init; }

    /// <summary>
    /// Gets the priority (1=Highest, 5=Lowest)
    /// </summary>
    public required int Priority { get; init; }

    /// <summary>
    /// Gets the due date of the work item
    /// </summary>
    public DateTime? DueDate { get; init; }

    /// <summary>
    /// Gets the timestamp when the work item was last updated
    /// </summary>
    public required DateTime UpdatedAt { get; init; }

    /// <summary>
    /// Gets the count of child work items (subtasks)
    /// </summary>
    public int ChildWorkItemCount { get; init; }

    /// <summary>
    /// Gets the count of comments
    /// </summary>
    public int CommentCount { get; init; }
}
