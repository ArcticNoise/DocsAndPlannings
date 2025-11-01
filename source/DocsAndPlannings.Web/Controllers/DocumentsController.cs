using System.Security.Claims;
using DocsAndPlannings.Core.DTOs.Documents;
using DocsAndPlannings.Web.Services;
using DocsAndPlannings.Web.ViewModels.Documents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocsAndPlannings.Web.Controllers;

/// <summary>
/// Controller for document management in the web UI
/// </summary>
[Authorize]
public sealed class DocumentsController : Controller
{
    private readonly IApiClient m_ApiClient;
    private readonly ILogger<DocumentsController> m_Logger;

    /// <summary>
    /// Initializes a new instance of the DocumentsController class
    /// </summary>
    public DocumentsController(
        IApiClient apiClient,
        ILogger<DocumentsController> logger)
    {
        m_ApiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // GET: /Documents
    /// <summary>
    /// Displays the document list with search and filter options
    /// </summary>
    public async Task<IActionResult> Index(
        string? searchQuery = null,
        int? tagId = null,
        bool publishedOnly = true,
        int pageNumber = 1,
        int pageSize = 20)
    {
        try
        {
            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            if (pageSize < 5 || pageSize > 100)
            {
                pageSize = 20;
            }

            DocumentListViewModel viewModel = new DocumentListViewModel
            {
                SearchQuery = searchQuery,
                SelectedTagId = tagId,
                PublishedOnly = publishedOnly,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            // Fetch tags for filter dropdown
            try
            {
                List<TagDto>? tags = await m_ApiClient.GetAsync<List<TagDto>>("/api/tags");
                viewModel.AvailableTags = tags ?? [];
            }
            catch (Exception ex)
            {
                m_Logger.LogWarning(ex, "Error fetching tags for document list");
                // Continue without tags
            }

            // Build search request
            DocumentSearchRequest searchRequest = new DocumentSearchRequest
            {
                Query = searchQuery,
                TagIds = tagId.HasValue ? [tagId.Value] : null,
                PublishedOnly = publishedOnly
            };

            // Fetch documents
            try
            {
                List<DocumentListItemDto>? allDocuments = await m_ApiClient.PostAsync<List<DocumentListItemDto>>(
                    "/api/documents/search",
                    searchRequest);

                if (allDocuments != null && allDocuments.Count > 0)
                {
                    viewModel.TotalCount = allDocuments.Count;

                    // Apply pagination
                    viewModel.Documents = allDocuments
                        .OrderByDescending(d => d.UpdatedAt)
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();
                }
                else
                {
                    viewModel.Documents = [];
                    viewModel.TotalCount = 0;
                }
            }
            catch (Exception ex)
            {
                m_Logger.LogError(ex, "Error searching documents");
                TempData["ErrorMessage"] = "An error occurred while loading documents.";
                viewModel.Documents = [];
                viewModel.TotalCount = 0;
            }

            return View(viewModel);
        }
        catch (Exception ex)
        {
            m_Logger.LogError(ex, "Error loading document list");
            TempData["ErrorMessage"] = "An error occurred while loading the document list.";
            return View(new DocumentListViewModel());
        }
    }

    // GET: /Documents/View/5
    /// <summary>
    /// Displays a single document with rendered markdown
    /// </summary>
    public async Task<IActionResult> View(int id)
    {
        try
        {
            int currentUserId = GetCurrentUserId();

            // Fetch document
            DocumentDto? document = await m_ApiClient.GetAsync<DocumentDto>($"/api/documents/{id}");
            if (document == null)
            {
                TempData["ErrorMessage"] = "Document not found.";
                return RedirectToAction(nameof(Index));
            }

            // Fetch attachments
            List<DocumentAttachmentDto>? attachments = [];
            try
            {
                attachments = await m_ApiClient.GetAsync<List<DocumentAttachmentDto>>(
                    $"/api/documents/{id}/attachments") ?? [];
            }
            catch (Exception ex)
            {
                m_Logger.LogWarning(ex, "Error fetching attachments for document {DocumentId}", id);
                // Continue without attachments
            }

            // Fetch parent document if exists
            DocumentListItemDto? parentDocument = null;
            if (document.ParentDocumentId.HasValue)
            {
                try
                {
                    DocumentDto? parent = await m_ApiClient.GetAsync<DocumentDto>(
                        $"/api/documents/{document.ParentDocumentId.Value}");
                    if (parent != null)
                    {
                        parentDocument = new DocumentListItemDto
                        {
                            Id = parent.Id,
                            Title = parent.Title,
                            AuthorName = parent.AuthorName,
                            CurrentVersion = parent.CurrentVersion,
                            UpdatedAt = parent.UpdatedAt,
                            IsPublished = parent.IsPublished,
                            ParentDocumentId = parent.ParentDocumentId,
                            Tags = parent.Tags
                        };
                    }
                }
                catch (Exception ex)
                {
                    m_Logger.LogWarning(ex, "Error fetching parent document for document {DocumentId}", id);
                    // Continue without parent
                }
            }

            // Fetch child documents
            List<DocumentListItemDto>? childDocuments = [];
            try
            {
                childDocuments = await m_ApiClient.GetAsync<List<DocumentListItemDto>>(
                    $"/api/documents/{id}/children") ?? [];
            }
            catch (Exception ex)
            {
                m_Logger.LogWarning(ex, "Error fetching child documents for document {DocumentId}", id);
                // Continue without children
            }

            // Build view model
            DocumentViewerViewModel viewModel = new DocumentViewerViewModel
            {
                Document = document,
                Attachments = attachments,
                RenderedContent = RenderMarkdown(document.Content),
                CanEdit = document.AuthorId == currentUserId,
                CanDelete = document.AuthorId == currentUserId,
                ParentDocument = parentDocument,
                ChildDocuments = childDocuments
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            m_Logger.LogError(ex, "Error viewing document {DocumentId}", id);
            TempData["ErrorMessage"] = "An error occurred while loading the document.";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: /Documents/Create
    /// <summary>
    /// Displays the form for creating a new document
    /// </summary>
    public async Task<IActionResult> Create(int? parentDocumentId = null)
    {
        try
        {
            DocumentEditorViewModel viewModel = new DocumentEditorViewModel
            {
                ParentDocumentId = parentDocumentId
            };

            // Fetch tags
            await LoadEditorDataAsync(viewModel);

            return View(viewModel);
        }
        catch (Exception ex)
        {
            m_Logger.LogError(ex, "Error loading document creation form");
            TempData["ErrorMessage"] = "An error occurred while loading the form.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: /Documents/Create
    /// <summary>
    /// Creates a new document
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DocumentEditorViewModel viewModel)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                await LoadEditorDataAsync(viewModel);
                return View(viewModel);
            }

            CreateDocumentRequest request = new CreateDocumentRequest
            {
                Title = viewModel.Title,
                Content = viewModel.Content,
                ParentDocumentId = viewModel.ParentDocumentId,
                TagIds = viewModel.SelectedTagIds,
                IsPublished = viewModel.IsPublished
            };

            DocumentDto? document = await m_ApiClient.PostAsync<DocumentDto>("/api/documents", request);
            if (document != null)
            {
                TempData["SuccessMessage"] = "Document created successfully.";
                return RedirectToAction(nameof(View), new { id = document.Id });
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to create document.";
                await LoadEditorDataAsync(viewModel);
                return View(viewModel);
            }
        }
        catch (Exception ex)
        {
            m_Logger.LogError(ex, "Error creating document");
            TempData["ErrorMessage"] = "An error occurred while creating the document.";
            await LoadEditorDataAsync(viewModel);
            return View(viewModel);
        }
    }

    // GET: /Documents/Edit/5
    /// <summary>
    /// Displays the form for editing an existing document
    /// </summary>
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            // Fetch document
            DocumentDto? document = await m_ApiClient.GetAsync<DocumentDto>($"/api/documents/{id}");
            if (document == null)
            {
                TempData["ErrorMessage"] = "Document not found.";
                return RedirectToAction(nameof(Index));
            }

            // Check if user can edit
            int currentUserId = GetCurrentUserId();
            if (document.AuthorId != currentUserId)
            {
                TempData["ErrorMessage"] = "You do not have permission to edit this document.";
                return RedirectToAction(nameof(View), new { id });
            }

            DocumentEditorViewModel viewModel = new DocumentEditorViewModel
            {
                Id = document.Id,
                Title = document.Title,
                Content = document.Content,
                ParentDocumentId = document.ParentDocumentId,
                SelectedTagIds = document.Tags.Select(t => t.Id).ToList(),
                IsPublished = document.IsPublished,
                CurrentVersion = document.CurrentVersion
            };

            await LoadEditorDataAsync(viewModel);

            return View(viewModel);
        }
        catch (Exception ex)
        {
            m_Logger.LogError(ex, "Error loading document edit form for document {DocumentId}", id);
            TempData["ErrorMessage"] = "An error occurred while loading the document.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: /Documents/Edit/5
    /// <summary>
    /// Updates an existing document
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DocumentEditorViewModel viewModel)
    {
        try
        {
            if (id != viewModel.Id)
            {
                TempData["ErrorMessage"] = "Invalid document ID.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                await LoadEditorDataAsync(viewModel);
                return View(viewModel);
            }

            UpdateDocumentRequest request = new UpdateDocumentRequest
            {
                Title = viewModel.Title,
                Content = viewModel.Content,
                ParentDocumentId = viewModel.ParentDocumentId,
                TagIds = viewModel.SelectedTagIds,
                IsPublished = viewModel.IsPublished
            };

            DocumentDto? document = await m_ApiClient.PutAsync<DocumentDto>($"/api/documents/{id}", request);
            if (document != null)
            {
                TempData["SuccessMessage"] = "Document updated successfully.";
                return RedirectToAction(nameof(View), new { id = document.Id });
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update document.";
                await LoadEditorDataAsync(viewModel);
                return View(viewModel);
            }
        }
        catch (Exception ex)
        {
            m_Logger.LogError(ex, "Error updating document {DocumentId}", id);
            TempData["ErrorMessage"] = "An error occurred while updating the document.";
            await LoadEditorDataAsync(viewModel);
            return View(viewModel);
        }
    }

    // POST: /Documents/Delete/5
    /// <summary>
    /// Deletes a document
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await m_ApiClient.DeleteAsync($"/api/documents/{id}");
            TempData["SuccessMessage"] = "Document deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            m_Logger.LogError(ex, "Error deleting document {DocumentId}", id);
            TempData["ErrorMessage"] = "An error occurred while deleting the document.";
            return RedirectToAction(nameof(View), new { id });
        }
    }

