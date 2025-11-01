using System.ComponentModel.DataAnnotations;
using DocsAndPlannings.Core.DTOs;
using DocsAndPlannings.Core.DTOs.Epics;
using DocsAndPlannings.Core.DTOs.Projects;
using DocsAndPlannings.Core.DTOs.Statuses;
using DocsAndPlannings.Core.DTOs.WorkItems;
using DocsAndPlannings.Core.Models;

namespace DocsAndPlannings.Web.ViewModels.WorkItems;

/// <summary>
/// View model for creating and editing work items
/// </summary>
public sealed class WorkItemEditorViewModel
{
    /// <summary>
    /// Gets or sets the work item ID (null for new work items)
    /// </summary>
    public int? Id { get; set; }

    /// <summary>
    /// Gets or sets the project ID
    /// </summary>
    [Required(ErrorMessage = "Project is required")]
    [Display(Name = "Project")]
    public int? ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the epic ID
    /// </summary>
    [Display(Name = "Epic")]
    public int? EpicId { get; set; }

    /// <summary>
    /// Gets or sets the parent work item ID (for subtasks)
    /// </summary>
    [Display(Name = "Parent Work Item")]
    public int? ParentWorkItemId { get; set; }

    /// <summary>
    /// Gets or sets the work item type
    /// </summary>
    [Required(ErrorMessage = "Type is required")]
    [Display(Name = "Type")]
    public WorkItemType? Type { get; set; }

    /// <summary>
    /// Gets or sets the work item summary/title
    /// </summary>
    [Required(ErrorMessage = "Summary is required")]
    [MaxLength(200, ErrorMessage = "Summary cannot exceed 200 characters")]
    [Display(Name = "Summary")]
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the work item description
    /// </summary>
    [Display(Name = "Description (Markdown)")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the assignee ID
    /// </summary>
    [Display(Name = "Assignee")]
    public int? AssigneeId { get; set; }

    /// <summary>
    /// Gets or sets the reporter ID
    /// </summary>
    [Display(Name = "Reporter")]
    public int? ReporterId { get; set; }

    /// <summary>
    /// Gets or sets the status ID
    /// </summary>
    [Display(Name = "Status")]
    public int? StatusId { get; set; }

    /// <summary>
    /// Gets or sets the priority
    /// </summary>
    [Required]
    [Range(1, 5, ErrorMessage = "Priority must be between 1 (Highest) and 5 (Lowest)")]
    [Display(Name = "Priority")]
    public int Priority { get; set; } = 3;

    /// <summary>
    /// Gets or sets the due date
    /// </summary>
    [Display(Name = "Due Date")]
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Gets or sets the list of available projects
    /// </summary>
    public IReadOnlyList<ProjectListItemDto> AvailableProjects { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of available epics for the selected project
    /// </summary>
    public IReadOnlyList<EpicListItemDto> AvailableEpics { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of available parent work items for subtasks
    /// </summary>
    public IReadOnlyList<WorkItemListItemDto> AvailableParentWorkItems { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of available statuses
    /// </summary>
    public IReadOnlyList<StatusDto> AvailableStatuses { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of available users for assignment
    /// </summary>
    public IReadOnlyList<UserDto> AvailableUsers { get; set; } = [];

    /// <summary>
    /// Gets whether this is a new work item
    /// </summary>
    public bool IsNewWorkItem => !Id.HasValue;
}
