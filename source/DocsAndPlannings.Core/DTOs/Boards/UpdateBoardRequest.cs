using System.ComponentModel.DataAnnotations;

namespace DocsAndPlannings.Core.DTOs.Boards;

/// <summary>
/// Request model for updating a board
/// </summary>
public sealed record UpdateBoardRequest
{
    /// <summary>
    /// Gets the name of the board
    /// </summary>
    [Required]
    [MaxLength(200)]
    public required string Name { get; init; }

    /// <summary>
    /// Gets the description of the board
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; init; }
}
