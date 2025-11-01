using DocsAndPlannings.Core.DTOs.Epics;
using DocsAndPlannings.Core.DTOs.WorkItems;

namespace DocsAndPlannings.Web.ViewModels.Epics;

/// <summary>
/// View model for viewing a single epic with details
/// </summary>
public sealed class EpicDetailsViewModel
{
    /// <summary>
    /// Gets or sets the epic details
    /// </summary>
    public required EpicDto Epic { get; set; }

    /// <summary>
    /// Gets or sets the list of work items in this epic
    /// </summary>
    public IReadOnlyList<WorkItemListItemDto> WorkItems { get; set; } = [];

    /// <summary>
    /// Gets or sets whether the current user can edit this epic
    /// </summary>
    public bool CanEdit { get; set; }

    /// <summary>
    /// Gets or sets whether the current user can delete this epic
    /// </summary>
    public bool CanDelete { get; set; }

    /// <summary>
    /// Gets the completion percentage
    /// </summary>
    public int CompletionPercentage
    {
        get
        {
            if (Epic.WorkItemCount == 0)
            {
                return 0;
            }

            return (int)Math.Round((double)Epic.CompletedWorkItemCount / Epic.WorkItemCount * 100);
        }
    }
}
