namespace DocsAndPlannings.Core.DTOs.Epics;

/// <summary>
/// Data transfer object for a complete epic with all details
/// </summary>
public sealed record EpicDto
{
    /// <summary>
    /// Gets the unique identifier of the epic
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets the ID of the project this epic belongs to
    /// </summary>
    public required int ProjectId { get; init; }

    /// <summary>
    /// Gets the project key
    /// </summary>
    public required string ProjectKey { get; init; }

    /// <summary>
    /// Gets the unique key of the epic (e.g., "PROJ-EPIC-1")
    /// </summary>
    public required string Key { get; init; }

    /// <summary>
    /// Gets the summary/title of the epic
    /// </summary>
    public required string Summary { get; init; }

    /// <summary>
    /// Gets the description of the epic
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the ID of the user assigned to this epic
    /// </summary>
    public int? AssigneeId { get; init; }

    /// <summary>
    /// Gets the name of the assignee
    /// </summary>
    public string? AssigneeName { get; init; }

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
    /// Gets the start date of the epic
    /// </summary>
    public DateTime? StartDate { get; init; }

    /// <summary>
    /// Gets the due date of the epic
    /// </summary>
    public DateTime? DueDate { get; init; }

    /// <summary>
    /// Gets the timestamp when the epic was created
    /// </summary>
    public required DateTime CreatedAt { get; init; }

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
