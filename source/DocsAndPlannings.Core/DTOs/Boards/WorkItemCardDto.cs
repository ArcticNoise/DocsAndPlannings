using DocsAndPlannings.Core.Models;

namespace DocsAndPlannings.Core.DTOs.Boards;

/// <summary>
/// Data transfer object for a work item card displayed on the board
/// </summary>
public sealed record WorkItemCardDto
{
    /// <summary>
    /// Gets the unique identifier of the work item
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets the work item key (e.g., "PROJ-123")
    /// </summary>
    public required string Key { get; init; }

    /// <summary>
    /// Gets the work item summary
    /// </summary>
    public required string Summary { get; init; }

    /// <summary>
    /// Gets the assignee name (null if unassigned)
    /// </summary>
    public string? AssigneeName { get; init; }

    /// <summary>
    /// Gets the work item type
    /// </summary>
    public required WorkItemType Type { get; init; }

    /// <summary>
    /// Gets the priority
    /// </summary>
    public required int Priority { get; init; }

    /// <summary>
    /// Gets the status identifier
    /// </summary>
    public required int StatusId { get; init; }

    /// <summary>
    /// Gets the order index within the column
    /// </summary>
    public int? OrderIndex { get; init; }
}
