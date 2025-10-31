# Phase 2 Implementation Guide - Addendum

**Date**: 2025-10-31
**Purpose**: Address validation findings and complete specifications
**Status**: Ready for Implementation

This addendum addresses the critical and important issues identified in the validation report and provides complete specifications for all missing components.

---

## Summary of Changes

### Critical Fixes Implemented
1. ✅ **Exception Classes**: Created in `source/DocsAndPlannings.Core/Exceptions/`
   - NotFoundException.cs
   - ForbiddenException.cs
   - BadRequestException.cs

2. ✅ **DocumentTagMap Verified**: Model exists and is properly configured

### Important Additions (This Document)
3. **Phase 2.1 Scope Clarification**: Removed file upload endpoints
4. **TagsController Added**: Complete CRUD specification
5. **Circular Hierarchy Prevention**: Added to DocumentService
6. **Phase 2.2 Complete Specification**: File upload implementation details

---

## 1. Phase 2.1 Scope Clarification

### CORRECTION: Remove File Upload from Phase 2.1

The original guide incorrectly included file upload endpoints in DocumentsController for Phase 2.1. File uploads are an **Advanced Feature** belonging to Phase 2.2.

### Updated DocumentsController Specification

**REMOVE these endpoints from Phase 2.1 DocumentsController**:
- `POST /api/Documents/{id}/attachments`
- `GET /api/Documents/attachments/{id}`

**Phase 2.1 DocumentsController Final Endpoint List**:
1. `GET /api/Documents` - Search/list documents
2. `GET /api/Documents/{id}` - Get document by ID
3. `POST /api/Documents` - Create document
4. `PUT /api/Documents/{id}` - Update document
5. `DELETE /api/Documents/{id}` - Soft delete document
6. `GET /api/Documents/{id}/versions` - Get all versions
7. `GET /api/Documents/{id}/versions/{versionNumber}` - Get specific version

**Total**: 7 endpoints (not 9)

### Updated Phase 2.1 Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                  ASP.NET Core Web API                        │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  DocumentsController (7 endpoints)                    │  │
│  │  - GET /api/Documents                                 │  │
│  │  - GET /api/Documents/{id}                           │  │
│  │  - POST /api/Documents                               │  │
│  │  - PUT /api/Documents/{id}                           │  │
│  │  - DELETE /api/Documents/{id}                        │  │
│  │  - GET /api/Documents/{id}/versions                  │  │
│  │  - GET /api/Documents/{id}/versions/{versionNumber}  │  │
│  └────────────────────┬─────────────────────────────────┘  │
│                       │                                      │
│  ┌────────────────────▼─────────────────────────────────┐  │
│  │  TagsController (4 endpoints) - NEW                   │  │
│  │  - GET /api/Tags                                      │  │
│  │  - POST /api/Tags                                     │  │
│  │  - PUT /api/Tags/{id}                                │  │
│  │  - DELETE /api/Tags/{id}                             │  │
│  └────────────────────┬─────────────────────────────────┘  │
│                       │                                      │
│  ┌────────────────────▼─────────────────────────────────┐  │
│  │  DocumentService (IDocumentService)                   │  │
│  │  - CRUD with authorization                            │  │
│  │  - Versioning                                         │  │
│  │  - Tag management                                     │  │
│  │  - Search/filtering                                   │  │
│  │  - Circular hierarchy prevention (NEW)               │  │
│  └────────────────────┬─────────────────────────────────┘  │
│                       │                                      │
│  ┌────────────────────▼─────────────────────────────────┐  │
│  │  ApplicationDbContext                                 │  │
│  │  - Documents, DocumentVersions, DocumentTags          │  │
│  └─────────────────────────────────────────────────────┬┘  │
└────────────────────────────────────────────────────────┼───┘
                                                          │
┌─────────────────────────────────────────────────────────▼───┐
│                   SQLite Database                            │
│  - Documents, DocumentVersions, DocumentTags, TagMaps        │
└─────────────────────────────────────────────────────────────┘
```

---

## 2. NEW: TagsController Specification

### Task 7: Create TagsController (4 hours)

Tags allow users to categorize documents. Only admins can manage tags; all users can assign existing tags.

### File: `source/DocsAndPlannings.Core/DTOs/Tags/CreateTagRequest.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace DocsAndPlannings.Core.DTOs.Tags;

public class CreateTagRequest
{
    [Required(ErrorMessage = "Tag name is required")]
    [MaxLength(50, ErrorMessage = "Tag name cannot exceed 50 characters")]
    public required string Name { get; set; }

    [MaxLength(20, ErrorMessage = "Color cannot exceed 20 characters")]
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be a valid hex code (e.g., #FF5733)")]
    public string? Color { get; set; }
}
```

### File: `source/DocsAndPlannings.Core/DTOs/Tags/UpdateTagRequest.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace DocsAndPlannings.Core.DTOs.Tags;

public class UpdateTagRequest
{
    [Required(ErrorMessage = "Tag name is required")]
    [MaxLength(50, ErrorMessage = "Tag name cannot exceed 50 characters")]
    public required string Name { get; set; }

    [MaxLength(20, ErrorMessage = "Color cannot exceed 20 characters")]
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be a valid hex code (e.g., #FF5733)")]
    public string? Color { get; set; }
}
```

### File: `source/DocsAndPlannings.Api/Controllers/TagsController.cs`

```csharp
using DocsAndPlannings.Core.Data;
using DocsAndPlannings.Core.DTOs.Documents; // For TagDto
using DocsAndPlannings.Core.DTOs.Tags;
using DocsAndPlannings.Core.Exceptions;
using DocsAndPlannings.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DocsAndPlannings.Api.Controllers;

/// <summary>
/// Controller for managing document tags
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TagsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TagsController> _logger;

    public TagsController(
        ApplicationDbContext context,
        ILogger<TagsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets all active tags
    /// </summary>
    /// <returns>List of active tags</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<TagDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll()
    {
        List<DocumentTag> tags = await _context.DocumentTags
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync();

        List<TagDto> dtos = tags.Select(t => new TagDto
        {
            Id = t.Id,
            Name = t.Name,
            Color = t.Color
        }).ToList();

        _logger.LogInformation("Retrieved {Count} active tags", dtos.Count);

        return Ok(dtos);
    }

    /// <summary>
    /// Gets a tag by ID
    /// </summary>
    /// <param name="id">Tag ID</param>
    /// <returns>Tag details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TagDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        DocumentTag? tag = await _context.DocumentTags
            .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);

