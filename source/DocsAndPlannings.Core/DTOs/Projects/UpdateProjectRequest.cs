using System.ComponentModel.DataAnnotations;

namespace DocsAndPlannings.Core.DTOs.Projects;

/// <summary>
/// Request model for updating an existing project
/// </summary>
public sealed record UpdateProjectRequest
{
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

    /// <summary>
    /// Gets whether the project is active
    /// </summary>
    public bool IsActive { get; init; } = true;
}
