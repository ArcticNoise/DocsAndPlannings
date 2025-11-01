using DocsAndPlannings.Core.DTOs.Documents;

namespace DocsAndPlannings.Web.ViewModels.Documents;

/// <summary>
/// View model for viewing a document with its attachments
/// </summary>
public sealed class DocumentViewerViewModel
{
    /// <summary>
    /// Gets or sets the document data
    /// </summary>
    public required DocumentDto Document { get; set; }

    /// <summary>
    /// Gets or sets the list of attachments
    /// </summary>
    public IReadOnlyList<DocumentAttachmentDto> Attachments { get; set; } = [];

    /// <summary>
    /// Gets or sets the rendered HTML content from markdown
    /// </summary>
    public string RenderedContent { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the current user can edit this document
    /// </summary>
    public bool CanEdit { get; set; }

    /// <summary>
    /// Gets or sets whether the current user can delete this document
    /// </summary>
    public bool CanDelete { get; set; }

    /// <summary>
    /// Gets or sets the parent document (if any)
    /// </summary>
    public DocumentListItemDto? ParentDocument { get; set; }

    /// <summary>
    /// Gets or sets the child documents
    /// </summary>
    public IReadOnlyList<DocumentListItemDto> ChildDocuments { get; set; } = [];
}
