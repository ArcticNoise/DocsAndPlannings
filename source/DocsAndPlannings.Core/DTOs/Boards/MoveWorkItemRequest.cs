using System.ComponentModel.DataAnnotations;

namespace DocsAndPlannings.Core.DTOs.Boards;

/// <summary>
/// Request model for moving a work item to a different status
/// </summary>
public sealed record MoveWorkItemRequest
{
    /// <summary>
    /// Gets the target status identifier
    /// </summary>
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Status ID must be greater than 0")]
    public required int ToStatusId { get; init; }
}