    // POST: /Documents/UploadAttachment
    /// <summary>
    /// Uploads a file attachment to a document
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadAttachment(UploadScreenshotViewModel viewModel)
    {
        try
        {
            if (!ModelState.IsValid || viewModel.File == null)
            {
                TempData["ErrorMessage"] = "Please select a valid file to upload.";
                return RedirectToAction(nameof(View), new { id = viewModel.DocumentId });
            }

            // Validate file size
            if (viewModel.File.Length > viewModel.MaxFileSizeBytes)
            {
                TempData["ErrorMessage"] = $"File size exceeds the maximum allowed size of {viewModel.MaxFileSizeMB:F1} MB.";
                return RedirectToAction(nameof(View), new { id = viewModel.DocumentId });
            }

            // Validate file extension
            string fileExtension = Path.GetExtension(viewModel.File.FileName).ToLowerInvariant();
            if (!viewModel.AllowedExtensions.Contains(fileExtension))
            {
                TempData["ErrorMessage"] = $"File type not allowed. Allowed types: {viewModel.AllowedExtensionsDisplay}";
                return RedirectToAction(nameof(View), new { id = viewModel.DocumentId });
            }

            // Create multipart form data
            using MultipartFormDataContent content = new MultipartFormDataContent();
            using MemoryStream ms = new MemoryStream();
            await viewModel.File.CopyToAsync(ms);
            ms.Position = 0;

            StreamContent fileContent = new StreamContent(ms);
            content.Add(fileContent, "file", viewModel.File.FileName);

            // Upload via API (note: we'll use a direct HttpClient approach here since FormData upload is complex)
            // For now, we'll redirect with an error and implement file upload via JavaScript
            TempData["InfoMessage"] = "Please use the upload button in the document view for file uploads.";
            return RedirectToAction(nameof(View), new { id = viewModel.DocumentId });
        }
        catch (Exception ex)
        {
            m_Logger.LogError(ex, "Error uploading attachment to document {DocumentId}", viewModel.DocumentId);
            TempData["ErrorMessage"] = "An error occurred while uploading the file.";
            return RedirectToAction(nameof(View), new { id = viewModel.DocumentId });
        }
    }

