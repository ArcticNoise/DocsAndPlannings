using System.ComponentModel.DataAnnotations;

namespace DocsAndPlannings.Web.ViewModels.Projects;

/// <summary>
/// View model for creating and editing projects
/// </summary>
public sealed class ProjectEditorViewModel
{
    /// <summary>
    /// Gets or sets the project ID (null for new projects)
    /// </summary>
    public int? Id { get; set; }

    /// <summary>
    /// Gets or sets the project key
    /// </summary>
    [Required(ErrorMessage = "Project key is required")]
    [MaxLength(10, ErrorMessage = "Project key cannot exceed 10 characters")]
    [RegularExpression(@"^[A-Z][A-Z0-9]*$", ErrorMessage = "Project key must start with a letter and contain only uppercase letters and numbers")]
    [Display(Name = "Project Key")]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project name
    /// </summary>
    [Required(ErrorMessage = "Project name is required")]
    [MaxLength(200, ErrorMessage = "Project name cannot exceed 200 characters")]
    [Display(Name = "Project Name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project description
    /// </summary>
    [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets whether the project is active
    /// </summary>
    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the project is archived
    /// </summary>
    [Display(Name = "Archived")]
    public bool IsArchived { get; set; }

    /// <summary>
    /// Gets whether this is a new project
    /// </summary>
    public bool IsNewProject => !Id.HasValue;
}
