using System.ComponentModel.DataAnnotations;

namespace DocsAndPlannings.Core.DTOs.Boards;

/// <summary>
/// Request model for reordering board columns
/// </summary>
public sealed record ReorderColumnsRequest
{
    /// <summary>
    /// Gets the ordered list of column IDs
    /// </summary>
    [Required]
    [MinLength(1, ErrorMessage = "At least one column ID is required")]
    public required IReadOnlyList<int> ColumnIds { get; init; }
}
