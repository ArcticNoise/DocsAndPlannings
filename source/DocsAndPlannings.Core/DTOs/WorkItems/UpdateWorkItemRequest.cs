using System.ComponentModel.DataAnnotations;

namespace DocsAndPlannings.Core.DTOs.WorkItems;

/// <summary>
/// Request model for updating an existing work item
/// </summary>
public sealed record UpdateWorkItemRequest
{
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
    /// Gets the ID of the status
    /// </summary>
    [Required]
    public required int StatusId { get; init; }

    /// <summary>
    /// Gets the priority (1=Highest, 5=Lowest)
    /// </summary>
    [Range(1, 5)]
    public int Priority { get; init; } = 3;

    /// <summary>
    /// Gets the due date of the work item
    /// </summary>
    public DateTime? DueDate { get; init; }
}
