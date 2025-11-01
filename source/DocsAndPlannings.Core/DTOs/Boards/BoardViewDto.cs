namespace DocsAndPlannings.Core.DTOs.Boards;

/// <summary>
/// Data transfer object for a complete board view with work items
/// </summary>
public sealed record BoardViewDto
{
    /// <summary>
    /// Gets the unique identifier of the board
    /// </summary>
    public required int BoardId { get; init; }

    /// <summary>
    /// Gets the project identifier
    /// </summary>
    public required int ProjectId { get; init; }

    /// <summary>
    /// Gets the name of the board
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the description of the board
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the list of columns with work items
    /// </summary>
    public required IReadOnlyList<BoardColumnViewDto> Columns { get; init; }

    /// <summary>
    /// Gets the total number of work items across all columns
    /// </summary>
    public required int TotalItems { get; init; }
}