        if (tag == null)
        {
            return NotFound(new { error = $"Tag with ID {id} not found" });
        }

        TagDto dto = new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Color = tag.Color
        };

        return Ok(dto);
    }

    /// <summary>
    /// Creates a new tag
    /// </summary>
    /// <param name="request">Tag creation data</param>
    /// <returns>Created tag</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(TagDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateTagRequest request)
    {
        // Check for duplicate name (case-insensitive)
        bool exists = await _context.DocumentTags
            .AnyAsync(t => t.Name.ToLower() == request.Name.ToLower() && t.IsActive);

        if (exists)
        {
            return BadRequest(new { error = $"Tag '{request.Name}' already exists" });
        }

        DocumentTag tag = new DocumentTag
        {
            Name = request.Name,
            Color = request.Color,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.DocumentTags.Add(tag);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Tag created: {TagId} with name '{TagName}'", tag.Id, tag.Name);

        TagDto dto = new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Color = tag.Color
        };

        return CreatedAtAction(nameof(GetById), new { id = tag.Id }, dto);
    }

    /// <summary>
    /// Updates a tag
    /// </summary>
    /// <param name="id">Tag ID</param>
    /// <param name="request">Update data</param>
    /// <returns>Updated tag</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(TagDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTagRequest request)
    {
        DocumentTag? tag = await _context.DocumentTags.FindAsync(id);

        if (tag == null || !tag.IsActive)
        {
            return NotFound(new { error = $"Tag with ID {id} not found" });
        }

        // Check for duplicate name (excluding current tag, case-insensitive)
        bool duplicateExists = await _context.DocumentTags
            .AnyAsync(t => t.Id != id &&
                          t.Name.ToLower() == request.Name.ToLower() &&
                          t.IsActive);

        if (duplicateExists)
        {
            return BadRequest(new { error = $"Tag '{request.Name}' already exists" });
        }

        tag.Name = request.Name;
        tag.Color = request.Color;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Tag updated: {TagId} to name '{TagName}'", id, tag.Name);

        TagDto dto = new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Color = tag.Color
        };

        return Ok(dto);
    }

    /// <summary>
    /// Deletes (deactivates) a tag
    /// </summary>
    /// <param name="id">Tag ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        DocumentTag? tag = await _context.DocumentTags.FindAsync(id);

        if (tag == null || !tag.IsActive)
        {
            return NotFound(new { error = $"Tag with ID {id} not found" });
        }

        // Soft delete - preserve existing document-tag associations
        tag.IsActive = false;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Tag deactivated: {TagId}", id);

        return NoContent();
    }
}
```

### TagsController Unit Tests

**File**: `tests/DocsAndPlannings.Core.Tests/Controllers/TagsControllerTests.cs`

```csharp
using DocsAndPlannings.Api.Controllers;
using DocsAndPlannings.Core.Data;
using DocsAndPlannings.Core.DTOs.Documents;
using DocsAndPlannings.Core.DTOs.Tags;
using DocsAndPlannings.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;

namespace DocsAndPlannings.Core.Tests.Controllers;

