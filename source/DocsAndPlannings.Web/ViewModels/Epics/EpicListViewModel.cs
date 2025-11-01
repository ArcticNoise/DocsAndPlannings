using System.ComponentModel.DataAnnotations;
using DocsAndPlannings.Core.DTOs.Epics;
using DocsAndPlannings.Core.DTOs.Projects;

namespace DocsAndPlannings.Web.ViewModels.Epics;

/// <summary>
/// View model for the epic list page with filtering and search
/// </summary>
public sealed class EpicListViewModel
{
    /// <summary>
    /// Gets or sets the search query
    /// </summary>
    [MaxLength(200)]
    public string? SearchQuery { get; set; }

    /// <summary>
    /// Gets or sets the selected project ID for filtering
    /// </summary>
    public int? SelectedProjectId { get; set; }

    /// <summary>
    /// Gets or sets the selected status ID for filtering
    /// </summary>
    public int? SelectedStatusId { get; set; }

    /// <summary>
    /// Gets or sets the selected assignee ID for filtering
    /// </summary>
    public int? SelectedAssigneeId { get; set; }

    /// <summary>
    /// Gets or sets the list of epics
    /// </summary>
    public IReadOnlyList<EpicListItemDto> Epics { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of available projects for filtering
    /// </summary>
    public IReadOnlyList<ProjectListItemDto> AvailableProjects { get; set; } = [];

    /// <summary>
    /// Gets whether there are any epics in the list
    /// </summary>
    public bool HasEpics => Epics.Count > 0;
}
