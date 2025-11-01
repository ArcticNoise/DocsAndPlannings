using System.ComponentModel.DataAnnotations;
using DocsAndPlannings.Core.DTOs.Projects;

namespace DocsAndPlannings.Web.ViewModels.Projects;

/// <summary>
/// View model for the project list page with filtering and pagination
/// </summary>
public sealed class ProjectListViewModel
{
    /// <summary>
    /// Gets or sets the search query
    /// </summary>
    [MaxLength(200)]
    public string? SearchQuery { get; set; }

    /// <summary>
    /// Gets or sets whether to show only active projects
    /// </summary>
    public bool ActiveOnly { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to show archived projects
    /// </summary>
    public bool ShowArchived { get; set; }

    /// <summary>
    /// Gets or sets the list of projects
    /// </summary>
    public IReadOnlyList<ProjectListItemDto> Projects { get; set; } = [];

    /// <summary>
    /// Gets whether there are any projects in the list
    /// </summary>
    public bool HasProjects => Projects.Count > 0;
}
