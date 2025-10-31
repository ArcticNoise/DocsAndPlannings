# Phase 2: Documentation Module - Implementation Guide

## Executive Summary

**Solution**: Confluence-like documentation management system with markdown support, versioning, and file attachments
**Approach**: Service-layer architecture with RESTful API, filesystem-based storage, and EF Core search
**Timeline**: 2-3 weeks (phased implementation)
**Team**: 1 full-stack developer

**Key Decisions**:
- **File Storage**: Filesystem-based (screenshots folder) with database metadata - simple, version control friendly
- **Search**: EF Core LINQ with basic text matching for MVP - upgrade to full-text search in future if needed
- **Markdown**: Store as-is, render client-side - offloads processing, enables rich editing
- **Authorization**: Document-level access control with Author/Published model - simple and effective
- **Versioning**: Automatic version creation on update - full history tracking built-in

---

## 1. Problem Definition & Context

### Problem Statement
Need to implement a documentation management system that allows users to create, organize, and version markdown documents with screenshot support, similar to Confluence but tailored for the DocsAndPlannings platform.

### Goals

**Primary**:
- Enable users to create and manage markdown documentation
- Provide hierarchical document organization (parent/child)
- Track complete version history of all changes
- Support screenshot/image uploads and embedding
- Enable document discovery through search and tags

**Secondary**:
- Provide document access control (drafts vs published)
- Support document templates (future)
- Enable document export (future)

### Current State

**Technology**:
- ASP.NET Core 9.0 Web API
- Entity Framework Core 9.0 with SQLite
- JWT authentication with role-based authorization
- Clean architecture (Core/Api/Web separation)

**Architecture**:
- Service layer pattern with interface-based DI
- Direct DbContext usage (no repository pattern)
- DTO pattern for API contracts
- Comprehensive unit testing with in-memory database

**Database Schema (Already Defined)**:
- ✅ Document model with parent/child hierarchy
- ✅ DocumentVersion for history tracking
- ✅ DocumentTag and DocumentTagMap for categorization
- ✅ All relationships configured in ApplicationDbContext

**Pain Points**:
- No document functionality exists yet (models only)
- Need to decide file storage strategy
- Need to determine search approach
- Versioning logic needs implementation

### Requirements

**Functional**:
- [x] Create documents with markdown content
- [x] Update documents (auto-create version)
- [x] Delete documents (soft delete)
- [x] View document with specific version
- [x] List all versions of a document
- [x] Organize documents in hierarchy (parent/child)
- [x] Upload screenshots/images
- [x] Tag documents for categorization
- [x] Search documents by title/content
- [x] Filter documents by author, tags, published status
- [x] Access control (author can edit, published docs are readable by all authenticated users)

**Non-Functional**:
- **Performance**: List operations <200ms, single document retrieval <100ms
- **Scale**: Support 10,000 documents, 1,000 concurrent users
- **Security**: JWT authentication required, author-only edit access
- **Maintainability**: Follow CLAUDE.md standards, 80%+ test coverage
- **Storage**: Screenshots stored locally (not in database for performance)

### Constraints

**Technical**:
- Must use existing stack (ASP.NET Core, EF Core, SQLite)
- Must follow CLAUDE.md mandatory rules
- No npm/additional package managers
- Nullable reference types enabled
- Warnings as errors

