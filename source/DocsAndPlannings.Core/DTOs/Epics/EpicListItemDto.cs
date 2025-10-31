namespace DocsAndPlannings.Core.DTOs.Epics;

/// <summary>
/// Data transfer object for an epic in list view (summary)
/// </summary>
public sealed record EpicListItemDto
{
    /// <summary>
    /// Gets the unique identifier of the epic
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets the unique key of the epic
    /// </summary>
    public required string Key { get; init; }

    /// <summary>
    /// Gets the summary/title of the epic
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
    /// Gets the due date of the epic
    /// </summary>
    public DateTime? DueDate { get; init; }

    /// <summary>
    /// Gets the timestamp when the epic was last updated
    /// </summary>
    public required DateTime UpdatedAt { get; init; }

    /// <summary>
    /// Gets the count of work items in this epic
    /// </summary>
    public int WorkItemCount { get; init; }

    /// <summary>
    /// Gets the count of completed work items
    /// </summary>
    public int CompletedWorkItemCount { get; init; }
}
