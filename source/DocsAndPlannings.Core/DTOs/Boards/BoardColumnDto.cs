namespace DocsAndPlannings.Core.DTOs.Boards;

/// <summary>
/// Data transfer object for a board column
/// </summary>
public sealed record BoardColumnDto
{
    /// <summary>
    /// Gets the unique identifier of the column
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets the board identifier
    /// </summary>
    public required int BoardId { get; init; }

    /// <summary>
    /// Gets the status identifier
    /// </summary>
    public required int StatusId { get; init; }

    /// <summary>
    /// Gets the status name
    /// </summary>
    public required string StatusName { get; init; }

    /// <summary>
    /// Gets the status color
    /// </summary>
    public required string StatusColor { get; init; }

    /// <summary>
    /// Gets the display order index
    /// </summary>
    public required int OrderIndex { get; init; }

    /// <summary>
    /// Gets the work-in-progress limit (null means no limit)
    /// </summary>
    public int? WIPLimit { get; init; }

    /// <summary>
    /// Gets whether the column is collapsed
    /// </summary>
    public required bool IsCollapsed { get; init; }
}
