namespace DocsAndPlannings.Core.DTOs.Boards;

/// <summary>
/// Data transfer object for a board
/// </summary>
public sealed record BoardDto
{
    /// <summary>
    /// Gets the unique identifier of the board
    /// </summary>
    public required int Id { get; init; }

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
    /// Gets the creation timestamp
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Gets the last update timestamp
    /// </summary>
    public required DateTime UpdatedAt { get; init; }

    /// <summary>
    /// Gets the list of board columns
    /// </summary>
    public required IReadOnlyList<BoardColumnDto> Columns { get; init; }
}