public class TagsControllerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly TagsController _controller;

    public TagsControllerTests()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        ILogger<TagsController> logger = new LoggerFactory().CreateLogger<TagsController>();
        _controller = new TagsController(_context, logger);
    }

    [Fact]
    public async Task GetAll_ReturnsActiveTagsOnly()
    {
        // Arrange
        DocumentTag activeTag1 = new DocumentTag
        {
            Name = "Active1",
            Color = "#FF0000",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        DocumentTag activeTag2 = new DocumentTag
        {
            Name = "Active2",
            Color = "#00FF00",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        DocumentTag inactiveTag = new DocumentTag
        {
            Name = "Inactive",
            Color = "#0000FF",
            CreatedAt = DateTime.UtcNow,
            IsActive = false
        };

        _context.DocumentTags.AddRange(activeTag1, activeTag2, inactiveTag);
        await _context.SaveChangesAsync();

        // Act
        IActionResult result = await _controller.GetAll();

        // Assert
        OkObjectResult? okResult = Assert.IsType<OkObjectResult>(result);
        List<TagDto>? tags = Assert.IsAssignableFrom<List<TagDto>>(okResult.Value);
        Assert.Equal(2, tags.Count);
        Assert.DoesNotContain(tags, t => t.Name == "Inactive");
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenDuplicateName()
    {
        // Arrange
        DocumentTag existing = new DocumentTag
        {
            Name = "Duplicate",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        _context.DocumentTags.Add(existing);
        await _context.SaveChangesAsync();

        CreateTagRequest request = new CreateTagRequest
        {
            Name = "DUPLICATE" // Case-insensitive check
        };

        // Act
        IActionResult result = await _controller.Create(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Update_UpdatesNameAndColor()
    {
        // Arrange
        DocumentTag tag = new DocumentTag
        {
            Name = "Original",
            Color = "#FF0000",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        _context.DocumentTags.Add(tag);
        await _context.SaveChangesAsync();

        UpdateTagRequest request = new UpdateTagRequest
        {
            Name = "Updated",
            Color = "#00FF00"
        };

        // Act
        IActionResult result = await _controller.Update(tag.Id, request);

        // Assert
        OkObjectResult? okResult = Assert.IsType<OkObjectResult>(result);
        TagDto? dto = Assert.IsType<TagDto>(okResult.Value);
        Assert.Equal("Updated", dto.Name);
        Assert.Equal("#00FF00", dto.Color);

        // Verify in database
        DocumentTag? updated = await _context.DocumentTags.FindAsync(tag.Id);
        Assert.Equal("Updated", updated?.Name);
    }

    [Fact]
    public async Task Delete_SoftDeletesTag()
    {
        // Arrange
        DocumentTag tag = new DocumentTag
        {
            Name = "ToDelete",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        _context.DocumentTags.Add(tag);
        await _context.SaveChangesAsync();

        // Act
        IActionResult result = await _controller.Delete(tag.Id);

        // Assert
        Assert.IsType<NoContentResult>(result);

        DocumentTag? deleted = await _context.DocumentTags.FindAsync(tag.Id);
        Assert.NotNull(deleted);
        Assert.False(deleted.IsActive); // Soft deleted
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
```

### Deliverable
- ✅ TagsController with 4 REST endpoints (GET, POST, PUT, DELETE)
- ✅ CreateTagRequest and UpdateTagRequest DTOs
- ✅ Admin-only tag management (via [Authorize(Roles = "Admin")])
- ✅ Case-insensitive duplicate checking
- ✅ Soft delete preserves tag associations
- ✅ Unit tests for all operations

**Time Estimate**: 4 hours

---

## 3. NEW: Circular Hierarchy Prevention

### Problem
Documents can have a parent document (ParentDocumentId). Without validation, a circular reference could be created:
- Doc A → parent: Doc B
- Doc B → parent: Doc C
- Doc C → parent: Doc A (creates cycle!)

This would cause infinite loops when traversing the hierarchy.

### Solution
Add cycle detection to DocumentService before allowing parent assignment.

### Updated DocumentService Implementation

Add this method to `DocumentService.cs`:

```csharp
/// <summary>
/// Checks if setting a parent would create a circular reference
/// </summary>
/// <param name="documentId">The document being updated</param>
/// <param name="newParentId">The proposed parent document ID</param>
/// <returns>True if a cycle would be created, false otherwise</returns>
private async Task<bool> WouldCreateCycleAsync(int documentId, int newParentId)
{
    // If the new parent is the document itself, that's a cycle
    if (documentId == newParentId)
    {
        return true;
    }

    // Traverse up the hierarchy from the new parent
    int? currentId = newParentId;
    HashSet<int> visited = new HashSet<int>();

    while (currentId.HasValue)
    {
        // If we encounter the document we're updating, there's a cycle
        if (currentId.Value == documentId)
        {
            return true;
        }

        // Detect infinite loop (shouldn't happen with proper data, but safety check)
        if (visited.Contains(currentId.Value))
        {
            _logger.LogWarning("Detected existing circular reference starting at document {DocumentId}",
                currentId.Value);
            return false; // Existing cycle, but not caused by this change
        }

        visited.Add(currentId.Value);

        // Get the parent of the current document
        Document? parent = await _context.Documents
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == currentId.Value && !d.IsDeleted);

        if (parent == null)
        {
            break; // Reached the top of the hierarchy
        }

        currentId = parent.ParentDocumentId;
    }

    return false; // No cycle detected
}
```

### Update CreateAsync Method

Add validation after parent document existence check:

```csharp
public async Task<DocumentDto> CreateAsync(CreateDocumentRequest request, int currentUserId)
{
    Debug.Assert(request != null, "CreateDocumentRequest cannot be null");
    Debug.Assert(currentUserId > 0, "Current user ID must be positive");

    // Validate parent document exists if specified
    if (request.ParentDocumentId.HasValue)
    {
        bool parentExists = await _context.Documents
            .AnyAsync(d => d.Id == request.ParentDocumentId.Value && !d.IsDeleted);

        if (!parentExists)
        {
            throw new NotFoundException($"Parent document with ID {request.ParentDocumentId.Value} not found");
        }

        // Note: No cycle check needed for Create - document doesn't exist yet so can't create a cycle
    }

    // ... rest of CreateAsync implementation
}
```

### Update UpdateAsync Method

Add validation after existing parent validation:

```csharp
// Validate parent document
if (request.ParentDocumentId.HasValue)
{
    // Cannot set self as parent
    if (request.ParentDocumentId.Value == id)
    {
        throw new InvalidOperationException("Document cannot be its own parent");
    }

    bool parentExists = await _context.Documents
        .AnyAsync(d => d.Id == request.ParentDocumentId.Value && !d.IsDeleted);

    if (!parentExists)
    {
        throw new NotFoundException($"Parent document with ID {request.ParentDocumentId.Value} not found");
    }

    // Check for circular hierarchy
    bool wouldCreateCycle = await WouldCreateCycleAsync(id, request.ParentDocumentId.Value);
    if (wouldCreateCycle)
    {
        throw new InvalidOperationException(
            "Setting this parent would create a circular document hierarchy");
    }
}
```

### Unit Tests for Circular Hierarchy

**File**: `tests/DocsAndPlannings.Core.Tests/Services/DocumentServiceHierarchyTests.cs`

```csharp
using DocsAndPlannings.Core.Data;
using DocsAndPlannings.Core.DTOs.Documents;
using DocsAndPlannings.Core.Exceptions;
using DocsAndPlannings.Core.Models;
using DocsAndPlannings.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;

namespace DocsAndPlannings.Core.Tests.Services;

public class DocumentServiceHierarchyTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly DocumentService _service;
    private readonly User _testUser;

    public DocumentServiceHierarchyTests()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        ILogger<DocumentService> logger = new LoggerFactory().CreateLogger<DocumentService>();
        _service = new DocumentService(_context, logger);

        _testUser = new User
        {
            Email = "test@example.com",
            PasswordHash = "hash",
            FirstName = "Test",
            LastName = "User",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        _context.Users.Add(_testUser);
        _context.SaveChanges();
    }

    [Fact]
    public async Task UpdateAsync_ThrowsException_WhenSettingSelfAsParent()
    {
        // Arrange
        Document doc = new Document
        {
            Title = "Test",
            Content = "Content",
            AuthorId = _testUser.Id,
            CurrentVersion = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Documents.Add(doc);
        await _context.SaveChangesAsync();

        UpdateDocumentRequest request = new UpdateDocumentRequest
        {
            Title = "Updated",
            Content = "Content",
            ParentDocumentId = doc.Id, // Self as parent!
            IsPublished = false,
            TagIds = new List<int>()
        };

        // Act & Assert
        InvalidOperationException ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateAsync(doc.Id, request, _testUser.Id, false));

        Assert.Contains("cannot be its own parent", ex.Message);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsException_WhenCreatingDirectCycle()
    {
        // Arrange
        // Doc A → parent: Doc B
        // Attempt: Doc B → parent: Doc A (would create cycle)

        Document docA = new Document
        {
            Title = "Doc A",
            Content = "Content",
            AuthorId = _testUser.Id,
            CurrentVersion = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Document docB = new Document
        {
            Title = "Doc B",
            Content = "Content",
            AuthorId = _testUser.Id,
            CurrentVersion = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Documents.AddRange(docA, docB);
        await _context.SaveChangesAsync();

        // Set Doc A's parent to Doc B
        docA.ParentDocumentId = docB.Id;
        await _context.SaveChangesAsync();

        // Now try to set Doc B's parent to Doc A (creates cycle)
        UpdateDocumentRequest request = new UpdateDocumentRequest
        {
            Title = "Doc B",
            Content = "Content",
            ParentDocumentId = docA.Id, // Would create cycle!
            IsPublished = false,
            TagIds = new List<int>()
        };

        // Act & Assert
        InvalidOperationException ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateAsync(docB.Id, request, _testUser.Id, false));

        Assert.Contains("circular", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsException_WhenCreatingIndirectCycle()
    {
        // Arrange
        // Doc A → parent: Doc B
        // Doc B → parent: Doc C
        // Attempt: Doc C → parent: Doc A (would create cycle)

        Document docA = new Document
        {
            Title = "Doc A",
            Content = "Content",
            AuthorId = _testUser.Id,
            CurrentVersion = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Document docB = new Document
        {
            Title = "Doc B",
            Content = "Content",
            AuthorId = _testUser.Id,
            CurrentVersion = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Document docC = new Document
        {
            Title = "Doc C",
            Content = "Content",
            AuthorId = _testUser.Id,
            CurrentVersion = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Documents.AddRange(docA, docB, docC);
        await _context.SaveChangesAsync();

        // Set up chain: A → B → C
        docA.ParentDocumentId = docB.Id;
        docB.ParentDocumentId = docC.Id;
        await _context.SaveChangesAsync();

        // Now try to set C's parent to A (creates cycle: A → B → C → A)
        UpdateDocumentRequest request = new UpdateDocumentRequest
        {
            Title = "Doc C",
            Content = "Content",
            ParentDocumentId = docA.Id, // Would create cycle!
            IsPublished = false,
            TagIds = new List<int>()
        };

        // Act & Assert
        InvalidOperationException ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateAsync(docC.Id, request, _testUser.Id, false));

        Assert.Contains("circular", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateAsync_Succeeds_WhenValidHierarchy()
    {
        // Arrange
        // Valid: Doc A → parent: Doc B (no cycle)

        Document docA = new Document
        {
            Title = "Doc A",
            Content = "Content",
            AuthorId = _testUser.Id,
            CurrentVersion = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Document docB = new Document
        {
            Title = "Doc B",
            Content = "Content",
            AuthorId = _testUser.Id,
            CurrentVersion = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Documents.AddRange(docA, docB);
        await _context.SaveChangesAsync();

        UpdateDocumentRequest request = new UpdateDocumentRequest
        {
            Title = "Doc A Updated",
            Content = "Content",
            ParentDocumentId = docB.Id, // Valid parent
            IsPublished = false,
            TagIds = new List<int>()
        };

        // Act
        DocumentDto result = await _service.UpdateAsync(docA.Id, request, _testUser.Id, false);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(docB.Id, result.ParentDocumentId);
        Assert.Equal("Doc B", result.ParentDocumentTitle);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
```

### Deliverable
- ✅ Circular hierarchy detection via graph traversal
- ✅ Prevents self-referencing (Doc A → parent: Doc A)
- ✅ Prevents direct cycles (A → B → A)
- ✅ Prevents indirect cycles (A → B → C → A)
- ✅ Comprehensive unit tests (4 scenarios)
- ✅ Clear error messages for users

**Time Estimate**: 2 hours

---

## 4. Phase 2.2 Complete Specification

### Phase 2.2: Advanced Documentation Features (Week 3)

**Goal**: Add file uploads, image embedding, and cleanup

**Prerequisites**: Phase 2.1 complete and all tests passing

---

### Task 1: Create DocumentAttachment Model (2 hours)

**File**: `source/DocsAndPlannings.Core/Models/DocumentAttachment.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace DocsAndPlannings.Core.Models;

/// <summary>
/// Represents a file attachment (screenshot, image) associated with a document
/// </summary>
public class DocumentAttachment
{
    public int Id { get; set; }

    public int DocumentId { get; set; }
    public Document Document { get; set; } = null!;

    [MaxLength(255)]
    public required string FileName { get; set; }

    [MaxLength(500)]
    public required string FilePath { get; set; }

    public long FileSize { get; set; }

    [MaxLength(100)]
    public required string ContentType { get; set; }

    public int UploadedById { get; set; }
    public User UploadedBy { get; set; } = null!;

    public DateTime UploadedAt { get; set; }
}
```

**Update ApplicationDbContext.cs** - Add to `ConfigureDocumentEntities()`:

```csharp
modelBuilder.Entity<DocumentAttachment>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
    entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
    entity.Property(e => e.FileSize).IsRequired();
    entity.Property(e => e.ContentType).IsRequired().HasMaxLength(100);
    entity.Property(e => e.UploadedAt).IsRequired();

    entity.HasOne(e => e.Document)
        .WithMany(d => d.Attachments)
        .HasForeignKey(e => e.DocumentId)
        .OnDelete(DeleteBehavior.Cascade);

    entity.HasOne(e => e.UploadedBy)
        .WithMany()
        .HasForeignKey(e => e.UploadedById)
        .OnDelete(DeleteBehavior.Restrict);
});
```

**Update Document.cs** - Add navigation property:

```csharp
public ICollection<DocumentAttachment> Attachments { get; set; } = new List<DocumentAttachment>();
```

**Create migration**:
```bash
cd source/DocsAndPlannings.Api
dotnet ef migrations add AddDocumentAttachment --project ../DocsAndPlannings.Core
```

---

### Task 2: Create IFileStorageService (2 hours)

**File**: `source/DocsAndPlannings.Core/Services/IFileStorageService.cs`

```csharp
namespace DocsAndPlannings.Core.Services;

/// <summary>
/// Service for managing file storage on the filesystem
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Saves a file to the filesystem
    /// </summary>
    /// <param name="documentId">The document this file belongs to</param>
    /// <param name="fileName">Original filename</param>
    /// <param name="contentType">MIME type of the file</param>
    /// <param name="fileStream">File content stream</param>
    /// <returns>Saved file path and size</returns>
    Task<(string FilePath, long FileSize)> SaveFileAsync(
        int documentId,
        string fileName,
        string contentType,
        Stream fileStream);

    /// <summary>
    /// Retrieves a file from the filesystem
    /// </summary>
    /// <param name="filePath">Path to the file</param>
    /// <returns>File stream and content type</returns>
    Task<(Stream FileStream, string ContentType)> GetFileAsync(string filePath);

    /// <summary>
    /// Deletes a file from the filesystem
    /// </summary>
    /// <param name="filePath">Path to the file to delete</param>
    Task DeleteFileAsync(string filePath);

    /// <summary>
    /// Deletes all files for a document
    /// </summary>
    /// <param name="documentId">Document ID</param>
    Task DeleteDocumentFilesAsync(int documentId);

    /// <summary>
    /// Validates if a file type is allowed
    /// </summary>
    /// <param name="contentType">MIME type</param>
    /// <returns>True if allowed, false otherwise</returns>
    bool IsAllowedFileType(string contentType);

    /// <summary>
    /// Validates if a file size is within limits
    /// </summary>
    /// <param name="fileSize">File size in bytes</param>
    /// <returns>True if within limit, false otherwise</returns>
    bool IsValidFileSize(long fileSize);
}
```

---

### Task 3: Implement FileStorageService (4 hours)

**File**: `source/DocsAndPlannings.Core/Services/FileStorageService.cs`

```csharp
using System.Diagnostics;
using DocsAndPlannings.Core.Exceptions;

namespace DocsAndPlannings.Core.Services;

public class FileStorageService : IFileStorageService
{
    private readonly string _baseStoragePath;
    private readonly ILogger<FileStorageService> _logger;

    // Configuration constants
    private const long MAX_FILE_SIZE = 10 * 1024 * 1024; // 10 MB
    private static readonly HashSet<string> AllowedContentTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/gif",
        "image/webp",
        "image/bmp"
    };

    public FileStorageService(IConfiguration configuration, ILogger<FileStorageService> logger)
    {
        Debug.Assert(configuration != null, "Configuration cannot be null");
        Debug.Assert(logger != null, "Logger cannot be null");

        _logger = logger;

        // Get storage path from configuration, default to "screenshots" folder
        _baseStoragePath = configuration["FileStorage:BasePath"] ?? "screenshots";

        // Ensure base directory exists
        if (!Directory.Exists(_baseStoragePath))
        {
            Directory.CreateDirectory(_baseStoragePath);
            _logger.LogInformation("Created storage directory: {Path}", _baseStoragePath);
        }
    }

    public async Task<(string FilePath, long FileSize)> SaveFileAsync(
        int documentId,
        string fileName,
        string contentType,
        Stream fileStream)
    {
        Debug.Assert(documentId > 0, "Document ID must be positive");
        Debug.Assert(!string.IsNullOrWhiteSpace(fileName), "Filename cannot be empty");
        Debug.Assert(fileStream != null, "File stream cannot be null");

        // Validate file type
        if (!IsAllowedFileType(contentType))
        {
            throw new BadRequestException($"File type '{contentType}' is not allowed. Only images are permitted.");
        }

        // Validate file size
        if (!IsValidFileSize(fileStream.Length))
        {
            throw new BadRequestException($"File size exceeds maximum allowed size of {MAX_FILE_SIZE / (1024 * 1024)} MB.");
        }

        try
        {
            // Create document-specific directory
            string documentDir = Path.Combine(_baseStoragePath, documentId.ToString());
            if (!Directory.Exists(documentDir))
            {
                Directory.CreateDirectory(documentDir);
            }

            // Generate unique filename to prevent overwrites
            string extension = Path.GetExtension(fileName);
            string safeFileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{extension}";
            string fullPath = Path.Combine(documentDir, safeFileName);

            // Save file
            using (FileStream fileStreamOutput = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
            {
                await fileStream.CopyToAsync(fileStreamOutput);
            }

            long fileSize = new FileInfo(fullPath).Length;

            _logger.LogInformation("File saved: {FilePath}, Size: {FileSize} bytes", fullPath, fileSize);

            // Return relative path for database storage
            string relativePath = Path.Combine(documentId.ToString(), safeFileName);
            return (relativePath, fileSize);
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Failed to save file for document {DocumentId}", documentId);
            throw new InvalidOperationException("Failed to save file to storage", ex);
        }
    }

    public async Task<(Stream FileStream, string ContentType)> GetFileAsync(string filePath)
    {
        Debug.Assert(!string.IsNullOrWhiteSpace(filePath), "File path cannot be empty");

        string fullPath = Path.Combine(_baseStoragePath, filePath);

        if (!File.Exists(fullPath))
        {
            throw new NotFoundException($"File not found: {filePath}");
        }

        try
        {
            // Determine content type from extension
            string extension = Path.GetExtension(fullPath).ToLowerInvariant();
            string contentType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".bmp" => "image/bmp",
                _ => "application/octet-stream"
            };

            // Return file stream
            FileStream fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);

            return (fileStream, contentType);
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Failed to retrieve file: {FilePath}", filePath);
            throw new InvalidOperationException("Failed to retrieve file from storage", ex);
        }
    }

    public async Task DeleteFileAsync(string filePath)
    {
        Debug.Assert(!string.IsNullOrWhiteSpace(filePath), "File path cannot be empty");

        string fullPath = Path.Combine(_baseStoragePath, filePath);

        if (!File.Exists(fullPath))
        {
            _logger.LogWarning("Attempted to delete non-existent file: {FilePath}", filePath);
            return; // Idempotent - not an error if already deleted
        }

        try
        {
            File.Delete(fullPath);
            _logger.LogInformation("File deleted: {FilePath}", filePath);
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Failed to delete file: {FilePath}", filePath);
            throw new InvalidOperationException("Failed to delete file from storage", ex);
        }
    }

    public async Task DeleteDocumentFilesAsync(int documentId)
    {
        Debug.Assert(documentId > 0, "Document ID must be positive");

        string documentDir = Path.Combine(_baseStoragePath, documentId.ToString());

        if (!Directory.Exists(documentDir))
        {
            _logger.LogWarning("Attempted to delete files for non-existent document directory: {DocumentId}", documentId);
            return; // Idempotent
        }

        try
        {
            Directory.Delete(documentDir, recursive: true);
            _logger.LogInformation("Deleted all files for document {DocumentId}", documentId);
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Failed to delete files for document {DocumentId}", documentId);
            throw new InvalidOperationException($"Failed to delete files for document {documentId}", ex);
        }
    }

    public bool IsAllowedFileType(string contentType)
    {
        return AllowedContentTypes.Contains(contentType);
    }

    public bool IsValidFileSize(long fileSize)
    {
        return fileSize > 0 && fileSize <= MAX_FILE_SIZE;
    }
}
```

**Add to appsettings.json**:
```json
{
  "FileStorage": {
    "BasePath": "screenshots"
  }
}
```

**Register in Program.cs**:
```csharp
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
```

---

### Task 4: Add File Upload Endpoints (4 hours)

**Create DTOs**:

**File**: `source/DocsAndPlannings.Core/DTOs/Documents/DocumentAttachmentDto.cs`

```csharp
namespace DocsAndPlannings.Core.DTOs.Documents;

public class DocumentAttachmentDto
{
    public int Id { get; set; }
    public int DocumentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string UploadedByName { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public string Url { get; set; } = string.Empty; // API URL to retrieve file
}
```

**Update DocumentsController** - Add these methods:

```csharp
/// <summary>
/// Uploads a file attachment to a document
/// </summary>
/// <param name="id">Document ID</param>
/// <param name="file">File to upload</param>
/// <returns>Attachment metadata</returns>
[HttpPost("{id}/attachments")]
[ProducesResponseType(typeof(DocumentAttachmentDto), StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
public async Task<IActionResult> UploadAttachment(int id, IFormFile file)
{
    if (file == null || file.Length == 0)
    {
        return BadRequest(new { error = "No file provided" });
    }

    int currentUserId = GetCurrentUserId();

    // Verify document exists and user is author
    Document? document = await _context.Documents
        .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

    if (document == null)
    {
        return NotFound(new { error = $"Document with ID {id} not found" });
    }

    bool isAdmin = User.IsInRole("Admin");
    if (document.AuthorId != currentUserId && !isAdmin)
    {
        return Forbid();
    }

    // Validate file
    if (!_fileStorageService.IsAllowedFileType(file.ContentType))
    {
        return BadRequest(new { error = $"File type '{file.ContentType}' not allowed. Only images are permitted." });
    }

    if (!_fileStorageService.IsValidFileSize(file.Length))
    {
        return StatusCode(StatusCodes.Status413PayloadTooLarge,
            new { error = "File size exceeds 10 MB limit" });
    }

    // Save file
    using Stream fileStream = file.OpenReadStream();
    (string filePath, long fileSize) = await _fileStorageService.SaveFileAsync(
        id,
        file.FileName,
        file.ContentType,
        fileStream);

    // Create attachment record
    DocumentAttachment attachment = new DocumentAttachment
    {
        DocumentId = id,
        FileName = file.FileName,
        FilePath = filePath,
        FileSize = fileSize,
        ContentType = file.ContentType,
        UploadedById = currentUserId,
        UploadedAt = DateTime.UtcNow
    };

    _context.Set<DocumentAttachment>().Add(attachment);
    await _context.SaveChangesAsync();

    _logger.LogInformation("Attachment uploaded: {AttachmentId} for document {DocumentId} by user {UserId}",
        attachment.Id, id, currentUserId);

    User? uploader = await _context.Users.FindAsync(currentUserId);

    DocumentAttachmentDto dto = new DocumentAttachmentDto
    {
        Id = attachment.Id,
        DocumentId = attachment.DocumentId,
        FileName = attachment.FileName,
        FileSize = attachment.FileSize,
        ContentType = attachment.ContentType,
        UploadedByName = $"{uploader?.FirstName} {uploader?.LastName}",
        UploadedAt = attachment.UploadedAt,
        Url = Url.Action(nameof(GetAttachment), new { id, attachmentId = attachment.Id }) ?? string.Empty
    };

    return CreatedAtAction(nameof(GetAttachment), new { id, attachmentId = attachment.Id }, dto);
}

/// <summary>
/// Gets an attachment file
/// </summary>
/// <param name="id">Document ID</param>
/// <param name="attachmentId">Attachment ID</param>
/// <returns>File content</returns>
[HttpGet("{id}/attachments/{attachmentId}")]
[ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> GetAttachment(int id, int attachmentId)
{
    int currentUserId = GetCurrentUserId();
    bool isAdmin = User.IsInRole("Admin");

    // Load attachment with document
    DocumentAttachment? attachment = await _context.Set<DocumentAttachment>()
        .Include(a => a.Document)
        .FirstOrDefaultAsync(a => a.Id == attachmentId && a.DocumentId == id);

    if (attachment == null)
    {
        return NotFound(new { error = $"Attachment with ID {attachmentId} not found" });
    }

    // Check authorization (same rules as viewing document)
    if (attachment.Document.AuthorId != currentUserId &&
        !attachment.Document.IsPublished &&
        !isAdmin)
    {
        return Forbid();
    }

    // Retrieve file
    (Stream fileStream, string contentType) = await _fileStorageService.GetFileAsync(attachment.FilePath);

    return File(fileStream, contentType, attachment.FileName);
}

/// <summary>
/// Lists all attachments for a document
/// </summary>
/// <param name="id">Document ID</param>
/// <returns>List of attachments</returns>
[HttpGet("{id}/attachments")]
[ProducesResponseType(typeof(List<DocumentAttachmentDto>), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> ListAttachments(int id)
{
    int currentUserId = GetCurrentUserId();
    bool isAdmin = User.IsInRole("Admin");

    Document? document = await _context.Documents
        .Include(d => d.Attachments)
            .ThenInclude(a => a.UploadedBy)
        .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

    if (document == null)
    {
        return NotFound(new { error = $"Document with ID {id} not found" });
    }

    // Check authorization
    if (document.AuthorId != currentUserId && !document.IsPublished && !isAdmin)
    {
        return Forbid();
    }

    List<DocumentAttachmentDto> dtos = document.Attachments.Select(a => new DocumentAttachmentDto
    {
        Id = a.Id,
        DocumentId = a.DocumentId,
        FileName = a.FileName,
        FileSize = a.FileSize,
        ContentType = a.ContentType,
        UploadedByName = $"{a.UploadedBy.FirstName} {a.UploadedBy.LastName}",
        UploadedAt = a.UploadedAt,
        Url = Url.Action(nameof(GetAttachment), new { id, attachmentId = a.Id }) ?? string.Empty
    }).ToList();

    return Ok(dtos);
}

/// <summary>
/// Deletes an attachment
/// </summary>
/// <param name="id">Document ID</param>
/// <param name="attachmentId">Attachment ID</param>
/// <returns>No content on success</returns>
[HttpDelete("{id}/attachments/{attachmentId}")]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> DeleteAttachment(int id, int attachmentId)
{
    int currentUserId = GetCurrentUserId();
    bool isAdmin = User.IsInRole("Admin");

    DocumentAttachment? attachment = await _context.Set<DocumentAttachment>()
        .Include(a => a.Document)
        .FirstOrDefaultAsync(a => a.Id == attachmentId && a.DocumentId == id);

    if (attachment == null)
    {
        return NotFound(new { error = $"Attachment with ID {attachmentId} not found" });
    }

    // Only author or admin can delete
    if (attachment.Document.AuthorId != currentUserId && !isAdmin)
    {
        return Forbid();
    }

    // Delete file from filesystem
    await _fileStorageService.DeleteFileAsync(attachment.FilePath);

    // Delete database record
    _context.Set<DocumentAttachment>().Remove(attachment);
    await _context.SaveChangesAsync();

    _logger.LogInformation("Attachment deleted: {AttachmentId} from document {DocumentId}",
        attachmentId, id);

    return NoContent();
}
```

**Update DocumentsController Constructor** - Inject IFileStorageService:

```csharp
private readonly IFileStorageService _fileStorageService;

public DocumentsController(
    IDocumentService documentService,
    ApplicationDbContext context,
    IFileStorageService fileStorageService,
    ILogger<DocumentsController> logger)
{
    _documentService = documentService;
    _context = context;
    _fileStorageService = fileStorageService;
    _logger = logger;
}
```

---

### Task 5: Update DocumentService DeleteAsync (1 hour)

Add file cleanup when document is soft-deleted:

```csharp
public async Task DeleteAsync(int id, int currentUserId, bool isAdmin)
{
    Debug.Assert(currentUserId > 0, "Current user ID must be positive");

    Document? document = await _context.Documents
        .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

    if (document == null)
    {
        throw new NotFoundException($"Document with ID {id} not found");
    }

    // Check authorization
    if (document.AuthorId != currentUserId && !isAdmin)
    {
        throw new ForbiddenException("You do not have permission to delete this document");
    }

    // Soft delete document
    document.IsDeleted = true;
    document.UpdatedAt = DateTime.UtcNow;

    await _context.SaveChangesAsync();

    _logger.LogInformation("Document soft-deleted: {DocumentId} by user {UserId}", id, currentUserId);

    // Delete associated files asynchronously (fire and forget - don't block)
    _ = Task.Run(async () =>
    {
        try
        {
            await _fileStorageService.DeleteDocumentFilesAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete files for document {DocumentId}", id);
            // Don't throw - file cleanup failure shouldn't prevent document deletion
        }
    });
}
```

**Update DocumentService Constructor** - Inject IFileStorageService:

```csharp
private readonly IFileStorageService _fileStorageService;

public DocumentService(
    ApplicationDbContext context,
    IFileStorageService fileStorageService,
    ILogger<DocumentService> logger)
{
    Debug.Assert(context != null, "ApplicationDbContext cannot be null");
    Debug.Assert(fileStorageService != null, "FileStorageService cannot be null");
    Debug.Assert(logger != null, "Logger cannot be null");

    _context = context;
    _fileStorageService = fileStorageService;
    _logger = logger;
}
```

---

### Task 6: Create Comprehensive Tests (4 hours)

**File**: `tests/DocsAndPlannings.Core.Tests/Services/FileStorageServiceTests.cs`

```csharp
using DocsAndPlannings.Core.Exceptions;
using DocsAndPlannings.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xunit;

namespace DocsAndPlannings.Core.Tests.Services;

public class FileStorageServiceTests : IDisposable
{
    private readonly FileStorageService _service;
    private readonly string _testStoragePath;

    public FileStorageServiceTests()
    {
        _testStoragePath = Path.Combine(Path.GetTempPath(), $"test_storage_{Guid.NewGuid():N}");

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["FileStorage:BasePath"] = _testStoragePath
            })
            .Build();

        ILogger<FileStorageService> logger = new LoggerFactory().CreateLogger<FileStorageService>();
        _service = new FileStorageService(configuration, logger);
    }

    [Fact]
    public async Task SaveFileAsync_SavesFileToCorrectLocation()
    {
        // Arrange
        int documentId = 123;
        string fileName = "test.png";
        string contentType = "image/png";
        byte[] fileContent = new byte[] { 1, 2, 3, 4, 5 };
        using MemoryStream stream = new MemoryStream(fileContent);

        // Act
        (string filePath, long fileSize) = await _service.SaveFileAsync(documentId, fileName, contentType, stream);

        // Assert
        Assert.NotNull(filePath);
        Assert.Equal(fileContent.Length, fileSize);
        Assert.Contains(documentId.ToString(), filePath);

        // Verify file exists
        string fullPath = Path.Combine(_testStoragePath, filePath);
        Assert.True(File.Exists(fullPath));

        // Verify content
        byte[] savedContent = await File.ReadAllBytesAsync(fullPath);
        Assert.Equal(fileContent, savedContent);
    }

    [Fact]
    public async Task SaveFileAsync_ThrowsException_WhenInvalidFileType()
    {
        // Arrange
        using MemoryStream stream = new MemoryStream(new byte[] { 1, 2, 3 });

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(
            () => _service.SaveFileAsync(1, "test.exe", "application/exe", stream));
    }

    [Fact]
    public async Task GetFileAsync_RetrievesFile()
    {
        // Arrange
        int documentId = 456;
        byte[] fileContent = new byte[] { 10, 20, 30 };
        using MemoryStream stream = new MemoryStream(fileContent);

        (string filePath, _) = await _service.SaveFileAsync(documentId, "test.jpg", "image/jpeg", stream);

        // Act
        (Stream retrievedStream, string contentType) = await _service.GetFileAsync(filePath);

        // Assert
        Assert.Equal("image/jpeg", contentType);

        using MemoryStream ms = new MemoryStream();
        await retrievedStream.CopyToAsync(ms);
        Assert.Equal(fileContent, ms.ToArray());

        retrievedStream.Dispose();
    }

    [Fact]
    public async Task DeleteFileAsync_DeletesFile()
    {
        // Arrange
        int documentId = 789;
        using MemoryStream stream = new MemoryStream(new byte[] { 1 });
        (string filePath, _) = await _service.SaveFileAsync(documentId, "test.png", "image/png", stream);

        string fullPath = Path.Combine(_testStoragePath, filePath);
        Assert.True(File.Exists(fullPath));

        // Act
        await _service.DeleteFileAsync(filePath);

        // Assert
        Assert.False(File.Exists(fullPath));
    }

    [Fact]
    public async Task DeleteDocumentFilesAsync_DeletesAllFiles()
    {
        // Arrange
        int documentId = 999;

        // Create multiple files
        using MemoryStream stream1 = new MemoryStream(new byte[] { 1 });
        using MemoryStream stream2 = new MemoryStream(new byte[] { 2 });

        await _service.SaveFileAsync(documentId, "file1.png", "image/png", stream1);
        await _service.SaveFileAsync(documentId, "file2.jpg", "image/jpeg", stream2);

        string docDir = Path.Combine(_testStoragePath, documentId.ToString());
        Assert.True(Directory.Exists(docDir));
        Assert.Equal(2, Directory.GetFiles(docDir).Length);

        // Act
        await _service.DeleteDocumentFilesAsync(documentId);

        // Assert
        Assert.False(Directory.Exists(docDir));
    }

    [Fact]
    public void IsAllowedFileType_ReturnsTrueForImages()
    {
        Assert.True(_service.IsAllowedFileType("image/jpeg"));
        Assert.True(_service.IsAllowedFileType("image/png"));
        Assert.True(_service.IsAllowedFileType("image/gif"));
    }

    [Fact]
    public void IsAllowedFileType_ReturnsFalseForNonImages()
    {
        Assert.False(_service.IsAllowedFileType("application/pdf"));
        Assert.False(_service.IsAllowedFileType("text/plain"));
        Assert.False(_service.IsAllowedFileType("video/mp4"));
    }

    [Fact]
    public void IsValidFileSize_ReturnsTrueForValidSizes()
    {
        Assert.True(_service.IsValidFileSize(1024)); // 1 KB
        Assert.True(_service.IsValidFileSize(5 * 1024 * 1024)); // 5 MB
    }

    [Fact]
    public void IsValidFileSize_ReturnsFalseForInvalidSizes()
    {
        Assert.False(_service.IsValidFileSize(0)); // Zero size
        Assert.False(_service.IsValidFileSize(-1)); // Negative
        Assert.False(_service.IsValidFileSize(11 * 1024 * 1024)); // Over 10 MB
    }

    public void Dispose()
    {
        // Cleanup test directory
        if (Directory.Exists(_testStoragePath))
        {
            try
            {
                Directory.Delete(_testStoragePath, recursive: true);
            }
            catch
            {
                // Best effort cleanup
            }
        }
    }
}
```

---

### Phase 2.2 Deliverables Summary

After completing all tasks in Phase 2.2, you will have:

- ✅ DocumentAttachment model with database migration
- ✅ IFileStorageService interface
- ✅ FileStorageService implementation with file validation
- ✅ 4 new DocumentsController endpoints for file operations
- ✅ File upload with size (10 MB) and type (images only) validation
- ✅ File retrieval with proper content types
- ✅ File cleanup on document deletion
- ✅ Comprehensive unit tests for file operations
- ✅ Screenshots stored in `screenshots/{documentId}/` structure

**Testing Checklist**:
- [ ] Upload image file to document (JPG, PNG, GIF)
- [ ] Reject non-image file types (PDF, TXT, EXE)
- [ ] Reject files over 10 MB
- [ ] Retrieve uploaded file via API
- [ ] List all attachments for a document
- [ ] Delete individual attachment
- [ ] Verify files deleted when document deleted
- [ ] All unit tests passing

**Total Phase 2.2 Time Estimate**: 17 hours (2-3 days)

---

## Updated Phase 2 Complete Timeline

### Phase 2.1: Core Documentation Features
- **Original Estimate**: 39 hours (Tasks 1-6)
- **With Additions**:
  - Exception classes (Task 0): 1 hour
  - Verify DocumentTagMap (Task 0.5): 1 hour
  - TagsController (Task 7): 4 hours
  - Circular hierarchy (integrated into Task 3): 2 hours
- **New Total**: 47 hours (~6 days)

### Phase 2.2: Advanced Documentation Features
- **Total**: 17 hours (~2-3 days)

### Phase 2 Complete
- **Total Time**: 64 hours (~8-9 days for 1 developer)
- **Calendar Time**: 2-3 weeks (accounting for testing, reviews, breaks)

---

## Final Validation Checklist

**Before Starting Implementation**:
- [x] Exception classes created in proper namespace
- [x] DocumentTagMap verified and properly configured
- [x] Phase 2.1 scope clarified (no file uploads)
- [x] TagsController specification complete
- [x] Circular hierarchy prevention specified
- [x] Phase 2.2 complete specification
- [ ] Git branch created (feature/phase-2-documentation-module)
- [ ] All validation issues addressed

**Ready to Proceed**: ✅ YES

---

## Implementation Notes

### Using Exception Classes

In all service and controller code, replace inline exception definitions with:

```csharp
using DocsAndPlannings.Core.Exceptions;

// Then use throughout:
throw new NotFoundException("Document not found");
throw new ForbiddenException("Access denied");
throw new BadRequestException("Invalid request");
```

### File Storage Configuration

Add to `.gitignore`:
```
screenshots/
```

This ensures uploaded files are not committed (per CLAUDE.md rule #20).

### Testing Strategy

1. **Unit Tests First**: Write tests for DocumentService, FileStorageService
2. **Integration Tests**: Test controller endpoints with in-memory database
3. **Manual Testing**: Use Swagger/Postman to test file uploads
4. **Bug Hunting**: Use bug-hunter-tester-SKILL after implementation complete
5. **Code Review**: Use code-review-task-creator-SKILL before PR

---

## Conclusion

This addendum provides complete specifications for all identified gaps and improvements. The Phase 2 implementation plan is now comprehensive, validated, and ready for development.

**Next Step**: Create git branch and begin Phase 2.1 implementation following the original guide + this addendum.
