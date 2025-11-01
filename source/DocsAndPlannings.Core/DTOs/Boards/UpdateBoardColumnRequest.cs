using System.ComponentModel.DataAnnotations;

namespace DocsAndPlannings.Core.DTOs.Boards;

/// <summary>
/// Request model for updating a board column
/// </summary>
public sealed record UpdateBoardColumnRequest
{
    /// <summary>
    /// Gets the work-in-progress limit (null means no limit)
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "WIP limit must be non-negative")]
    public int? WIPLimit { get; init; }

    /// <summary>
    /// Gets whether the column is collapsed
    /// </summary>
    [Required]
    public required bool IsCollapsed { get; init; }
}
