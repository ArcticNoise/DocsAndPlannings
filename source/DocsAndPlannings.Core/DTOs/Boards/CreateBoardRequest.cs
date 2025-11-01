using System.ComponentModel.DataAnnotations;

namespace DocsAndPlannings.Core.DTOs.Boards;

/// <summary>
/// Request model for creating a new board
/// </summary>
public sealed record CreateBoardRequest
{
    /// <summary>
    /// Gets the name of the board (optional, defaults to "{Project.Name} Board")
    /// </summary>
    [MaxLength(200)]
    public string? Name { get; init; }

    /// <summary>
    /// Gets the description of the board
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; init; }
}