**Business**:
- Phase 2.1 (Core) before Phase 2.2 (Advanced)
- Must work with existing authentication
- Git branch required (per CLAUDE.md rule #17)

### Success Criteria

- [x] All CRUD operations working with proper authorization
- [x] Versioning automatically tracks all changes
- [x] Screenshots uploadable and accessible
- [x] Search returns relevant documents
- [x] Unit tests: 80%+ coverage, all passing
- [x] Build: 0 warnings, 0 errors
- [x] API documented with XML comments

---

## 2. Solution Research & Analysis

### Key Architectural Decision #1: File Storage Strategy

#### Approach 1: Filesystem Storage ✅ RECOMMENDED

**Overview**: Store uploaded files in `screenshots/` folder with metadata in database

**Architecture**:
```
POST /api/Documents/{id}/attachments
    ↓
DocumentService validates file
    ↓
FileStorageService saves to:
  screenshots/{documentId}/{filename}
    ↓
Database stores metadata:
  - FilePath
  - FileName
  - FileSize
  - UploadedAt
    ↓
GET /api/Documents/attachments/{id}
  returns file stream
```

**Pros**:
- ✅ Simple implementation (System.IO)
- ✅ Version control friendly (git can ignore screenshots folder per CLAUDE.md rule #20)
- ✅ Easy backup/restore
- ✅ No database bloat
- ✅ Fast file serving (static files)
- ✅ Easy to browse/debug

**Cons**:
- ❌ Need to handle file cleanup on document deletion
- ❌ Requires filesystem permissions
- ❌ Not ideal for distributed/cloud deployment (but fine for MVP)

**Cost**: $0 (local storage)
**Complexity**: Low
**Maturity**: Proven pattern

---

#### Approach 2: Database Storage (BLOB)

**Overview**: Store file bytes directly in SQLite as BLOB column

**Pros**:
- ✅ Transactional integrity (file and metadata together)
- ✅ Simpler deployment (single DB file)
- ✅ No filesystem permissions needed

**Cons**:
- ❌ Database bloat (100MB of images = 100MB+ DB growth)
- ❌ Slower performance (large queries)
- ❌ SQLite not optimized for large BLOBs
- ❌ Difficult to browse/manage files
- ❌ Can't use static file serving

**Cost**: $0
**Complexity**: Low-Medium
**Not Recommended**: Performance concerns with SQLite

---

#### Approach 3: Cloud Storage (Azure Blob/AWS S3)

**Overview**: Upload files to cloud storage service

**Pros**:
- ✅ Highly scalable
- ✅ CDN integration for fast delivery
- ✅ Built-in redundancy

**Cons**:
- ❌ Additional cost ($5-20/month)
- ❌ Additional complexity (SDK, authentication)
- ❌ Requires cloud account
- ❌ Overkill for MVP
- ❌ Against CLAUDE.md simplicity principle

**Cost**: $5-20/month
**Complexity**: Medium-High
**Not Recommended**: Overengineering for current scale

---

**Decision**: **Filesystem Storage (Approach 1)**

**Rationale**:
1. Aligns with CLAUDE.md rule #20 (screenshots stored locally, not committed)
2. Simple implementation, perfect for MVP scale
3. Easy to migrate to cloud storage later if needed
4. Familiar pattern for team
5. Zero additional infrastructure cost

---

### Key Architectural Decision #2: Search Implementation

#### Approach 1: EF Core LINQ (Basic Search) ✅ RECOMMENDED FOR MVP

**Overview**: Use `Contains()` for title/content matching

**Implementation**:
```csharp
var query = _context.Documents
    .Where(d => !d.IsDeleted);

if (!string.IsNullOrWhiteSpace(searchTerm))
{
    query = query.Where(d =>
        d.Title.Contains(searchTerm) ||
        d.Content.Contains(searchTerm));
}
```

**Pros**:
- ✅ Zero additional dependencies
- ✅ Simple implementation (1 day)
- ✅ Works for small-medium datasets (<10K docs)
- ✅ Case-insensitive with EF.Functions.Like
- ✅ Easy to test

**Cons**:
- ❌ Slow with large content fields
- ❌ No ranking/relevance scoring
- ❌ No fuzzy matching
- ❌ Full table scan (but SQLite is fast enough for MVP)

**Complexity**: Very Low
**Performance**: Acceptable for <10K documents
**Cost**: $0

---

#### Approach 2: SQLite FTS5 (Full-Text Search)

**Overview**: Use SQLite's built-in FTS5 extension

**Pros**:
- ✅ Fast full-text search
- ✅ Ranking and snippets
- ✅ Still using SQLite (no new infrastructure)
- ✅ Proven extension

**Cons**:
- ❌ Requires FTS5 virtual table setup
- ❌ Data synchronization complexity
- ❌ EF Core doesn't natively support FTS5
- ❌ Additional testing complexity

**Complexity**: Medium
**Recommended**: Phase 2 enhancement if basic search insufficient

---

#### Approach 3: Elasticsearch / Azure Cognitive Search

**Overview**: Dedicated search engine

**Pros**:
- ✅ Extremely powerful search
- ✅ Advanced features (facets, suggestions)
- ✅ Scales to millions of documents

**Cons**:
- ❌ Significant infrastructure ($50-100/month minimum)
- ❌ High complexity (separate service, data sync)
- ❌ Massive overkill for current needs
- ❌ Violates simplicity principles

**Complexity**: High
**Not Recommended**: Premature optimization

---

**Decision**: **EF Core LINQ (Approach 1)** for MVP

**Rationale**:
1. Meets Phase 2 requirements with minimal complexity
2. Fast enough for target scale (10K documents)
3. Easy to implement and test
4. Can upgrade to FTS5 later without API changes
5. Zero additional cost or infrastructure

**Migration Path**:
- If search becomes slow (>500ms), migrate to FTS5
- If needs advance beyond FTS5, consider Elasticsearch
- API contract remains unchanged

---

### Key Architectural Decision #3: Document Authorization

#### Approach 1: Author + Published Model ✅ RECOMMENDED

**Overview**: Documents have an author (creator) and IsPublished flag

**Rules**:
- Author can always view/edit/delete their documents
- Other authenticated users can view published documents only
- Admins can view/edit all documents

**Implementation**:
```csharp
public async Task<DocumentDto> GetDocumentAsync(int id, int currentUserId, bool isAdmin)
{
    var doc = await _context.Documents
        .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

    if (doc == null) throw new NotFoundException();

    // Authorization
    if (doc.AuthorId != currentUserId && !doc.IsPublished && !isAdmin)
    {
        throw new ForbiddenException("Access denied");
    }

    return MapToDto(doc);
}
```

**Pros**:
- ✅ Simple mental model
- ✅ Easy to implement
- ✅ Covers 90% of use cases
- ✅ Aligns with existing User/Role system

**Cons**:
- ❌ No granular permissions (read vs write)
- ❌ No sharing with specific users
- ❌ No team/group support

**Complexity**: Low
**Best For**: MVP and small teams

---

#### Approach 2: Full ACL (Access Control Lists)

**Overview**: Separate table for document permissions

**Schema**:
```csharp
public class DocumentPermission
{
    public int DocumentId { get; set; }
    public int UserId { get; set; }
    public PermissionType Permission { get; set; } // Read, Write, Admin
}
```

**Pros**:
- ✅ Granular control
- ✅ Per-user permissions
- ✅ Flexible and powerful

**Cons**:
- ❌ Significant complexity
- ❌ Performance overhead (joins)
- ❌ Harder to reason about
- ❌ Overkill for current needs

**Complexity**: High
**Not Recommended**: Premature for MVP

---

**Decision**: **Author + Published Model (Approach 1)**

**Rationale**:
1. Matches Confluence's draft/published workflow
2. Simple implementation aligns with CLAUDE.md
3. Easy to understand and test
4. Can add ACL later if needed (non-breaking)

---

### Key Architectural Decision #4: Markdown Handling

#### Approach 1: Client-Side Rendering ✅ RECOMMENDED

**Overview**: Store markdown as-is, render in browser with library like marked.js or react-markdown

**Flow**:
```
Database: "# Hello\n\nThis is **bold**"
    ↓
API: Return raw markdown in DTO
    ↓
Client: Render with markdown library
    ↓
Display: HTML with proper formatting
```

**Pros**:
- ✅ Server is stateless (just storage)
- ✅ Client can preview in real-time while editing
- ✅ Flexible rendering (different styles per page)
- ✅ No server-side rendering overhead
- ✅ Enables rich markdown editors

**Cons**:
- ❌ Client must include markdown library
- ❌ Slight rendering delay on client
- ❌ XSS risk if markdown not sanitized (but libraries handle this)

**Complexity**: Low
**Best For**: Rich editing experience

---

#### Approach 2: Server-Side Rendering

**Overview**: Convert markdown to HTML on server before sending to client

**Pros**:
- ✅ Client receives ready-to-display HTML
- ✅ Consistent rendering
- ✅ Server controls sanitization

**Cons**:
- ❌ Server CPU overhead
- ❌ Harder to provide live preview
- ❌ Less flexible client-side
- ❌ Requires markdown library in .NET (Markdig)

**Complexity**: Medium
**Not Recommended**: Limits client flexibility

---

**Decision**: **Client-Side Rendering (Approach 1)**

**Rationale**:
1. Modern SPA pattern (MVC will render via JavaScript)
2. Enables real-time preview in editor
3. Server stays simple and fast
4. Industry standard approach (GitHub, GitLab, etc.)

---

## 3. Recommended Architecture

### System Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                        Client (Browser)                      │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  Document Editor (Razor View)                        │   │
│  │  - Markdown textarea with preview                    │   │
│  │  - File upload for screenshots                       │   │
│  │  - Tag selection                                     │   │
│  │  - Parent document picker                            │   │
│  └─────────────────────────────────────────────────────┘   │
└───────────────────────┬─────────────────────────────────────┘
                        │ HTTP/JSON
┌───────────────────────▼─────────────────────────────────────┐
│                  ASP.NET Core Web API                        │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  DocumentsController                                  │  │
│  │  - GET /api/Documents (list with search/filter)      │  │
│  │  - GET /api/Documents/{id}                           │  │
│  │  - POST /api/Documents                               │  │
│  │  - PUT /api/Documents/{id}                           │  │
│  │  - DELETE /api/Documents/{id}                        │  │
│  │  - GET /api/Documents/{id}/versions                  │  │
│  │  - POST /api/Documents/{id}/attachments              │  │
│  │  - GET /api/Documents/attachments/{id}               │  │
│  └────────────────────┬─────────────────────────────────┘  │
│                       │                                      │
│  ┌────────────────────▼─────────────────────────────────┐  │
│  │  DocumentService (IDocumentService)                   │  │
│  │  - Business logic and validation                      │  │
│  │  - Authorization checks                               │  │
│  │  - Version management                                 │  │
│  └────────────────────┬─────────────────────────────────┘  │
│                       │                                      │
│  ┌────────────────────▼─────────────────────────────────┐  │
│  │  FileStorageService (IFileStorageService)             │  │
│  │  - Save/retrieve/delete files                         │  │
│  │  - Path management                                    │  │
│  └────────────────────┬─────────────────────────────────┘  │
│                       │                                      │
│  ┌────────────────────▼─────────────────────────────────┐  │
│  │  ApplicationDbContext (Entity Framework Core)         │  │
│  │  - Documents, DocumentVersions                        │  │
│  │  - DocumentTags, DocumentTagMaps                      │  │
│  └─────────────────────────────────────────────────────┬┘  │
└────────────────────────────────────────────────────────┼───┘
                                                          │
┌─────────────────────────────────────────────────────────▼───┐
│                    Data & File Storage                       │
│  ┌──────────────────┐         ┌─────────────────────────┐  │
│  │  SQLite Database │         │  Filesystem             │  │
│  │  - Metadata      │         │  screenshots/           │  │
│  │  - Relationships │         │    {docId}/             │  │
│  │  - Versions      │         │      image1.png         │  │
│  └──────────────────┘         └─────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

### Component Responsibilities

**DocumentsController** (API Layer)
- Route HTTP requests to service methods
- Validate JWT authentication
- Extract current user from claims
- Handle exceptions and return appropriate HTTP status codes
- Log requests and errors

**IDocumentService / DocumentService** (Business Logic)
- CRUD operations with business validation
- Authorization enforcement (author/published/admin rules)
- Automatic versioning on document update
- Search and filtering logic
- Tag management
- Coordinate with FileStorageService for attachments

**IFileStorageService / FileStorageService** (Infrastructure)
- Save uploaded files to filesystem
- Generate unique filenames (prevent overwrites)
- Create directory structure (screenshots/{documentId}/)
- Retrieve file streams for download
- Delete files when document deleted
- Validate file types and sizes

**ApplicationDbContext** (Data Access)
- Query documents with EF Core LINQ
- Include related entities (Author, Tags, Versions)
- Track changes and save to database
- Handle concurrency and transactions

### Data Flow Examples

**Create Document**:
1. User fills form, clicks "Create"
2. Client POST /api/Documents with CreateDocumentRequest
3. Controller validates request, extracts userId from JWT
4. DocumentService.CreateAsync():
   - Validates title, content not empty
   - Sets AuthorId = currentUserId
   - Sets CreatedAt, UpdatedAt = DateTime.UtcNow
   - Sets CurrentVersion = 1
   - Creates initial DocumentVersion record
   - Saves to database
5. Returns DocumentDto to client
6. Client redirects to document view page

**Update Document (with Versioning)**:
1. User edits document, clicks "Save"
2. Client PUT /api/Documents/{id} with UpdateDocumentRequest
3. Controller validates request, extracts userId
4. DocumentService.UpdateAsync():
   - Loads existing document
   - Checks authorization (author or admin)
   - Creates new DocumentVersion:
     - VersionNumber = CurrentVersion + 1
     - Copies Title, Content
     - Sets ModifiedById = currentUserId
     - Sets ChangeDescription from request
   - Updates Document:
     - Title, Content = new values
     - CurrentVersion++
     - UpdatedAt = DateTime.UtcNow
   - Saves transaction
5. Returns updated DocumentDto
6. Client shows success message

**Upload Screenshot**:
1. User selects file, clicks "Upload"
2. Client POST /api/Documents/{id}/attachments with multipart/form-data
3. Controller validates file (type, size)
4. DocumentService.AddAttachmentAsync():
   - Checks document exists and user is author
   - Calls FileStorageService.SaveFileAsync():
     - Creates directory screenshots/{documentId}/
     - Generates filename: {timestamp}_{originalName}
     - Writes file stream to disk
     - Returns file metadata
   - Creates DocumentAttachment record (future model)
   - Saves to database
5. Returns attachment URL: /api/Documents/attachments/{attachmentId}
6. Client inserts markdown: ![Image](url)

---

## 4. Technology Stack & Patterns

| Layer | Technology | Version | Purpose |
|-------|------------|---------|---------|
| **Backend API** | ASP.NET Core | 9.0 | REST API endpoints |
| **Business Logic** | C# Services | .NET 9.0 | Document operations |
| **Data Access** | Entity Framework Core | 9.0 | Database queries |
| **Database** | SQLite | 3.x | Persistent storage |
| **Authentication** | JWT | 8.14.0 | User authentication |
| **File I/O** | System.IO | .NET 9.0 | Screenshot storage |
| **Validation** | DataAnnotations | .NET 9.0 | Input validation |
| **Testing** | xUnit | 2.9.2 | Unit tests |
| **Mocking** | In-Memory EF | 9.0.10 | Test isolation |

### Design Patterns

- **Service Layer Pattern**: Business logic encapsulated in services (IDocumentService)
- **DTO Pattern**: Data transfer objects for API contracts
- **Dependency Injection**: Interface-based DI for testability
- **Soft Delete**: IsDeleted flag instead of hard deletes
- **Audit Trailing**: CreatedAt, UpdatedAt timestamps
- **Versioning**: Automatic history tracking via DocumentVersion
- **Authorization**: Claim-based with UserId from JWT

---

## 5. Implementation Phases

### Phase 2.1: Core Documentation Features (Week 1-2)

**Goal**: Implement basic document CRUD with versioning

#### Task 1: Create DTOs (4 hours)

**Files to Create**:
- `source/DocsAndPlannings.Core/DTOs/Documents/CreateDocumentRequest.cs`
- `source/DocsAndPlannings.Core/DTOs/Documents/UpdateDocumentRequest.cs`
- `source/DocsAndPlannings.Core/DTOs/Documents/DocumentDto.cs`
- `source/DocsAndPlannings.Core/DTOs/Documents/DocumentVersionDto.cs`
- `source/DocsAndPlannings.Core/DTOs/Documents/DocumentListItemDto.cs`
- `source/DocsAndPlannings.Core/DTOs/Documents/DocumentSearchRequest.cs`

**CreateDocumentRequest.cs**:
```csharp
using System.ComponentModel.DataAnnotations;

namespace DocsAndPlannings.Core.DTOs.Documents;

public class CreateDocumentRequest
{
    [Required(ErrorMessage = "Title is required")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public required string Title { get; set; }

    [Required(ErrorMessage = "Content is required")]
    public required string Content { get; set; }

    public int? ParentDocumentId { get; set; }

    public bool IsPublished { get; set; } = false;

    public List<int> TagIds { get; set; } = new List<int>();
}
```

**UpdateDocumentRequest.cs**:
```csharp
using System.ComponentModel.DataAnnotations;

namespace DocsAndPlannings.Core.DTOs.Documents;

public class UpdateDocumentRequest
{
    [Required(ErrorMessage = "Title is required")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public required string Title { get; set; }

    [Required(ErrorMessage = "Content is required")]
    public required string Content { get; set; }

    public int? ParentDocumentId { get; set; }

    public bool IsPublished { get; set; }

    public List<int> TagIds { get; set; } = new List<int>();

    [MaxLength(500, ErrorMessage = "Change description cannot exceed 500 characters")]
    public string? ChangeDescription { get; set; }
}
```

**DocumentDto.cs**:
```csharp
namespace DocsAndPlannings.Core.DTOs.Documents;

public class DocumentDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int? ParentDocumentId { get; set; }
    public string? ParentDocumentTitle { get; set; }
    public int AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public int CurrentVersion { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsPublished { get; set; }
    public List<TagDto> Tags { get; set; } = new List<TagDto>();
    public List<DocumentDto> ChildDocuments { get; set; } = new List<DocumentDto>();
}

public class TagDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
}
```

**DocumentVersionDto.cs**:
```csharp
namespace DocsAndPlannings.Core.DTOs.Documents;

public class DocumentVersionDto
{
    public int Id { get; set; }
    public int DocumentId { get; set; }
    public int VersionNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int ModifiedById { get; set; }
    public string ModifiedByName { get; set; } = string.Empty;
    public string? ChangeDescription { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

**DocumentListItemDto.cs** (optimized for list views):
```csharp
namespace DocsAndPlannings.Core.DTOs.Documents;

public class DocumentListItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public int CurrentVersion { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsPublished { get; set; }
    public List<TagDto> Tags { get; set; } = new List<TagDto>();
}
```

**DocumentSearchRequest.cs**:
```csharp
namespace DocsAndPlannings.Core.DTOs.Documents;

public class DocumentSearchRequest
{
    public string? SearchTerm { get; set; }
    public int? AuthorId { get; set; }
    public List<int>? TagIds { get; set; }
    public bool? IsPublished { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
```

**Deliverable**: All DTOs created with proper validation attributes

---

#### Task 2: Create Document Service Interface (2 hours)

**File**: `source/DocsAndPlannings.Core/Services/IDocumentService.cs`

```csharp
using DocsAndPlannings.Core.DTOs.Documents;

namespace DocsAndPlannings.Core.Services;

/// <summary>
/// Service for managing documents and their versions
/// </summary>
public interface IDocumentService
{
    /// <summary>
    /// Creates a new document
    /// </summary>
    /// <param name="request">Document creation data</param>
    /// <param name="currentUserId">ID of the user creating the document</param>
    /// <returns>Created document DTO</returns>
    Task<DocumentDto> CreateAsync(CreateDocumentRequest request, int currentUserId);

    /// <summary>
    /// Updates an existing document and creates a new version
    /// </summary>
    /// <param name="id">Document ID</param>
    /// <param name="request">Update data</param>
    /// <param name="currentUserId">ID of the user updating the document</param>
    /// <param name="isAdmin">Whether the current user is an admin</param>
    /// <returns>Updated document DTO</returns>
    /// <exception cref="NotFoundException">Thrown when document not found</exception>
    /// <exception cref="ForbiddenException">Thrown when user lacks permission</exception>
    Task<DocumentDto> UpdateAsync(int id, UpdateDocumentRequest request, int currentUserId, bool isAdmin);

    /// <summary>
    /// Soft deletes a document
    /// </summary>
    /// <param name="id">Document ID</param>
    /// <param name="currentUserId">ID of the user deleting the document</param>
    /// <param name="isAdmin">Whether the current user is an admin</param>
    /// <exception cref="NotFoundException">Thrown when document not found</exception>
    /// <exception cref="ForbiddenException">Thrown when user lacks permission</exception>
    Task DeleteAsync(int id, int currentUserId, bool isAdmin);

    /// <summary>
    /// Retrieves a document by ID
    /// </summary>
    /// <param name="id">Document ID</param>
    /// <param name="currentUserId">ID of the current user</param>
    /// <param name="isAdmin">Whether the current user is an admin</param>
    /// <returns>Document DTO</returns>
    /// <exception cref="NotFoundException">Thrown when document not found</exception>
    /// <exception cref="ForbiddenException">Thrown when user lacks permission to view</exception>
    Task<DocumentDto> GetByIdAsync(int id, int currentUserId, bool isAdmin);

    /// <summary>
    /// Searches and filters documents
    /// </summary>
    /// <param name="request">Search criteria</param>
    /// <param name="currentUserId">ID of the current user</param>
    /// <param name="isAdmin">Whether the current user is an admin</param>
    /// <returns>List of matching documents</returns>
    Task<(List<DocumentListItemDto> Documents, int TotalCount)> SearchAsync(
        DocumentSearchRequest request,
        int currentUserId,
        bool isAdmin);

    /// <summary>
    /// Gets all versions of a document
    /// </summary>
    /// <param name="documentId">Document ID</param>
    /// <param name="currentUserId">ID of the current user</param>
    /// <param name="isAdmin">Whether the current user is an admin</param>
    /// <returns>List of document versions</returns>
    /// <exception cref="NotFoundException">Thrown when document not found</exception>
    /// <exception cref="ForbiddenException">Thrown when user lacks permission</exception>
    Task<List<DocumentVersionDto>> GetVersionsAsync(int documentId, int currentUserId, bool isAdmin);

    /// <summary>
    /// Gets a specific version of a document
    /// </summary>
    /// <param name="documentId">Document ID</param>
    /// <param name="versionNumber">Version number to retrieve</param>
    /// <param name="currentUserId">ID of the current user</param>
    /// <param name="isAdmin">Whether the current user is an admin</param>
    /// <returns>Document version DTO</returns>
    /// <exception cref="NotFoundException">Thrown when document or version not found</exception>
    /// <exception cref="ForbiddenException">Thrown when user lacks permission</exception>
    Task<DocumentVersionDto> GetVersionAsync(int documentId, int versionNumber, int currentUserId, bool isAdmin);
}
```

**Deliverable**: Interface defined with XML documentation

---

#### Task 3: Implement DocumentService (12 hours)

**File**: `source/DocsAndPlannings.Core/Services/DocumentService.cs`

This is a large implementation. Key methods:

```csharp
using DocsAndPlannings.Core.Data;
using DocsAndPlannings.Core.DTOs.Documents;
using DocsAndPlannings.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace DocsAndPlannings.Core.Services;

public class DocumentService : IDocumentService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(ApplicationDbContext context, ILogger<DocumentService> logger)
    {
        Debug.Assert(context != null, "ApplicationDbContext cannot be null");
        Debug.Assert(logger != null, "Logger cannot be null");

        _context = context;
        _logger = logger;
    }

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
        }

        // Create document
        Document document = new Document
        {
            Title = request.Title,
            Content = request.Content,
            ParentDocumentId = request.ParentDocumentId,
            AuthorId = currentUserId,
            CurrentVersion = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsPublished = request.IsPublished,
            IsDeleted = false
        };

        _context.Documents.Add(document);

        // Create initial version
        DocumentVersion version = new DocumentVersion
        {
            Document = document,
            VersionNumber = 1,
            Title = request.Title,
            Content = request.Content,
            ModifiedById = currentUserId,
            ChangeDescription = "Initial version",
            CreatedAt = DateTime.UtcNow
        };

        _context.DocumentVersions.Add(version);

        // Add tags
        if (request.TagIds.Count > 0)
        {
            await AddTagsToDocumentAsync(document, request.TagIds);
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Document created: {DocumentId} by user {UserId}", document.Id, currentUserId);

        return await GetByIdAsync(document.Id, currentUserId, false);
    }

    public async Task<DocumentDto> UpdateAsync(int id, UpdateDocumentRequest request, int currentUserId, bool isAdmin)
    {
        Debug.Assert(request != null, "UpdateDocumentRequest cannot be null");
        Debug.Assert(currentUserId > 0, "Current user ID must be positive");

        // Load document
        Document? document = await _context.Documents
            .Include(d => d.Author)
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

        if (document == null)
        {
            throw new NotFoundException($"Document with ID {id} not found");
        }

        // Check authorization
        if (document.AuthorId != currentUserId && !isAdmin)
        {
            throw new ForbiddenException("You do not have permission to edit this document");
        }

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
        }

        // Create new version
        DocumentVersion newVersion = new DocumentVersion
        {
            DocumentId = document.Id,
            VersionNumber = document.CurrentVersion + 1,
            Title = request.Title,
            Content = request.Content,
            ModifiedById = currentUserId,
            ChangeDescription = request.ChangeDescription ?? "Updated document",
            CreatedAt = DateTime.UtcNow
        };

        _context.DocumentVersions.Add(newVersion);

        // Update document
        document.Title = request.Title;
        document.Content = request.Content;
        document.ParentDocumentId = request.ParentDocumentId;
        document.IsPublished = request.IsPublished;
        document.CurrentVersion++;
        document.UpdatedAt = DateTime.UtcNow;

        // Update tags
        await UpdateDocumentTagsAsync(document, request.TagIds);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Document updated: {DocumentId} to version {Version} by user {UserId}",
            id, document.CurrentVersion, currentUserId);

        return await GetByIdAsync(id, currentUserId, isAdmin);
    }

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

        // Soft delete
        document.IsDeleted = true;
        document.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Document soft-deleted: {DocumentId} by user {UserId}", id, currentUserId);
    }

    public async Task<DocumentDto> GetByIdAsync(int id, int currentUserId, bool isAdmin)
    {
        Debug.Assert(currentUserId > 0, "Current user ID must be positive");

        Document? document = await _context.Documents
            .Include(d => d.Author)
            .Include(d => d.ParentDocument)
            .Include(d => d.Tags).ThenInclude(t => t.Tag)
            .Include(d => d.ChildDocuments.Where(c => !c.IsDeleted))
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

        if (document == null)
        {
            throw new NotFoundException($"Document with ID {id} not found");
        }

        // Check authorization
        if (document.AuthorId != currentUserId && !document.IsPublished && !isAdmin)
        {
            throw new ForbiddenException("You do not have permission to view this document");
        }

        return MapToDto(document);
    }

    public async Task<(List<DocumentListItemDto> Documents, int TotalCount)> SearchAsync(
        DocumentSearchRequest request,
        int currentUserId,
        bool isAdmin)
    {
        Debug.Assert(request != null, "DocumentSearchRequest cannot be null");
        Debug.Assert(currentUserId > 0, "Current user ID must be positive");

        IQueryable<Document> query = _context.Documents
            .Include(d => d.Author)
            .Include(d => d.Tags).ThenInclude(t => t.Tag)
            .Where(d => !d.IsDeleted);

        // Authorization filter
        if (!isAdmin)
        {
            query = query.Where(d => d.AuthorId == currentUserId || d.IsPublished);
        }

        // Search term filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            string searchLower = request.SearchTerm.ToLower();
            query = query.Where(d =>
                d.Title.ToLower().Contains(searchLower) ||
                d.Content.ToLower().Contains(searchLower));
        }

        // Author filter
        if (request.AuthorId.HasValue)
        {
            query = query.Where(d => d.AuthorId == request.AuthorId.Value);
        }

        // Tag filter
        if (request.TagIds != null && request.TagIds.Count > 0)
        {
            query = query.Where(d => d.Tags.Any(t => request.TagIds.Contains(t.TagId)));
        }

        // Published filter
        if (request.IsPublished.HasValue)
        {
            query = query.Where(d => d.IsPublished == request.IsPublished.Value);
        }

        // Count total results
        int totalCount = await query.CountAsync();

        // Order by update date descending
        query = query.OrderByDescending(d => d.UpdatedAt);

        // Pagination
        List<Document> documents = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        List<DocumentListItemDto> dtos = documents.Select(MapToListItemDto).ToList();

        return (dtos, totalCount);
    }

    public async Task<List<DocumentVersionDto>> GetVersionsAsync(int documentId, int currentUserId, bool isAdmin)
    {
        Debug.Assert(currentUserId > 0, "Current user ID must be positive");

        Document? document = await _context.Documents
            .FirstOrDefaultAsync(d => d.Id == documentId && !d.IsDeleted);

        if (document == null)
        {
            throw new NotFoundException($"Document with ID {documentId} not found");
        }

        // Check authorization
        if (document.AuthorId != currentUserId && !document.IsPublished && !isAdmin)
        {
            throw new ForbiddenException("You do not have permission to view this document's versions");
        }

        List<DocumentVersion> versions = await _context.DocumentVersions
            .Include(v => v.ModifiedBy)
            .Where(v => v.DocumentId == documentId)
            .OrderByDescending(v => v.VersionNumber)
            .ToListAsync();

        return versions.Select(MapToVersionDto).ToList();
    }

    public async Task<DocumentVersionDto> GetVersionAsync(int documentId, int versionNumber, int currentUserId, bool isAdmin)
    {
        Debug.Assert(currentUserId > 0, "Current user ID must be positive");
        Debug.Assert(versionNumber > 0, "Version number must be positive");

        Document? document = await _context.Documents
            .FirstOrDefaultAsync(d => d.Id == documentId && !d.IsDeleted);

        if (document == null)
        {
            throw new NotFoundException($"Document with ID {documentId} not found");
        }

        // Check authorization
        if (document.AuthorId != currentUserId && !document.IsPublished && !isAdmin)
        {
            throw new ForbiddenException("You do not have permission to view this document's versions");
        }

        DocumentVersion? version = await _context.DocumentVersions
            .Include(v => v.ModifiedBy)
            .FirstOrDefaultAsync(v => v.DocumentId == documentId && v.VersionNumber == versionNumber);

        if (version == null)
        {
            throw new NotFoundException($"Version {versionNumber} not found for document {documentId}");
        }

        return MapToVersionDto(version);
    }

    // Helper methods

    private async Task AddTagsToDocumentAsync(Document document, List<int> tagIds)
    {
        foreach (int tagId in tagIds)
        {
            bool tagExists = await _context.DocumentTags.AnyAsync(t => t.Id == tagId && t.IsActive);
            if (!tagExists)
            {
                throw new NotFoundException($"Tag with ID {tagId} not found");
            }

            DocumentTagMap tagMap = new DocumentTagMap
            {
                Document = document,
                TagId = tagId,
                AssignedAt = DateTime.UtcNow
            };

            _context.Set<DocumentTagMap>().Add(tagMap);
        }
    }

    private async Task UpdateDocumentTagsAsync(Document document, List<int> tagIds)
    {
        // Load existing tags
        List<DocumentTagMap> existingTags = await _context.Set<DocumentTagMap>()
            .Where(t => t.DocumentId == document.Id)
            .ToListAsync();

        // Remove tags not in new list
        List<DocumentTagMap> toRemove = existingTags
            .Where(t => !tagIds.Contains(t.TagId))
            .ToList();

        _context.Set<DocumentTagMap>().RemoveRange(toRemove);

        // Add new tags
        List<int> existingTagIds = existingTags.Select(t => t.TagId).ToList();
        List<int> newTagIds = tagIds.Except(existingTagIds).ToList();

        foreach (int tagId in newTagIds)
        {
            bool tagExists = await _context.DocumentTags.AnyAsync(t => t.Id == tagId && t.IsActive);
            if (!tagExists)
            {
                throw new NotFoundException($"Tag with ID {tagId} not found");
            }

            DocumentTagMap tagMap = new DocumentTagMap
            {
                DocumentId = document.Id,
                TagId = tagId,
                AssignedAt = DateTime.UtcNow
            };

            _context.Set<DocumentTagMap>().Add(tagMap);
        }
    }

    private static DocumentDto MapToDto(Document document)
    {
        return new DocumentDto
        {
            Id = document.Id,
            Title = document.Title,
            Content = document.Content,
            ParentDocumentId = document.ParentDocumentId,
            ParentDocumentTitle = document.ParentDocument?.Title,
            AuthorId = document.AuthorId,
            AuthorName = $"{document.Author.FirstName} {document.Author.LastName}",
            CurrentVersion = document.CurrentVersion,
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt,
            IsPublished = document.IsPublished,
            Tags = document.Tags.Select(t => new TagDto
            {
                Id = t.Tag.Id,
                Name = t.Tag.Name,
                Color = t.Tag.Color
            }).ToList(),
            ChildDocuments = document.ChildDocuments.Select(c => new DocumentDto
            {
                Id = c.Id,
                Title = c.Title,
                Content = string.Empty, // Don't load content for children
                AuthorId = c.AuthorId,
                CurrentVersion = c.CurrentVersion,
                UpdatedAt = c.UpdatedAt,
                IsPublished = c.IsPublished
            }).ToList()
        };
    }

    private static DocumentListItemDto MapToListItemDto(Document document)
    {
        return new DocumentListItemDto
        {
            Id = document.Id,
            Title = document.Title,
            AuthorName = $"{document.Author.FirstName} {document.Author.LastName}",
            CurrentVersion = document.CurrentVersion,
            UpdatedAt = document.UpdatedAt,
            IsPublished = document.IsPublished,
            Tags = document.Tags.Select(t => new TagDto
            {
                Id = t.Tag.Id,
                Name = t.Tag.Name,
                Color = t.Tag.Color
            }).ToList()
        };
    }

    private static DocumentVersionDto MapToVersionDto(DocumentVersion version)
    {
        return new DocumentVersionDto
        {
            Id = version.Id,
            DocumentId = version.DocumentId,
            VersionNumber = version.VersionNumber,
            Title = version.Title,
            Content = version.Content,
            ModifiedById = version.ModifiedById,
            ModifiedByName = $"{version.ModifiedBy.FirstName} {version.ModifiedBy.LastName}",
            ChangeDescription = version.ChangeDescription,
            CreatedAt = version.CreatedAt
        };
    }
}

