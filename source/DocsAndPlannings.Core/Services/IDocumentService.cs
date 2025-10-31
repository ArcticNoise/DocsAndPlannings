using DocsAndPlannings.Core.DTOs.Documents;

namespace DocsAndPlannings.Core.Services;

/// <summary>
/// Service interface for managing documents
/// </summary>
public interface IDocumentService
{
    /// <summary>
    /// Creates a new document
    /// </summary>
    /// <param name="request">The document creation request</param>
    /// <param name="userId">The ID of the user creating the document</param>
    /// <returns>The created document</returns>
    /// <exception cref="BadRequestException">Thrown when parent document ID would create a cycle</exception>
    /// <exception cref="NotFoundException">Thrown when parent document or tags are not found</exception>
    Task<DocumentDto> CreateDocumentAsync(CreateDocumentRequest request, int userId);

    /// <summary>
    /// Gets a document by ID
    /// </summary>
    /// <param name="id">The document ID</param>
    /// <param name="userId">The ID of the requesting user</param>
    /// <returns>The document if found and accessible</returns>
    /// <exception cref="NotFoundException">Thrown when document is not found</exception>
    /// <exception cref="ForbiddenException">Thrown when user lacks access to unpublished document</exception>
    Task<DocumentDto> GetDocumentByIdAsync(int id, int userId);

    /// <summary>
    /// Updates an existing document
    /// </summary>
    /// <param name="id">The document ID</param>
    /// <param name="request">The update request</param>
    /// <param name="userId">The ID of the user updating the document</param>
    /// <returns>The updated document</returns>
    /// <exception cref="NotFoundException">Thrown when document is not found</exception>
    /// <exception cref="ForbiddenException">Thrown when user is not the author</exception>
    /// <exception cref="BadRequestException">Thrown when parent document ID would create a cycle</exception>
    Task<DocumentDto> UpdateDocumentAsync(int id, UpdateDocumentRequest request, int userId);

    /// <summary>
    /// Soft-deletes a document
    /// </summary>
    /// <param name="id">The document ID</param>
    /// <param name="userId">The ID of the user deleting the document</param>
    /// <exception cref="NotFoundException">Thrown when document is not found</exception>
    /// <exception cref="ForbiddenException">Thrown when user is not the author</exception>
    Task DeleteDocumentAsync(int id, int userId);

    /// <summary>
    /// Gets all versions of a document
    /// </summary>
    /// <param name="documentId">The document ID</param>
    /// <param name="userId">The ID of the requesting user</param>
    /// <returns>List of document versions</returns>
    /// <exception cref="NotFoundException">Thrown when document is not found</exception>
    /// <exception cref="ForbiddenException">Thrown when user lacks access to unpublished document</exception>
    Task<IReadOnlyList<DocumentVersionDto>> GetDocumentVersionsAsync(int documentId, int userId);

    /// <summary>
    /// Gets all child documents of a parent document
    /// </summary>
    /// <param name="parentId">The parent document ID</param>
    /// <param name="userId">The ID of the requesting user</param>
    /// <returns>List of child documents</returns>
    /// <exception cref="NotFoundException">Thrown when parent document is not found</exception>
    Task<IReadOnlyList<DocumentListItemDto>> GetChildDocumentsAsync(int parentId, int userId);

    /// <summary>
    /// Searches documents based on criteria
    /// </summary>
    /// <param name="request">The search criteria</param>
    /// <param name="userId">The ID of the requesting user</param>
    /// <returns>List of documents matching the criteria</returns>
    Task<IReadOnlyList<DocumentListItemDto>> SearchDocumentsAsync(DocumentSearchRequest request, int userId);

    /// <summary>
    /// Uploads a file attachment to a document
    /// </summary>
    /// <param name="documentId">The document ID</param>
    /// <param name="fileStream">The file stream</param>
    /// <param name="fileName">The original file name</param>
    /// <param name="contentType">The MIME type</param>
    /// <param name="userId">The ID of the user uploading the file</param>
    /// <returns>The created attachment</returns>
    /// <exception cref="NotFoundException">Thrown when document is not found</exception>
    /// <exception cref="ForbiddenException">Thrown when user is not the author</exception>
    /// <exception cref="BadRequestException">Thrown when file is invalid</exception>
    Task<DocumentAttachmentDto> UploadAttachmentAsync(int documentId, Stream fileStream, string fileName, string contentType, int userId);

    /// <summary>
    /// Gets all attachments for a document
    /// </summary>
    /// <param name="documentId">The document ID</param>
    /// <param name="userId">The ID of the requesting user</param>
    /// <returns>List of attachments</returns>
    /// <exception cref="NotFoundException">Thrown when document is not found</exception>
    /// <exception cref="ForbiddenException">Thrown when user lacks access to unpublished document</exception>
    Task<IReadOnlyList<DocumentAttachmentDto>> GetDocumentAttachmentsAsync(int documentId, int userId);

    /// <summary>
    /// Gets an attachment file stream
    /// </summary>
    /// <param name="documentId">The document ID</param>
    /// <param name="attachmentId">The attachment ID</param>
    /// <param name="userId">The ID of the requesting user</param>
    /// <returns>A tuple containing the file stream, content type, and file name</returns>
    /// <exception cref="NotFoundException">Thrown when document or attachment is not found</exception>
    /// <exception cref="ForbiddenException">Thrown when user lacks access to unpublished document</exception>
    Task<(Stream FileStream, string ContentType, string FileName)> GetAttachmentFileAsync(int documentId, int attachmentId, int userId);

    /// <summary>
    /// Deletes an attachment
    /// </summary>
    /// <param name="documentId">The document ID</param>
    /// <param name="attachmentId">The attachment ID</param>
    /// <param name="userId">The ID of the user deleting the attachment</param>
    /// <exception cref="NotFoundException">Thrown when document or attachment is not found</exception>
    /// <exception cref="ForbiddenException">Thrown when user is not the author</exception>
    Task DeleteAttachmentAsync(int documentId, int attachmentId, int userId);
}
