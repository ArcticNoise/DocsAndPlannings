using System.ComponentModel.DataAnnotations;

namespace DocsAndPlannings.Core.DTOs.Epics;

/// <summary>
/// Request model for updating an existing epic
/// </summary>
public sealed record UpdateEpicRequest
{
    /// <summary>
    /// Gets the summary/title of the epic
    /// </summary>
    [Required]
    [MaxLength(200)]
    public required string Summary { get; init; }

    /// <summary>
    /// Gets the description of the epic (supports markdown)
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the ID of the user assigned to this epic
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
    /// Gets the start date of the epic
    /// </summary>
    public DateTime? StartDate { get; init; }

    /// <summary>
    /// Gets the due date of the epic
    /// </summary>
    public DateTime? DueDate { get; init; }
}
