using System.ComponentModel.DataAnnotations;
using DocsAndPlannings.Core.DTOs.Boards;
using DocsAndPlannings.Core.DTOs.Statuses;

namespace DocsAndPlannings.Web.ViewModels.Kanban;

/// <summary>
/// View model for board settings (create/edit board configuration)
/// </summary>
public sealed class BoardSettingsViewModel
{
    /// <summary>
    /// Gets or sets the board identifier (null for create)
    /// </summary>
    public int? Id { get; set; }

    /// <summary>
    /// Gets or sets the project identifier
    /// </summary>
    public int ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the project name
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project key
    /// </summary>
    public string ProjectKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the board name
    /// </summary>
    [Required(ErrorMessage = "Board name is required")]
    [MaxLength(200, ErrorMessage = "Board name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the board description
    /// </summary>
    [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the available statuses for the project
    /// </summary>
    public IReadOnlyList<StatusDto> AvailableStatuses { get; set; } = Array.Empty<StatusDto>();

    /// <summary>
    /// Gets or sets the existing board columns
    /// </summary>
    public IReadOnlyList<BoardColumnDto> Columns { get; set; } = Array.Empty<BoardColumnDto>();

    /// <summary>
    /// Gets a value indicating whether this is an edit operation
    /// </summary>
    public bool IsEdit => Id.HasValue;
}
