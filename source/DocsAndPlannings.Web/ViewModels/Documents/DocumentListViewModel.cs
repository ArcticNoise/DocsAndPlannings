using System.ComponentModel.DataAnnotations;
using DocsAndPlannings.Core.DTOs.Documents;

namespace DocsAndPlannings.Web.ViewModels.Documents;

/// <summary>
/// View model for the document list page with search and pagination
/// </summary>
public sealed class DocumentListViewModel
{
    /// <summary>
    /// Gets or sets the search query
    /// </summary>
    [MaxLength(200)]
    public string? SearchQuery { get; set; }

    /// <summary>
    /// Gets or sets the selected tag ID for filtering
    /// </summary>
    public int? SelectedTagId { get; set; }

    /// <summary>
    /// Gets or sets whether to show only published documents
    /// </summary>
    public bool PublishedOnly { get; set; } = true;

    /// <summary>
    /// Gets or sets the current page number (1-based)
    /// </summary>
    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Gets or sets the page size
    /// </summary>
    [Range(5, 100)]
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Gets or sets the list of documents
    /// </summary>
    public IReadOnlyList<DocumentListItemDto> Documents { get; set; } = [];

    /// <summary>
    /// Gets or sets the total count of documents matching the filter
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the list of available tags for filtering
    /// </summary>
    public IReadOnlyList<TagDto> AvailableTags { get; set; } = [];

    /// <summary>
    /// Gets the total number of pages
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    /// <summary>
    /// Gets whether there is a previous page
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Gets whether there is a next page
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;
}
