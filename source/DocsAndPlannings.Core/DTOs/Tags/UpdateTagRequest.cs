using System.ComponentModel.DataAnnotations;

namespace DocsAndPlannings.Core.DTOs.Tags;

/// <summary>
/// Request model for updating an existing tag
/// </summary>
public sealed record UpdateTagRequest
{
    /// <summary>
    /// Gets the updated name of the tag
    /// </summary>
    [MaxLength(50)]
    public string? Name { get; init; }

    /// <summary>
    /// Gets the updated color code for the tag (e.g., #FF5733)
    /// </summary>
    [MaxLength(7)]
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be a valid hex code (e.g., #FF5733)")]
    public string? Color { get; init; }
}
