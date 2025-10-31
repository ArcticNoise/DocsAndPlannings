using System.ComponentModel.DataAnnotations;

namespace DocsAndPlannings.Core.DTOs.Projects;

/// <summary>
/// Request model for creating a new project
/// </summary>
public sealed record CreateProjectRequest
{
    /// <summary>
    /// Gets the unique key for the project (e.g., "PROJ", "DEV")
    /// </summary>
    [Required]
    [MaxLength(10)]
    [RegularExpression(@"^[A-Z][A-Z0-9]*$", ErrorMessage = "Project key must start with a letter and contain only uppercase letters and numbers")]
    public required string Key { get; init; }

    /// <summary>
    /// Gets the name of the project
    /// </summary>
    [Required]
    [MaxLength(200)]
    public required string Name { get; init; }

    /// <summary>
    /// Gets the description of the project
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; init; }
}
