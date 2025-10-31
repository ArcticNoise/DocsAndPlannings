namespace DocsAndPlannings.Core.DTOs.Statuses;

/// <summary>
/// Data transfer object for a status transition
/// </summary>
public sealed record StatusTransitionDto
{
    /// <summary>
    /// Gets the unique identifier of the transition
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets the ID of the source status
    /// </summary>
    public required int FromStatusId { get; init; }

    /// <summary>
    /// Gets the name of the source status
    /// </summary>
    public required string FromStatusName { get; init; }

    /// <summary>
    /// Gets the ID of the target status
    /// </summary>
    public required int ToStatusId { get; init; }

    /// <summary>
    /// Gets the name of the target status
    /// </summary>
    public required string ToStatusName { get; init; }

    /// <summary>
    /// Gets whether this transition is allowed
    /// </summary>
    public required bool IsAllowed { get; init; }

    /// <summary>
    /// Gets the timestamp when the transition was created
    /// </summary>
    public required DateTime CreatedAt { get; init; }
}