    // GET: /Documents/DownloadAttachment/5/10
    /// <summary>
    /// Downloads an attachment file
    /// </summary>
    public IActionResult DownloadAttachment(int documentId, int attachmentId)
    {
        try
        {
            // Note: This would need special handling with HttpClient to download files
            // For now, we'll use a direct API call approach
            TempData["InfoMessage"] = "File download will be implemented via direct API access.";
            return RedirectToAction(nameof(View), new { id = documentId });
        }
        catch (Exception ex)
        {
            m_Logger.LogError(ex, "Error downloading attachment {AttachmentId} from document {DocumentId}", attachmentId, documentId);
            TempData["ErrorMessage"] = "An error occurred while downloading the file.";
            return RedirectToAction(nameof(View), new { id = documentId });
        }
    }

    // POST: /Documents/DeleteAttachment
    /// <summary>
    /// Deletes an attachment
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAttachment(int documentId, int attachmentId)
    {
        try
        {
            await m_ApiClient.DeleteAsync($"/api/documents/{documentId}/attachments/{attachmentId}");
            TempData["SuccessMessage"] = "Attachment deleted successfully.";
            return RedirectToAction(nameof(View), new { id = documentId });
        }
        catch (Exception ex)
        {
            m_Logger.LogError(ex, "Error deleting attachment {AttachmentId} from document {DocumentId}", attachmentId, documentId);
            TempData["ErrorMessage"] = "An error occurred while deleting the attachment.";
            return RedirectToAction(nameof(View), new { id = documentId });
        }
    }

