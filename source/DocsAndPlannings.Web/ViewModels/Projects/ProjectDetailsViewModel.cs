using DocsAndPlannings.Core.DTOs.Epics;
using DocsAndPlannings.Core.DTOs.Projects;
using DocsAndPlannings.Core.DTOs.WorkItems;

namespace DocsAndPlannings.Web.ViewModels.Projects;

/// <summary>
/// View model for viewing a single project with details
/// </summary>
public sealed class ProjectDetailsViewModel
{
    /// <summary>
    /// Gets or sets the project details
    /// </summary>
    public required ProjectDto Project { get; set; }

    /// <summary>
    /// Gets or sets the list of epics in this project
    /// </summary>
    public IReadOnlyList<EpicListItemDto> Epics { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of recent work items
    /// </summary>
    public IReadOnlyList<WorkItemListItemDto> RecentWorkItems { get; set; } = [];

    /// <summary>
    /// Gets or sets whether the current user can edit this project
    /// </summary>
    public bool CanEdit { get; set; }

    /// <summary>
    /// Gets or sets whether the current user can delete this project
    /// </summary>
    public bool CanDelete { get; set; }
}
