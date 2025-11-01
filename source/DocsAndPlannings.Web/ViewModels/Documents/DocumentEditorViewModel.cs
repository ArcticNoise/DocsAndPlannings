using System.ComponentModel.DataAnnotations;
using DocsAndPlannings.Core.DTOs.Documents;

namespace DocsAndPlannings.Web.ViewModels.Documents;

/// <summary>
/// View model for creating or editing a document
/// </summary>
public sealed class DocumentEditorViewModel
{
    /// <summary>
    /// Gets or sets the document ID (null for new documents)
    /// </summary>
    public int? Id { get; set; }

    /// <summary>
    /// Gets or sets the title of the document
    /// </summary>
    [Required(ErrorMessage = "Title is required")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    [Display(Name = "Document Title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the markdown content of the document
    /// </summary>
    [Required(ErrorMessage = "Content is required")]
    [Display(Name = "Content (Markdown)")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the parent document ID
    /// </summary>
    [Display(Name = "Parent Document")]
    public int? ParentDocumentId { get; set; }

    /// <summary>
    /// Gets or sets the selected tag IDs
    /// </summary>
    [Display(Name = "Tags")]
    public List<int> SelectedTagIds { get; set; } = [];

    /// <summary>
    /// Gets or sets whether the document should be published
    /// </summary>
    [Display(Name = "Publish Document")]
    public bool IsPublished { get; set; }

    /// <summary>
    /// Gets or sets the list of available tags
    /// </summary>
    public IReadOnlyList<TagDto> AvailableTags { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of available parent documents
    /// </summary>
    public IReadOnlyList<DocumentListItemDto> AvailableParentDocuments { get; set; } = [];

    /// <summary>
    /// Gets or sets the current version number (for editing)
    /// </summary>
    public int? CurrentVersion { get; set; }

    /// <summary>
    /// Gets whether this is a new document
    /// </summary>
    public bool IsNewDocument => !Id.HasValue;

    /// <summary>
    /// Gets the page title
    /// </summary>
    public string PageTitle => IsNewDocument ? "Create New Document" : $"Edit Document: {Title}";
}