// Exception classes
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message) { }
}
```

**Deliverable**: DocumentService implementation complete with:
- All interface methods implemented
- Authorization checks
- Automatic versioning
- Tag management
- Debug assertions
- Logging

---

#### Task 4: Create DocumentsController (8 hours)

**File**: `source/DocsAndPlannings.Api/Controllers/DocumentsController.cs`

```csharp
using DocsAndPlannings.Core.DTOs.Documents;
using DocsAndPlannings.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DocsAndPlannings.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly ILogger<DocumentsController> _logger;

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
    /// <param name="request">Document creation data</param>
    /// <returns>Created document</returns>
    [HttpPost]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateDocumentRequest request)
    {
        try
        {
            int currentUserId = GetCurrentUserId();
            DocumentDto document = await _documentService.CreateAsync(request, currentUserId);

            _logger.LogInformation("Document created: {DocumentId} by user {UserId}", document.Id, currentUserId);

            return CreatedAtAction(nameof(GetById), new { id = document.Id }, document);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Create document failed: {Message}", ex.Message);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating document");
            return StatusCode(500, new { error = "An error occurred while creating the document" });
        }
    }

    /// <summary>
    /// Updates an existing document
    /// </summary>
    /// <param name="id">Document ID</param>
    /// <param name="request">Update data</param>
    /// <returns>Updated document</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDocumentRequest request)
    {
        try
        {
            int currentUserId = GetCurrentUserId();
            bool isAdmin = User.IsInRole("Admin");

            DocumentDto document = await _documentService.UpdateAsync(id, request, currentUserId, isAdmin);

            _logger.LogInformation("Document updated: {DocumentId} by user {UserId}", id, currentUserId);

            return Ok(document);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Update document failed: {Message}", ex.Message);
            return NotFound(new { error = ex.Message });
        }
        catch (ForbiddenException ex)
        {
            _logger.LogWarning(ex, "Update document forbidden: {Message}", ex.Message);
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Update document validation failed: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating document {DocumentId}", id);
            return StatusCode(500, new { error = "An error occurred while updating the document" });
        }
    }

    /// <summary>
    /// Deletes a document (soft delete)
    /// </summary>
    /// <param name="id">Document ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            int currentUserId = GetCurrentUserId();
            bool isAdmin = User.IsInRole("Admin");

            await _documentService.DeleteAsync(id, currentUserId, isAdmin);

            _logger.LogInformation("Document deleted: {DocumentId} by user {UserId}", id, currentUserId);

            return NoContent();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Delete document failed: {Message}", ex.Message);
            return NotFound(new { error = ex.Message });
        }
        catch (ForbiddenException ex)
        {
            _logger.LogWarning(ex, "Delete document forbidden: {Message}", ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document {DocumentId}", id);
            return StatusCode(500, new { error = "An error occurred while deleting the document" });
        }
    }

    /// <summary>
    /// Gets a document by ID
    /// </summary>
    /// <param name="id">Document ID</param>
    /// <returns>Document details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            int currentUserId = GetCurrentUserId();
            bool isAdmin = User.IsInRole("Admin");

            DocumentDto document = await _documentService.GetByIdAsync(id, currentUserId, isAdmin);

            return Ok(document);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Get document failed: {Message}", ex.Message);
            return NotFound(new { error = ex.Message });
        }
        catch (ForbiddenException ex)
        {
            _logger.LogWarning(ex, "Get document forbidden: {Message}", ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving document {DocumentId}", id);
            return StatusCode(500, new { error = "An error occurred while retrieving the document" });
        }
    }

    /// <summary>
    /// Searches and filters documents
    /// </summary>
    /// <param name="request">Search criteria</param>
    /// <returns>List of matching documents with pagination</returns>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Search([FromQuery] DocumentSearchRequest request)
    {
        try
        {
            int currentUserId = GetCurrentUserId();
            bool isAdmin = User.IsInRole("Admin");

            (List<DocumentListItemDto> documents, int totalCount) = await _documentService.SearchAsync(
                request,
                currentUserId,
                isAdmin);

            return Ok(new
            {
                documents,
                totalCount,
                pageNumber = request.PageNumber,
                pageSize = request.PageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching documents");
            return StatusCode(500, new { error = "An error occurred while searching documents" });
        }
    }

    /// <summary>
    /// Gets all versions of a document
    /// </summary>
    /// <param name="id">Document ID</param>
    /// <returns>List of document versions</returns>
    [HttpGet("{id}/versions")]
    [ProducesResponseType(typeof(List<DocumentVersionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVersions(int id)
    {
        try
        {
            int currentUserId = GetCurrentUserId();
            bool isAdmin = User.IsInRole("Admin");

            List<DocumentVersionDto> versions = await _documentService.GetVersionsAsync(id, currentUserId, isAdmin);

            return Ok(versions);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Get versions failed: {Message}", ex.Message);
            return NotFound(new { error = ex.Message });
        }
        catch (ForbiddenException ex)
        {
            _logger.LogWarning(ex, "Get versions forbidden: {Message}", ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving versions for document {DocumentId}", id);
            return StatusCode(500, new { error = "An error occurred while retrieving document versions" });
        }
    }

    /// <summary>
    /// Gets a specific version of a document
    /// </summary>
    /// <param name="id">Document ID</param>
    /// <param name="versionNumber">Version number</param>
    /// <returns>Document version details</returns>
    [HttpGet("{id}/versions/{versionNumber}")]
    [ProducesResponseType(typeof(DocumentVersionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVersion(int id, int versionNumber)
    {
        try
        {
            int currentUserId = GetCurrentUserId();
            bool isAdmin = User.IsInRole("Admin");

            DocumentVersionDto version = await _documentService.GetVersionAsync(
                id,
                versionNumber,
                currentUserId,
                isAdmin);

            return Ok(version);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Get version failed: {Message}", ex.Message);
            return NotFound(new { error = ex.Message });
        }
        catch (ForbiddenException ex)
        {
            _logger.LogWarning(ex, "Get version forbidden: {Message}", ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving version {VersionNumber} for document {DocumentId}",
                versionNumber, id);
            return StatusCode(500, new { error = "An error occurred while retrieving the document version" });
        }
    }

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
```

**Deliverable**: DocumentsController with all CRUD endpoints, proper error handling, XML comments

---

#### Task 5: Register Services in DI Container (1 hour)

**File**: `source/DocsAndPlannings.Api/Program.cs`

Add after existing service registrations:

```csharp
// Document services
builder.Services.AddScoped<IDocumentService, DocumentService>();
```

**Deliverable**: DocumentService registered and ready for injection

---

#### Task 6: Create Comprehensive Unit Tests (12 hours)

**File**: `tests/DocsAndPlannings.Core.Tests/Services/DocumentServiceTests.cs`

This will be extensive. Key test categories:

```csharp
public class DocumentServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly DocumentService _service;
    private readonly ILogger<DocumentService> _logger;

    public DocumentServiceTests()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _logger = new LoggerFactory().CreateLogger<DocumentService>();
        _service = new DocumentService(_context, _logger);

        // Seed test data
        SeedTestData();
    }

    private void SeedTestData()
    {
        User author = new User
        {
            Email = "author@test.com",
            PasswordHash = "hash",
            FirstName = "Test",
            LastName = "Author",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        User otherUser = new User
        {
            Email = "other@test.com",
            PasswordHash = "hash",
            FirstName = "Other",
            LastName = "User",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Users.AddRange(author, otherUser);
        _context.SaveChanges();
    }

    [Fact]
    public async Task CreateAsync_CreatesDocumentAndInitialVersion()
    {
        // Arrange
        User author = _context.Users.First();
        CreateDocumentRequest request = new CreateDocumentRequest
        {
            Title = "Test Document",
            Content = "# Hello World\n\nThis is a test.",
            IsPublished = false,
            TagIds = new List<int>()
        };

        // Act
        DocumentDto result = await _service.CreateAsync(request, author.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Document", result.Title);
        Assert.Equal(author.Id, result.AuthorId);
        Assert.Equal(1, result.CurrentVersion);
        Assert.False(result.IsPublished);

        // Verify version created
        DocumentVersion? version = await _context.DocumentVersions
            .FirstOrDefaultAsync(v => v.DocumentId == result.Id && v.VersionNumber == 1);
        Assert.NotNull(version);
        Assert.Equal("Initial version", version.ChangeDescription);
    }

    [Fact]
    public async Task UpdateAsync_CreatesNewVersion()
    {
        // Arrange
        User author = _context.Users.First();
        Document doc = new Document
        {
            Title = "Original",
            Content = "Original content",
            AuthorId = author.Id,
            CurrentVersion = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Documents.Add(doc);
        await _context.SaveChangesAsync();

        UpdateDocumentRequest request = new UpdateDocumentRequest
        {
            Title = "Updated",
            Content = "Updated content",
            IsPublished = false,
            TagIds = new List<int>(),
            ChangeDescription = "Updated for testing"
        };

        // Act
        DocumentDto result = await _service.UpdateAsync(doc.Id, request, author.Id, false);

        // Assert
        Assert.Equal("Updated", result.Title);
        Assert.Equal(2, result.CurrentVersion);

        // Verify new version
        DocumentVersion? version = await _context.DocumentVersions
            .FirstOrDefaultAsync(v => v.DocumentId == doc.Id && v.VersionNumber == 2);
        Assert.NotNull(version);
        Assert.Equal("Updated for testing", version.ChangeDescription);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsForbiddenException_WhenUserNotAuthor()
    {
        // Arrange
        User author = _context.Users.First();
        User otherUser = _context.Users.Skip(1).First();

        Document doc = new Document
        {
            Title = "Original",
            Content = "Original content",
            AuthorId = author.Id,
            CurrentVersion = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Documents.Add(doc);
        await _context.SaveChangesAsync();

        UpdateDocumentRequest request = new UpdateDocumentRequest
        {
            Title = "Hacked",
            Content = "Hacked content",
            IsPublished = false,
            TagIds = new List<int>()
        };

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(
            () => _service.UpdateAsync(doc.Id, request, otherUser.Id, false));
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsDocument_WhenUserIsAuthor()
    {
        // Arrange
        User author = _context.Users.First();
        Document doc = new Document
        {
            Title = "Test",
            Content = "Content",
            AuthorId = author.Id,
            CurrentVersion = 1,
            IsPublished = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Documents.Add(doc);
        await _context.SaveChangesAsync();

        // Act
        DocumentDto result = await _service.GetByIdAsync(doc.Id, author.Id, false);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test", result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_ThrowsForbiddenException_WhenUnpublishedAndUserNotAuthor()
    {
        // Arrange
        User author = _context.Users.First();
        User otherUser = _context.Users.Skip(1).First();

        Document doc = new Document
        {
            Title = "Private",
            Content = "Content",
            AuthorId = author.Id,
            CurrentVersion = 1,
            IsPublished = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Documents.Add(doc);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(
            () => _service.GetByIdAsync(doc.Id, otherUser.Id, false));
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsDocument_WhenPublishedAndUserNotAuthor()
    {
        // Arrange
        User author = _context.Users.First();
        User otherUser = _context.Users.Skip(1).First();

        Document doc = new Document
        {
            Title = "Public",
            Content = "Content",
            AuthorId = author.Id,
            CurrentVersion = 1,
            IsPublished = true, // Published
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Documents.Add(doc);
        await _context.SaveChangesAsync();

        // Act
        DocumentDto result = await _service.GetByIdAsync(doc.Id, otherUser.Id, false);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Public", result.Title);
    }

    [Fact]
    public async Task SearchAsync_FiltersUnpublishedDocuments_WhenUserNotAuthor()
    {
        // Arrange
        User author = _context.Users.First();
        User otherUser = _context.Users.Skip(1).First();

        Document publicDoc = new Document
        {
            Title = "Public",
            Content = "Public content",
            AuthorId = author.Id,
            CurrentVersion = 1,
            IsPublished = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Document privateDoc = new Document
        {
            Title = "Private",
            Content = "Private content",
            AuthorId = author.Id,
            CurrentVersion = 1,
            IsPublished = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Documents.AddRange(publicDoc, privateDoc);
        await _context.SaveChangesAsync();

        DocumentSearchRequest request = new DocumentSearchRequest
        {
            PageNumber = 1,
            PageSize = 20
        };

        // Act
        (List<DocumentListItemDto> documents, int totalCount) = await _service.SearchAsync(
            request,
            otherUser.Id,
            false);

        // Assert
        Assert.Single(documents);
        Assert.Equal("Public", documents[0].Title);
        Assert.Equal(1, totalCount);
    }

    [Fact]
    public async Task SearchAsync_SearchesTitleAndContent()
    {
        // Arrange
        User author = _context.Users.First();

        Document doc1 = new Document
        {
            Title = "C# Programming",
            Content = "Learn about classes",
            AuthorId = author.Id,
            CurrentVersion = 1,
            IsPublished = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Document doc2 = new Document
        {
            Title = "Python Basics",
            Content = "Python is great for C# developers",
            AuthorId = author.Id,
            CurrentVersion = 1,
            IsPublished = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Document doc3 = new Document
        {
            Title = "JavaScript",
            Content = "JavaScript fundamentals",
            AuthorId = author.Id,
            CurrentVersion = 1,
            IsPublished = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Documents.AddRange(doc1, doc2, doc3);
        await _context.SaveChangesAsync();

        DocumentSearchRequest request = new DocumentSearchRequest
        {
            SearchTerm = "C#",
            PageNumber = 1,
            PageSize = 20
        };

        // Act
        (List<DocumentListItemDto> documents, int totalCount) = await _service.SearchAsync(
            request,
            author.Id,
            false);

        // Assert
        Assert.Equal(2, totalCount);
        Assert.Contains(documents, d => d.Title.Contains("C#"));
        Assert.Contains(documents, d => d.Title.Contains("Python")); // Content match
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesDocument()
    {
        // Arrange
        User author = _context.Users.First();
        Document doc = new Document
        {
            Title = "To Delete",
            Content = "Content",
            AuthorId = author.Id,
            CurrentVersion = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Documents.Add(doc);
        await _context.SaveChangesAsync();

        // Act
        await _service.DeleteAsync(doc.Id, author.Id, false);

        // Assert
        Document? deleted = await _context.Documents.FindAsync(doc.Id);
        Assert.NotNull(deleted);
        Assert.True(deleted.IsDeleted);
    }

    [Fact]
    public async Task GetVersionsAsync_ReturnsAllVersionsOrderedByVersionNumber()
    {
        // Arrange
        User author = _context.Users.First();
        Document doc = new Document
        {
            Title = "Versioned",
            Content = "Content v3",
            AuthorId = author.Id,
            CurrentVersion = 3,
            IsPublished = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        DocumentVersion v1 = new DocumentVersion
        {
            Document = doc,
            VersionNumber = 1,
            Title = "Versioned",
            Content = "Content v1",
            ModifiedById = author.Id,
            ChangeDescription = "Initial",
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        };

        DocumentVersion v2 = new DocumentVersion
        {
            Document = doc,
            VersionNumber = 2,
            Title = "Versioned",
            Content = "Content v2",
            ModifiedById = author.Id,
            ChangeDescription = "Update 1",
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        DocumentVersion v3 = new DocumentVersion
        {
            Document = doc,
            VersionNumber = 3,
            Title = "Versioned",
            Content = "Content v3",
            ModifiedById = author.Id,
            ChangeDescription = "Update 2",
            CreatedAt = DateTime.UtcNow
        };

        _context.Documents.Add(doc);
        _context.DocumentVersions.AddRange(v1, v2, v3);
        await _context.SaveChangesAsync();

        // Act
        List<DocumentVersionDto> versions = await _service.GetVersionsAsync(doc.Id, author.Id, false);

        // Assert
        Assert.Equal(3, versions.Count);
        Assert.Equal(3, versions[0].VersionNumber); // Descending order
        Assert.Equal(2, versions[1].VersionNumber);
        Assert.Equal(1, versions[2].VersionNumber);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
```

Additional test files needed:
- `DocumentServiceAuthorizationTests.cs` - Focus on all auth scenarios
- `DocumentServiceVersioningTests.cs` - Deep versioning tests
- `DocumentServiceTagTests.cs` - Tag assignment and filtering
- `DocumentsControllerTests.cs` - Controller endpoint tests

**Deliverable**: Comprehensive unit tests with >80% coverage

---

### Phase 2.1 Deliverables Summary

After completing all tasks in Phase 2.1, you will have:

- ✅ 7 DTO classes for document operations
- ✅ IDocumentService interface with full XML documentation
- ✅ DocumentService implementation (500+ lines)
- ✅ DocumentsController with 7 REST endpoints
- ✅ DI registration complete
- ✅ 50+ unit tests covering all scenarios
- ✅ All CRUD operations functional
- ✅ Automatic versioning working
- ✅ Authorization enforced
- ✅ Search and filtering operational

**Testing Checklist**:
- [ ] Create document via API
- [ ] Update document (verify version increments)
- [ ] List documents (verify filtering works)
- [ ] Search documents by keyword
- [ ] View version history
- [ ] Test authorization (unpublished doc blocked for non-author)
- [ ] Soft delete document
- [ ] All unit tests passing (dotnet test)
- [ ] Build with 0 warnings (dotnet build -warnaserror)

---

### Phase 2.2: Advanced Documentation Features (Week 3)

**Goal**: Add file uploads, tag management, and enhanced search

[Continuing in next message due to length...]

This implementation guide is getting quite long. Would you like me to:
1. Continue with Phase 2.2 (Screenshot uploads, tag management, advanced search)
2. Save this current guide and proceed to implementation
3. Create a shorter summary version

What would be most helpful?
