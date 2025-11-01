using DocsAndPlannings.Core.DTOs.WorkItems;

namespace DocsAndPlannings.Web.ViewModels.WorkItems;

/// <summary>
/// View model for viewing a single work item with details
/// </summary>
public sealed class WorkItemDetailsViewModel
{
    /// <summary>
    /// Gets or sets the work item details
    /// </summary>
    public required WorkItemDto WorkItem { get; set; }

    /// <summary>
    /// Gets or sets the list of child work items (subtasks)
    /// </summary>
    public IReadOnlyList<WorkItemListItemDto> ChildWorkItems { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of comments
    /// </summary>
    public IReadOnlyList<WorkItemCommentDto> Comments { get; set; } = [];

    /// <summary>
    /// Gets or sets the rendered description content (HTML)
    /// </summary>
    public string RenderedDescription { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the current user can edit this work item
    /// </summary>
    public bool CanEdit { get; set; }

    /// <summary>
    /// Gets or sets whether the current user can delete this work item
    /// </summary>
    public bool CanDelete { get; set; }

    /// <summary>
    /// Gets or sets whether the current user can add comments
    /// </summary>
    public bool CanComment { get; set; } = true;
}
