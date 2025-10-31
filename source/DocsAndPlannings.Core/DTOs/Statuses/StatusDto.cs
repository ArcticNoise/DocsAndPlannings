namespace DocsAndPlannings.Core.DTOs.Statuses;

/// <summary>
/// Data transfer object for a status
/// </summary>
public sealed record StatusDto
{
    /// <summary>
    /// Gets the unique identifier of the status
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets the name of the status
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the color code for the status
    /// </summary>
    public string? Color { get; init; }

    /// <summary>
    /// Gets the order index for displaying statuses
    /// </summary>
    public required int OrderIndex { get; init; }

    /// <summary>
    /// Gets whether this status is the default for new items
    /// </summary>
    public required bool IsDefaultForNew { get; init; }

    /// <summary>
    /// Gets whether this status indicates completed state
    /// </summary>
    public required bool IsCompletedStatus { get; init; }

    /// <summary>
    /// Gets whether this status indicates cancelled state
    /// </summary>
    public required bool IsCancelledStatus { get; init; }

    /// <summary>
    /// Gets the timestamp when the status was created
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Gets whether the status is active
    /// </summary>
    public required bool IsActive { get; init; }
}
