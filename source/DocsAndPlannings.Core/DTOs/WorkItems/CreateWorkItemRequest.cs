using System.ComponentModel.DataAnnotations;
using DocsAndPlannings.Core.Models;

namespace DocsAndPlannings.Core.DTOs.WorkItems;

/// <summary>
/// Request model for creating a new work item (Task, Bug, or Subtask)
/// </summary>
public sealed record CreateWorkItemRequest
{
    /// <summary>
    /// Gets the ID of the project this work item belongs to
    /// </summary>
    [Required]
    public required int ProjectId { get; init; }

    /// <summary>
    /// Gets the ID of the epic (optional)
    /// </summary>
    public int? EpicId { get; init; }

    /// <summary>
    /// Gets the ID of the parent work item (required for Subtasks)
    /// </summary>
    public int? ParentWorkItemId { get; init; }

    /// <summary>
    /// Gets the type of work item (Task, Bug, or Subtask)
    /// </summary>
    [Required]
    public required WorkItemType Type { get; init; }

    /// <summary>
    /// Gets the summary/title of the work item
    /// </summary>
    [Required]
    [MaxLength(200)]
    public required string Summary { get; init; }

    /// <summary>
    /// Gets the description of the work item (supports markdown)
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the ID of the user assigned to this work item
    /// </summary>
    public int? AssigneeId { get; init; }

    /// <summary>
    /// Gets the ID of the user who reported this work item
    /// </summary>
    public int? ReporterId { get; init; }

    /// <summary>
    /// Gets the priority (1=Highest, 5=Lowest, default=3)
    /// </summary>
    [Range(1, 5)]
    public int Priority { get; init; } = 3;

    /// <summary>
    /// Gets the due date of the work item
    /// </summary>
    public DateTime? DueDate { get; init; }
}
