using System.ComponentModel.DataAnnotations;
using DocsAndPlannings.Core.DTOs.Projects;
using DocsAndPlannings.Core.DTOs.WorkItems;
using DocsAndPlannings.Core.Models;

namespace DocsAndPlannings.Web.ViewModels.WorkItems;

/// <summary>
/// View model for the work item list page with advanced filtering and search
/// </summary>
public sealed class WorkItemListViewModel
{
    /// <summary>
    /// Gets or sets the search text
    /// </summary>
    [MaxLength(200)]
    public string? SearchText { get; set; }

    /// <summary>
    /// Gets or sets the selected project ID for filtering
    /// </summary>
    public int? SelectedProjectId { get; set; }

    /// <summary>
    /// Gets or sets the selected epic ID for filtering
    /// </summary>
    public int? SelectedEpicId { get; set; }

    /// <summary>
    /// Gets or sets the selected work item type for filtering
    /// </summary>
    public WorkItemType? SelectedType { get; set; }

    /// <summary>
    /// Gets or sets the selected status ID for filtering
    /// </summary>
    public int? SelectedStatusId { get; set; }

    /// <summary>
    /// Gets or sets the selected assignee ID for filtering
    /// </summary>
    public int? SelectedAssigneeId { get; set; }

    /// <summary>
    /// Gets or sets the selected reporter ID for filtering
    /// </summary>
    public int? SelectedReporterId { get; set; }

    /// <summary>
    /// Gets or sets the selected priority for filtering
    /// </summary>
    public int? SelectedPriority { get; set; }

    /// <summary>
    /// Gets or sets the current page number (1-based)
    /// </summary>
    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Gets or sets the page size
    /// </summary>
    [Range(5, 100)]
    public int PageSize { get; set; } = 50;

    /// <summary>
    /// Gets or sets the list of work items
    /// </summary>
    public IReadOnlyList<WorkItemListItemDto> WorkItems { get; set; } = [];

    /// <summary>
    /// Gets or sets the total count of work items matching the filter
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the list of available projects for filtering
    /// </summary>
    public IReadOnlyList<ProjectListItemDto> AvailableProjects { get; set; } = [];

    /// <summary>
    /// Gets the total number of pages
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    /// <summary>
    /// Gets whether there is a previous page
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Gets whether there is a next page
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Gets whether there are any work items in the list
    /// </summary>
    public bool HasWorkItems => WorkItems.Count > 0;
}
