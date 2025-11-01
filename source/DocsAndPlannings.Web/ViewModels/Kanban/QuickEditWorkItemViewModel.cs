using System.ComponentModel.DataAnnotations;
using DocsAndPlannings.Core.DTOs;
using DocsAndPlannings.Core.DTOs.Statuses;

namespace DocsAndPlannings.Web.ViewModels.Kanban;

/// <summary>
/// View model for quick editing a work item from the kanban board
/// </summary>
public sealed class QuickEditWorkItemViewModel
{
    /// <summary>
    /// Gets or sets the work item identifier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the work item key (e.g., "PROJ-123")
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the work item summary
    /// </summary>
    [Required(ErrorMessage = "Summary is required")]
    [MaxLength(500, ErrorMessage = "Summary cannot exceed 500 characters")]
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the status identifier
    /// </summary>
    [Required(ErrorMessage = "Status is required")]
    public int StatusId { get; set; }

    /// <summary>
    /// Gets or sets the assignee identifier (null for unassigned)
    /// </summary>
    public int? AssigneeId { get; set; }

    /// <summary>
    /// Gets or sets the priority (1-5)
    /// </summary>
    [Required(ErrorMessage = "Priority is required")]
    [Range(1, 5, ErrorMessage = "Priority must be between 1 and 5")]
    public int Priority { get; set; }

    /// <summary>
    /// Gets or sets the available statuses
    /// </summary>
    public IReadOnlyList<StatusDto> AvailableStatuses { get; set; } = Array.Empty<StatusDto>();

    /// <summary>
    /// Gets or sets the available users
    /// </summary>
    public IReadOnlyList<UserDto> AvailableUsers { get; set; } = Array.Empty<UserDto>();
}
