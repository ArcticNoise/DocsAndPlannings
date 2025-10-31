using System.Diagnostics;
using DocsAndPlannings.Core.Data;
using DocsAndPlannings.Core.DTOs.Documents;
using DocsAndPlannings.Core.Exceptions;
using DocsAndPlannings.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DocsAndPlannings.Core.Services;

/// <summary>
/// Service for managing documents
/// </summary>
public sealed class DocumentService : IDocumentService
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the DocumentService class
    /// </summary>
    /// <param name="context">The database context</param>
    public DocumentService(ApplicationDbContext context)
    {
        Debug.Assert(context != null, "Database context cannot be null");
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<DocumentDto> CreateDocumentAsync(CreateDocumentRequest request, int userId)
    {
        Debug.Assert(request != null, "Create document request cannot be null");
        Debug.Assert(userId > 0, "User ID must be positive");

        // Verify parent document exists if specified
        if (request.ParentDocumentId.HasValue)
        {
            bool parentExists = await _context.Documents
                .AnyAsync(d => d.Id == request.ParentDocumentId.Value && !d.IsDeleted);

            if (!parentExists)
            {
                throw new NotFoundException($"Parent document with ID {request.ParentDocumentId.Value} not found");
            }

            // Check for circular hierarchy
            if (await WouldCreateCycleAsync(null, request.ParentDocumentId.Value))
            {
                throw new BadRequestException("Setting this parent would create a circular hierarchy");
            }
        }

        // Verify all tags exist
        if (request.TagIds.Count > 0)
        {
            int existingTagCount = await _context.DocumentTags
                .CountAsync(t => request.TagIds.Contains(t.Id));

            if (existingTagCount != request.TagIds.Count)
            {
                throw new NotFoundException("One or more tags not found");
            }
        }

        DateTime now = DateTime.UtcNow;

        Document document = new Document
        {
            Title = request.Title,
            Content = request.Content,
            ParentDocumentId = request.ParentDocumentId,
            AuthorId = userId,
            CurrentVersion = 1,
            CreatedAt = now,
            UpdatedAt = now,
            IsPublished = request.IsPublished,
            IsDeleted = false,
            ChildDocuments = [],
            Versions = [],
            Tags = []
        };

        _context.Documents.Add(document);
        await _context.SaveChangesAsync();

        // Create initial version
        DocumentVersion version = new DocumentVersion
        {
            DocumentId = document.Id,
            VersionNumber = 1,
            Title = document.Title,
            Content = document.Content,
            CreatedAt = now,
            ModifiedById = userId
        };

        _context.DocumentVersions.Add(version);

        // Assign tags
        if (request.TagIds.Count > 0)
        {
            foreach (int tagId in request.TagIds)
            {
                DocumentTagMap tagMap = new DocumentTagMap
                {
                    DocumentId = document.Id,
                    TagId = tagId,
                    AssignedAt = now
                };

                _context.Set<DocumentTagMap>().Add(tagMap);
            }
        }

        await _context.SaveChangesAsync();

        // Load related entities and return DTO
        return await GetDocumentDtoAsync(document.Id, userId);
    }

    /// <inheritdoc/>
    public async Task<DocumentDto> GetDocumentByIdAsync(int id, int userId)
    {
        Debug.Assert(id > 0, "Document ID must be positive");
        Debug.Assert(userId > 0, "User ID must be positive");

        Document? document = await _context.Documents
            .Include(d => d.Author)
            .Include(d => d.Tags)
                .ThenInclude(tm => tm.Tag)
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

        if (document == null)
        {
            throw new NotFoundException($"Document with ID {id} not found");
        }

        // Check access: unpublished documents only accessible to author
        if (!document.IsPublished && document.AuthorId != userId)
        {
            throw new ForbiddenException("You do not have permission to access this document");
        }

        return MapToDocumentDto(document);
    }

    /// <inheritdoc/>
    public async Task<DocumentDto> UpdateDocumentAsync(int id, UpdateDocumentRequest request, int userId)
    {
        Debug.Assert(id > 0, "Document ID must be positive");
        Debug.Assert(request != null, "Update document request cannot be null");
        Debug.Assert(userId > 0, "User ID must be positive");

        Document? document = await _context.Documents
            .Include(d => d.Tags)
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

        if (document == null)
        {
            throw new NotFoundException($"Document with ID {id} not found");
        }

        // Only author can update
        if (document.AuthorId != userId)
        {
            throw new ForbiddenException("You do not have permission to update this document");
        }

        bool contentChanged = false;

        // Update title if provided
        if (request.Title != null && request.Title != document.Title)
        {
            document.Title = request.Title;
            contentChanged = true;
        }

        // Update content if provided
        if (request.Content != null && request.Content != document.Content)
        {
            document.Content = request.Content;
            contentChanged = true;
        }

        // Update parent document ID if provided
        if (request.ParentDocumentId != document.ParentDocumentId)
        {
            if (request.ParentDocumentId.HasValue)
            {
                bool parentExists = await _context.Documents
                    .AnyAsync(d => d.Id == request.ParentDocumentId.Value && !d.IsDeleted);

                if (!parentExists)
                {
                    throw new NotFoundException($"Parent document with ID {request.ParentDocumentId.Value} not found");
                }

                // Check for circular hierarchy (including self-reference)
                if (await WouldCreateCycleAsync(id, request.ParentDocumentId.Value))
                {
                    throw new BadRequestException("Setting this parent would create a circular hierarchy");
                }
            }

            document.ParentDocumentId = request.ParentDocumentId;
        }

        // Update published status if provided
        if (request.IsPublished.HasValue)
        {
            document.IsPublished = request.IsPublished.Value;
        }

        // Update tags if provided
        if (request.TagIds != null)
        {
            // Verify all tags exist
            if (request.TagIds.Count > 0)
            {
                int existingTagCount = await _context.DocumentTags
                    .CountAsync(t => request.TagIds.Contains(t.Id));

                if (existingTagCount != request.TagIds.Count)
                {
                    throw new NotFoundException("One or more tags not found");
                }
            }

            // Remove existing tags
            _context.Set<DocumentTagMap>().RemoveRange(document.Tags);

            // Add new tags
            DateTime now = DateTime.UtcNow;
            foreach (int tagId in request.TagIds)
            {
                DocumentTagMap tagMap = new DocumentTagMap
                {
                    DocumentId = document.Id,
                    TagId = tagId,
                    AssignedAt = now
                };

                _context.Set<DocumentTagMap>().Add(tagMap);
            }
        }

        document.UpdatedAt = DateTime.UtcNow;

        // Create new version if content changed
        if (contentChanged)
        {
            document.CurrentVersion++;

            DocumentVersion version = new DocumentVersion
            {
                DocumentId = document.Id,
                VersionNumber = document.CurrentVersion,
                Title = document.Title,
                Content = document.Content,
                CreatedAt = document.UpdatedAt,
                ModifiedById = userId
            };

            _context.DocumentVersions.Add(version);
        }

        await _context.SaveChangesAsync();

        return await GetDocumentDtoAsync(id, userId);
    }

    /// <inheritdoc/>
    public async Task DeleteDocumentAsync(int id, int userId)
    {
        Debug.Assert(id > 0, "Document ID must be positive");
        Debug.Assert(userId > 0, "User ID must be positive");

        Document? document = await _context.Documents
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

        if (document == null)
        {
            throw new NotFoundException($"Document with ID {id} not found");
        }

        // Only author can delete
        if (document.AuthorId != userId)
        {
            throw new ForbiddenException("You do not have permission to delete this document");
        }

        document.IsDeleted = true;
        document.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<DocumentVersionDto>> GetDocumentVersionsAsync(int documentId, int userId)
    {
        Debug.Assert(documentId > 0, "Document ID must be positive");
        Debug.Assert(userId > 0, "User ID must be positive");

        Document? document = await _context.Documents
            .FirstOrDefaultAsync(d => d.Id == documentId && !d.IsDeleted);

        if (document == null)
        {
            throw new NotFoundException($"Document with ID {documentId} not found");
        }

        // Check access: unpublished documents only accessible to author
        if (!document.IsPublished && document.AuthorId != userId)
        {
            throw new ForbiddenException("You do not have permission to access this document");
        }

        List<DocumentVersion> versions = await _context.DocumentVersions
            .Include(v => v.ModifiedBy)
            .Where(v => v.DocumentId == documentId)
            .OrderByDescending(v => v.VersionNumber)
            .ToListAsync();

        return versions.Select(MapToDocumentVersionDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<DocumentListItemDto>> GetChildDocumentsAsync(int parentId, int userId)
    {
        Debug.Assert(parentId > 0, "Parent document ID must be positive");
        Debug.Assert(userId > 0, "User ID must be positive");

        bool parentExists = await _context.Documents
            .AnyAsync(d => d.Id == parentId && !d.IsDeleted);

        if (!parentExists)
        {
            throw new NotFoundException($"Parent document with ID {parentId} not found");
        }

        List<Document> childDocuments = await _context.Documents
            .Include(d => d.Author)
            .Include(d => d.Tags)
                .ThenInclude(tm => tm.Tag)
            .Where(d => d.ParentDocumentId == parentId && !d.IsDeleted)
            .Where(d => d.IsPublished || d.AuthorId == userId)
            .OrderBy(d => d.Title)
            .ToListAsync();

        return childDocuments.Select(MapToDocumentListItemDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<DocumentListItemDto>> SearchDocumentsAsync(DocumentSearchRequest request, int userId)
    {
        Debug.Assert(request != null, "Search request cannot be null");
        Debug.Assert(userId > 0, "User ID must be positive");

        IQueryable<Document> query = _context.Documents
            .Include(d => d.Author)
            .Include(d => d.Tags)
                .ThenInclude(tm => tm.Tag)
            .Where(d => !d.IsDeleted);

        // Filter by published status
        if (request.PublishedOnly == true)
        {
            query = query.Where(d => d.IsPublished);
        }
        else
        {
            // Include unpublished documents only if user is the author
            query = query.Where(d => d.IsPublished || d.AuthorId == userId);
        }

        // Filter by search query (title or content)
        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            string searchTerm = request.Query.ToLowerInvariant();
            query = query.Where(d =>
                d.Title.ToLower().Contains(searchTerm) ||
                d.Content.ToLower().Contains(searchTerm));
        }

        // Filter by author
        if (request.AuthorId.HasValue)
        {
            query = query.Where(d => d.AuthorId == request.AuthorId.Value);
        }

        // Filter by parent document
        if (request.ParentDocumentId.HasValue)
        {
            query = query.Where(d => d.ParentDocumentId == request.ParentDocumentId.Value);
        }
        else if (request.ParentDocumentId == null && request.Query == null && request.AuthorId == null)
        {
            // If no filters and ParentDocumentId not explicitly set, show root documents
            query = query.Where(d => d.ParentDocumentId == null);
        }

        // Filter by tags (documents must have ALL specified tags)
        if (request.TagIds != null && request.TagIds.Count > 0)
        {
            foreach (int tagId in request.TagIds)
            {
                int currentTagId = tagId;
                query = query.Where(d => d.Tags.Any(tm => tm.TagId == currentTagId));
            }
        }

        List<Document> documents = await query
            .OrderBy(d => d.Title)
            .ToListAsync();

        return documents.Select(MapToDocumentListItemDto).ToList();
    }

    /// <summary>
    /// Checks if setting parentId as the parent of documentId would create a cycle
    /// </summary>
    /// <param name="documentId">The document ID (null for new documents)</param>
    /// <param name="parentId">The proposed parent ID</param>
    /// <returns>True if a cycle would be created, false otherwise</returns>
    private async Task<bool> WouldCreateCycleAsync(int? documentId, int parentId)
    {
        // Self-reference check (for existing documents)
        if (documentId.HasValue && documentId.Value == parentId)
        {
            return true;
        }

        // Traverse up the hierarchy to check for cycles
        HashSet<int> visited = [];
        int? currentId = parentId;

        while (currentId.HasValue)
        {
            // If we encounter the document being updated, it's a cycle
            if (documentId.HasValue && currentId.Value == documentId.Value)
            {
                return true;
            }

            // If we've seen this ID before, there's a cycle in the existing hierarchy
            if (!visited.Add(currentId.Value))
            {
                return true;
            }

            // Get parent of current document
            currentId = await _context.Documents
                .Where(d => d.Id == currentId.Value && !d.IsDeleted)
                .Select(d => d.ParentDocumentId)
                .FirstOrDefaultAsync();
        }

        return false;
    }

    /// <summary>
    /// Helper method to load a document with all related entities and map to DTO
    /// </summary>
    private async Task<DocumentDto> GetDocumentDtoAsync(int id, int userId)
    {
        Document? document = await _context.Documents
            .Include(d => d.Author)
            .Include(d => d.Tags)
                .ThenInclude(tm => tm.Tag)
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

        if (document == null)
        {
            throw new NotFoundException($"Document with ID {id} not found");
        }

        return MapToDocumentDto(document);
    }

    /// <summary>
    /// Maps a Document entity to DocumentDto
    /// </summary>
    private static DocumentDto MapToDocumentDto(Document document)
    {
        return new DocumentDto
        {
            Id = document.Id,
            Title = document.Title,
            Content = document.Content,
            ParentDocumentId = document.ParentDocumentId,
            AuthorId = document.AuthorId,
            AuthorName = $"{document.Author.FirstName} {document.Author.LastName}",
            CurrentVersion = document.CurrentVersion,
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt,
            IsPublished = document.IsPublished,
            Tags = document.Tags.Select(tm => new DocsAndPlannings.Core.DTOs.Documents.TagDto
            {
                Id = tm.Tag.Id,
                Name = tm.Tag.Name,
                Color = tm.Tag.Color
            }).ToList()
        };
    }

    /// <summary>
    /// Maps a Document entity to DocumentListItemDto
    /// </summary>
    private static DocumentListItemDto MapToDocumentListItemDto(Document document)
    {
        return new DocumentListItemDto
        {
            Id = document.Id,
            Title = document.Title,
            ParentDocumentId = document.ParentDocumentId,
            AuthorName = $"{document.Author.FirstName} {document.Author.LastName}",
            CurrentVersion = document.CurrentVersion,
            UpdatedAt = document.UpdatedAt,
            IsPublished = document.IsPublished,
            Tags = document.Tags.Select(tm => new DocsAndPlannings.Core.DTOs.Documents.TagDto
            {
                Id = tm.Tag.Id,
                Name = tm.Tag.Name,
                Color = tm.Tag.Color
            }).ToList()
        };
    }

    /// <summary>
    /// Maps a DocumentVersion entity to DocumentVersionDto
    /// </summary>
    private static DocumentVersionDto MapToDocumentVersionDto(DocumentVersion version)
    {
        return new DocumentVersionDto
        {
            Id = version.Id,
            DocumentId = version.DocumentId,
            VersionNumber = version.VersionNumber,
            Title = version.Title,
            Content = version.Content,
            CreatedAt = version.CreatedAt,
            CreatedById = version.ModifiedById,
            CreatedByName = $"{version.ModifiedBy.FirstName} {version.ModifiedBy.LastName}"
        };
    }
}
