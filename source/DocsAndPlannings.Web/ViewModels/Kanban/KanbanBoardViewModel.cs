using DocsAndPlannings.Core.DTOs;
using DocsAndPlannings.Core.DTOs.Boards;
using DocsAndPlannings.Core.DTOs.Epics;

namespace DocsAndPlannings.Web.ViewModels.Kanban;

/// <summary>
/// View model for the Kanban board page
/// </summary>
public sealed class KanbanBoardViewModel
{
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
    /// Gets or sets the board view data
    /// </summary>
    public BoardViewDto? Board { get; set; }

    /// <summary>
    /// Gets or sets the search text filter
    /// </summary>
    public string? SearchText { get; set; }

    /// <summary>
    /// Gets or sets the selected epic ID filter
    /// </summary>
    public int? SelectedEpicId { get; set; }

    /// <summary>
    /// Gets or sets the selected assignee ID filter
    /// </summary>
    public int? SelectedAssigneeId { get; set; }

    /// <summary>
    /// Gets or sets the available epics for filtering
    /// </summary>
    public IReadOnlyList<EpicListItemDto> AvailableEpics { get; set; } = Array.Empty<EpicListItemDto>();

    /// <summary>
    /// Gets or sets the available users for filtering
    /// </summary>
    public IReadOnlyList<UserDto> AvailableUsers { get; set; } = Array.Empty<UserDto>();

    /// <summary>
    /// Gets a value indicating whether the board exists
    /// </summary>
    public bool HasBoard => Board != null;
}