    /// <summary>
    /// Gets the current user ID from claims
    /// </summary>
    private int GetCurrentUserId()
    {
        string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return 0;
        }
        return userId;
    }

    /// <summary>
    /// Loads tags and parent documents for the editor view
    /// </summary>
    private async Task LoadEditorDataAsync(DocumentEditorViewModel viewModel)
    {
        // Fetch tags
        try
        {
            List<TagDto>? tags = await m_ApiClient.GetAsync<List<TagDto>>("/api/tags");
            viewModel.AvailableTags = tags ?? [];
        }
        catch (Exception ex)
        {
            m_Logger.LogWarning(ex, "Error fetching tags for document editor");
            viewModel.AvailableTags = [];
        }

        // Fetch available parent documents (only published ones)
        try
        {
            DocumentSearchRequest searchRequest = new DocumentSearchRequest
            {
                PublishedOnly = true
            };

            List<DocumentListItemDto>? documents = await m_ApiClient.PostAsync<List<DocumentListItemDto>>(
                "/api/documents/search",
                searchRequest);

            // Exclude current document from parent options (when editing)
            if (viewModel.Id.HasValue && documents != null)
            {
                documents = documents.Where(d => d.Id != viewModel.Id.Value).ToList();
            }

            viewModel.AvailableParentDocuments = documents ?? [];
        }
        catch (Exception ex)
        {
            m_Logger.LogWarning(ex, "Error fetching parent documents for document editor");
            viewModel.AvailableParentDocuments = [];
        }
    }

    /// <summary>
    /// Renders markdown content to HTML
    /// Simple implementation - can be enhanced with a proper markdown library
    /// </summary>
    private static string RenderMarkdown(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return string.Empty;
        }

        // Simple markdown rendering (replace with proper library in production)
        // For now, just escape HTML and preserve line breaks
        string html = System.Net.WebUtility.HtmlEncode(markdown);
        html = html.Replace("\n", "<br>");

        return html;
    }
}
