using System.ComponentModel.DataAnnotations;

namespace DocsAndPlannings.Core.DTOs.Statuses;

/// <summary>
/// Request model for creating a status transition rule
/// </summary>
public sealed record CreateStatusTransitionRequest
{
    /// <summary>
    /// Gets the ID of the source status
    /// </summary>
    [Required]
    public required int FromStatusId { get; init; }

    /// <summary>
    /// Gets the ID of the target status
    /// </summary>
    [Required]
    public required int ToStatusId { get; init; }

    /// <summary>
    /// Gets whether this transition is allowed
    /// </summary>
    public bool IsAllowed { get; init; } = true;
}
