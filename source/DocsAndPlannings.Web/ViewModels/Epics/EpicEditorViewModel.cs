using System.ComponentModel.DataAnnotations;
using DocsAndPlannings.Core.DTOs;
using DocsAndPlannings.Core.DTOs.Projects;
using DocsAndPlannings.Core.DTOs.Statuses;

namespace DocsAndPlannings.Web.ViewModels.Epics;

/// <summary>
/// View model for creating and editing epics
/// </summary>
public sealed class EpicEditorViewModel
{
    /// <summary>
    /// Gets or sets the epic ID (null for new epics)
    /// </summary>
    public int? Id { get; set; }

    /// <summary>
    /// Gets or sets the project ID
    /// </summary>
    [Required(ErrorMessage = "Project is required")]
    [Display(Name = "Project")]
    public int? ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the epic summary/title
    /// </summary>
    [Required(ErrorMessage = "Summary is required")]
    [MaxLength(200, ErrorMessage = "Summary cannot exceed 200 characters")]
    [Display(Name = "Summary")]
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the epic description
    /// </summary>
    [Display(Name = "Description (Markdown)")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the assignee ID
    /// </summary>
    [Display(Name = "Assignee")]
    public int? AssigneeId { get; set; }

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
    /// Gets or sets the start date
    /// </summary>
    [Display(Name = "Start Date")]
    public DateTime? StartDate { get; set; }

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
    /// Gets or sets the list of available statuses
    /// </summary>
    public IReadOnlyList<StatusDto> AvailableStatuses { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of available users for assignment
    /// </summary>
    public IReadOnlyList<UserDto> AvailableUsers { get; set; } = [];

    /// <summary>
    /// Gets whether this is a new epic
    /// </summary>
    public bool IsNewEpic => !Id.HasValue;
}
