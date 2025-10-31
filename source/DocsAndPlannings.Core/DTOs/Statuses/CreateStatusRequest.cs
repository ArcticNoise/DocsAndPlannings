using System.ComponentModel.DataAnnotations;

namespace DocsAndPlannings.Core.DTOs.Statuses;

/// <summary>
/// Request model for creating a new status
/// </summary>
public sealed record CreateStatusRequest
{
    /// <summary>
    /// Gets the name of the status
    /// </summary>
    [Required]
    [MaxLength(50)]
    public required string Name { get; init; }

    /// <summary>
    /// Gets the color code for the status (e.g., "#3498db")
    /// </summary>
    [MaxLength(20)]
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be a valid hex color code (e.g., #3498db)")]
    public string? Color { get; init; }

    /// <summary>
    /// Gets the order index for displaying statuses
    /// </summary>
    [Range(0, int.MaxValue)]
    public int OrderIndex { get; init; }

    /// <summary>
    /// Gets whether this status should be the default for new items
    /// </summary>
    public bool IsDefaultForNew { get; init; } = false;

    /// <summary>
    /// Gets whether this status indicates completed state
    /// </summary>
    public bool IsCompletedStatus { get; init; } = false;

    /// <summary>
    /// Gets whether this status indicates cancelled state
    /// </summary>
    public bool IsCancelledStatus { get; init; } = false;
}
