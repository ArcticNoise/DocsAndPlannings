using System.Security.Claims;
using DocsAndPlannings.Core.DTOs.Documents;
using DocsAndPlannings.Core.Exceptions;
using DocsAndPlannings.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocsAndPlannings.Api.Controllers;

/// <summary>
/// Controller for managing documents
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly ILogger<DocumentsController> _logger;

    /// <summary>
    /// Initializes a new instance of the DocumentsController class
    /// </summary>
    public DocumentsController(
        IDocumentService documentService,
        ILogger<DocumentsController> logger)
    {
        _documentService = documentService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new document
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<DocumentDto>> CreateDocument([FromBody] CreateDocumentRequest request)
    {
        try
        {
            int userId = GetCurrentUserId();
            DocumentDto document = await _documentService.CreateDocumentAsync(request, userId);
            _logger.LogInformation("Document created: {DocumentId} by user {UserId}", document.Id, userId);
            return CreatedAtAction(nameof(GetDocumentById), new { id = document.Id }, document);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Document creation failed: {Message}", ex.Message);
            return NotFound(new { error = ex.Message });
        }
        catch (BadRequestException ex)
        {
            _logger.LogWarning("Document creation failed: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during document creation");
            return StatusCode(500, new { error = "An error occurred while creating the document" });
        }
    }

    /// <summary>
    /// Gets a document by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<DocumentDto>> GetDocumentById(int id)
    {
        try
        {
            int userId = GetCurrentUserId();
            DocumentDto document = await _documentService.GetDocumentByIdAsync(id, userId);
            return Ok(document);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ForbiddenException ex)
        {
            return StatusCode(403, new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving document {DocumentId}", id);
            return StatusCode(500, new { error = "An error occurred while retrieving the document" });
        }
    }

    /// <summary>
    /// Updates an existing document
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<DocumentDto>> UpdateDocument(int id, [FromBody] UpdateDocumentRequest request)
    {
        try
        {
            int userId = GetCurrentUserId();
            DocumentDto document = await _documentService.UpdateDocumentAsync(id, request, userId);
            _logger.LogInformation("Document updated: {DocumentId} by user {UserId}", id, userId);
            return Ok(document);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Document update failed: {Message}", ex.Message);
            return NotFound(new { error = ex.Message });
        }
        catch (ForbiddenException ex)
        {
            _logger.LogWarning("Document update forbidden: {Message}", ex.Message);
            return StatusCode(403, new { error = ex.Message });
        }
        catch (BadRequestException ex)
        {
            _logger.LogWarning("Document update failed: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating document {DocumentId}", id);
            return StatusCode(500, new { error = "An error occurred while updating the document" });
        }
    }

    /// <summary>
    /// Deletes a document (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteDocument(int id)
    {
        try
        {
            int userId = GetCurrentUserId();
            await _documentService.DeleteDocumentAsync(id, userId);
            _logger.LogInformation("Document deleted: {DocumentId} by user {UserId}", id, userId);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Document deletion failed: {Message}", ex.Message);
            return NotFound(new { error = ex.Message });
        }
        catch (ForbiddenException ex)
        {
            _logger.LogWarning("Document deletion forbidden: {Message}", ex.Message);
            return StatusCode(403, new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting document {DocumentId}", id);
            return StatusCode(500, new { error = "An error occurred while deleting the document" });
        }
    }

    /// <summary>
    /// Gets all versions of a document
    /// </summary>
    [HttpGet("{id}/versions")]
    public async Task<ActionResult<IReadOnlyList<DocumentVersionDto>>> GetDocumentVersions(int id)
    {
        try
        {
            int userId = GetCurrentUserId();
            IReadOnlyList<DocumentVersionDto> versions = await _documentService.GetDocumentVersionsAsync(id, userId);
            return Ok(versions);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ForbiddenException ex)
        {
            return StatusCode(403, new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving versions for document {DocumentId}", id);
            return StatusCode(500, new { error = "An error occurred while retrieving document versions" });
        }
    }

    /// <summary>
    /// Gets all child documents of a parent document
    /// </summary>
    [HttpGet("{parentId}/children")]
    public async Task<ActionResult<IReadOnlyList<DocumentListItemDto>>> GetChildDocuments(int parentId)
    {
        try
        {
            int userId = GetCurrentUserId();
            IReadOnlyList<DocumentListItemDto> children = await _documentService.GetChildDocumentsAsync(parentId, userId);
            return Ok(children);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving child documents for parent {ParentId}", parentId);
            return StatusCode(500, new { error = "An error occurred while retrieving child documents" });
        }
    }

    /// <summary>
    /// Searches documents based on criteria
    /// </summary>
    [HttpPost("search")]
    public async Task<ActionResult<IReadOnlyList<DocumentListItemDto>>> SearchDocuments([FromBody] DocumentSearchRequest request)
    {
        try
        {
            int userId = GetCurrentUserId();
            IReadOnlyList<DocumentListItemDto> documents = await _documentService.SearchDocumentsAsync(request, userId);
            return Ok(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during document search");
            return StatusCode(500, new { error = "An error occurred while searching documents" });
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
            throw new UnauthorizedAccessException("User ID not found in token");
        }
        return userId;
    }
}
