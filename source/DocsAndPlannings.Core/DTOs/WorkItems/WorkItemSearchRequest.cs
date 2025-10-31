using DocsAndPlannings.Core.Models;

namespace DocsAndPlannings.Core.DTOs.WorkItems;

/// <summary>
/// Request model for searching and filtering work items
/// </summary>
public sealed record WorkItemSearchRequest
{
    /// <summary>
    /// Gets the project ID to filter by
    /// </summary>
    public int? ProjectId { get; init; }

    /// <summary>
    /// Gets the epic ID to filter by
    /// </summary>
    public int? EpicId { get; init; }

    /// <summary>
    /// Gets the work item type to filter by
    /// </summary>
    public WorkItemType? Type { get; init; }

    /// <summary>
    /// Gets the status ID to filter by
    /// </summary>
    public int? StatusId { get; init; }

    /// <summary>
    /// Gets the assignee ID to filter by
    /// </summary>
    public int? AssigneeId { get; init; }

    /// <summary>
    /// Gets the reporter ID to filter by
    /// </summary>
    public int? ReporterId { get; init; }

    /// <summary>
    /// Gets the priority to filter by
    /// </summary>
    public int? Priority { get; init; }

    /// <summary>
    /// Gets the search text to match against summary and description
    /// </summary>
    public string? SearchText { get; init; }

    /// <summary>
    /// Gets the page number for pagination (1-based)
    /// </summary>
    public int Page { get; init; } = 1;

    /// <summary>
    /// Gets the page size for pagination
    /// </summary>
    public int PageSize { get; init; } = 50;
}
