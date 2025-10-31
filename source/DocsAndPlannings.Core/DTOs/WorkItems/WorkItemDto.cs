using DocsAndPlannings.Core.Models;

namespace DocsAndPlannings.Core.DTOs.WorkItems;

/// <summary>
/// Data transfer object for a complete work item with all details
/// </summary>
public sealed record WorkItemDto
{
    /// <summary>
    /// Gets the unique identifier of the work item
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets the ID of the project this work item belongs to
    /// </summary>
    public required int ProjectId { get; init; }

    /// <summary>
    /// Gets the project key
    /// </summary>
    public required string ProjectKey { get; init; }

    /// <summary>
    /// Gets the unique key of the work item (e.g., "PROJ-123")
    /// </summary>
    public required string Key { get; init; }

    /// <summary>
    /// Gets the ID of the epic (if assigned)
    /// </summary>
    public int? EpicId { get; init; }

    /// <summary>
    /// Gets the epic key
    /// </summary>
    public string? EpicKey { get; init; }

    /// <summary>
    /// Gets the ID of the parent work item (for subtasks)
    /// </summary>
    public int? ParentWorkItemId { get; init; }

    /// <summary>
    /// Gets the parent work item key
    /// </summary>
    public string? ParentWorkItemKey { get; init; }

    /// <summary>
    /// Gets the type of work item
    /// </summary>
    public required WorkItemType Type { get; init; }

    /// <summary>
    /// Gets the summary/title of the work item
    /// </summary>
    public required string Summary { get; init; }

    /// <summary>
    /// Gets the description of the work item
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the ID of the user assigned to this work item
    /// </summary>
    public int? AssigneeId { get; init; }

    /// <summary>
    /// Gets the name of the assignee
    /// </summary>
    public string? AssigneeName { get; init; }

    /// <summary>
    /// Gets the ID of the user who reported this work item
    /// </summary>
    public int? ReporterId { get; init; }

    /// <summary>
    /// Gets the name of the reporter
    /// </summary>
    public string? ReporterName { get; init; }

    /// <summary>
    /// Gets the ID of the status
    /// </summary>
    public required int StatusId { get; init; }

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
    /// Gets the timestamp when the work item was created
    /// </summary>
    public required DateTime CreatedAt { get; init; }

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
