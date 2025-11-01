using System.ComponentModel.DataAnnotations;

namespace DocsAndPlannings.Web.ViewModels.Kanban;

/// <summary>
/// View model for updating board column configuration
/// </summary>
public sealed class UpdateColumnViewModel
{
    /// <summary>
    /// Gets or sets the column identifier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the column name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the work-in-progress limit (null means no limit)
    /// </summary>
    [Range(0, 999, ErrorMessage = "WIP limit must be between 0 and 999")]
    public int? WIPLimit { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the column is collapsed
    /// </summary>
    public bool IsCollapsed { get; set; }
}
