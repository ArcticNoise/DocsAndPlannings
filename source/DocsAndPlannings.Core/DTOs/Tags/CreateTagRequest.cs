using System.ComponentModel.DataAnnotations;

namespace DocsAndPlannings.Core.DTOs.Tags;

/// <summary>
/// Request model for creating a new tag
/// </summary>
public sealed record CreateTagRequest
{
    /// <summary>
    /// Gets the name of the tag
    /// </summary>
    [Required]
    [MaxLength(50)]
    public required string Name { get; init; }

    /// <summary>
    /// Gets the optional color code for the tag (e.g., #FF5733)
    /// </summary>
    [MaxLength(7)]
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be a valid hex code (e.g., #FF5733)")]
    public string? Color { get; init; }
}
