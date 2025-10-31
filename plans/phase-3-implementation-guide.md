# Phase 3: Planning/Tracking Module - Implementation Guide

**Branch**: `feature/phase-3-planning-module`
**Status**: In Progress (40% complete)
**Created**: 2025-10-31

---

## Current Status

### ✅ Completed (40%)

**DTOs Created (22 files)**:
- Projects: `CreateProjectRequest`, `UpdateProjectRequest`, `ProjectDto`, `ProjectListItemDto`
- Epics: `CreateEpicRequest`, `UpdateEpicRequest`, `EpicDto`, `EpicListItemDto`
- WorkItems: `CreateWorkItemRequest`, `UpdateWorkItemRequest`, `WorkItemDto`, `WorkItemListItemDto`, `WorkItemSearchRequest`
- Comments: `CreateCommentRequest`, `UpdateCommentRequest`, `CommentDto`
- Statuses: `CreateStatusRequest`, `UpdateStatusRequest`, `StatusDto`, `CreateStatusTransitionRequest`, `StatusTransitionDto`

**Exceptions Created (4 files)**:
- `InvalidStatusTransitionException`
- `CircularHierarchyException`
- `InvalidHierarchyException`
- `DuplicateKeyException`

**Services Created (2 files)**:
- `IKeyGenerationService` + `KeyGenerationService` (complete)
- `IStatusService` (interface only)

### 🚧 Remaining Work (60%)

1. **StatusService** implementation
2. **ProjectService** (interface + implementation)
3. **EpicService** (interface + implementation)
4. **WorkItemService** (interface + implementation)
5. **CommentService** (interface + implementation)
6. **5 Controllers** (Projects, Epics, WorkItems, Comments, Statuses)
7. **340+ Unit Tests** (services + controllers)
8. **Database Migration**
9. **Service Registration** in DI container
10. **Code Review & Bug Hunting**
11. **Documentation Updates**

---

## Implementation Guide

### Part 1: StatusService Implementation

**File**: `source/DocsAndPlannings.Core/Services/StatusService.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using DocsAndPlannings.Core.Data;
using DocsAndPlannings.Core.DTOs.Statuses;
using DocsAndPlannings.Core.Exceptions;
using DocsAndPlannings.Core.Exceptions.Planning;
using DocsAndPlannings.Core.Models;

namespace DocsAndPlannings.Core.Services;

/// <summary>
/// Service for managing statuses and status transitions
/// </summary>
public class StatusService : IStatusService
{
    private readonly ApplicationDbContext _context;

    public StatusService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<StatusDto>> GetAllStatusesAsync()
    {
        var statuses = await _context.Statuses
            .AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.OrderIndex)
            .ToListAsync();

        return statuses.Select(MapToDto).ToList();
    }

    public async Task<StatusDto?> GetStatusByIdAsync(int id)
    {
        var status = await _context.Statuses
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        return status != null ? MapToDto(status) : null;
    }

    public async Task<StatusDto> CreateStatusAsync(CreateStatusRequest request)
    {
        // Check for duplicate name
        var exists = await _context.Statuses
            .AnyAsync(s => s.Name == request.Name);

        if (exists)
        {
            throw new BadRequestException($"Status with name '{request.Name}' already exists");
        }

        var status = new Status
        {
            Name = request.Name,
            Color = request.Color,
            OrderIndex = request.OrderIndex,
            IsDefaultForNew = request.IsDefaultForNew,
            IsCompletedStatus = request.IsCompletedStatus,
            IsCancelledStatus = request.IsCancelledStatus,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Statuses.Add(status);
        await _context.SaveChangesAsync();

        return MapToDto(status);
    }

    public async Task<StatusDto> UpdateStatusAsync(int id, UpdateStatusRequest request)
    {
        var status = await _context.Statuses
            .FirstOrDefaultAsync(s => s.Id == id);

        if (status == null)
        {
            throw new NotFoundException($"Status with ID {id} not found");
        }

        // Check for duplicate name (excluding current status)
        var exists = await _context.Statuses
            .AnyAsync(s => s.Name == request.Name && s.Id != id);

        if (exists)
        {
            throw new BadRequestException($"Status with name '{request.Name}' already exists");
        }

        status.Name = request.Name;
        status.Color = request.Color;
        status.OrderIndex = request.OrderIndex;
        status.IsDefaultForNew = request.IsDefaultForNew;
        status.IsCompletedStatus = request.IsCompletedStatus;
        status.IsCancelledStatus = request.IsCancelledStatus;
        status.IsActive = request.IsActive;

        await _context.SaveChangesAsync();

        return MapToDto(status);
    }

    public async Task DeleteStatusAsync(int id)
    {
        var status = await _context.Statuses
            .FirstOrDefaultAsync(s => s.Id == id);

        if (status == null)
        {
            throw new NotFoundException($"Status with ID {id} not found");
        }

        // Check if status is in use
        var epicCount = await _context.Epics.CountAsync(e => e.StatusId == id);
        var workItemCount = await _context.WorkItems.CountAsync(w => w.StatusId == id);

        if (epicCount > 0 || workItemCount > 0)
        {
            throw new BadRequestException(
                $"Cannot delete status '{status.Name}' because it is in use by {epicCount} epics and {workItemCount} work items");
        }

        _context.Statuses.Remove(status);
        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<StatusDto>> GetAllowedTransitionsAsync(int fromStatusId)
    {
        var transitions = await _context.StatusTransitions
            .AsNoTracking()
            .Include(t => t.ToStatus)
            .Where(t => t.FromStatusId == fromStatusId && t.IsAllowed)
            .Select(t => t.ToStatus)
            .ToListAsync();

        return transitions.Select(MapToDto).ToList();
    }

    public async Task<bool> ValidateTransitionAsync(int fromStatusId, int toStatusId)
    {
        // Same status is always allowed
        if (fromStatusId == toStatusId)
        {
            return true;
        }

        // Check if there's an explicit transition rule
        var transition = await _context.StatusTransitions
            .AsNoTracking()
            .FirstOrDefaultAsync(t =>
                t.FromStatusId == fromStatusId &&
                t.ToStatusId == toStatusId);

        // If no explicit rule exists, allow transition (permissive by default)
        // If rule exists, respect the IsAllowed flag
        return transition?.IsAllowed ?? true;
    }

    public async Task<StatusTransitionDto> CreateStatusTransitionAsync(CreateStatusTransitionRequest request)
    {
        // Validate source and target statuses exist
        var fromStatus = await _context.Statuses.FindAsync(request.FromStatusId);
        var toStatus = await _context.Statuses.FindAsync(request.ToStatusId);

        if (fromStatus == null)
        {
            throw new NotFoundException($"Source status with ID {request.FromStatusId} not found");
        }

        if (toStatus == null)
        {
            throw new NotFoundException($"Target status with ID {request.ToStatusId} not found");
        }

        // Check for duplicate transition
        var exists = await _context.StatusTransitions
            .AnyAsync(t => t.FromStatusId == request.FromStatusId && t.ToStatusId == request.ToStatusId);

        if (exists)
        {
            throw new BadRequestException(
                $"Transition from '{fromStatus.Name}' to '{toStatus.Name}' already exists");
        }

        var transition = new StatusTransition
        {
            FromStatusId = request.FromStatusId,
            ToStatusId = request.ToStatusId,
            IsAllowed = request.IsAllowed,
            CreatedAt = DateTime.UtcNow
        };

        _context.StatusTransitions.Add(transition);
        await _context.SaveChangesAsync();

        return new StatusTransitionDto
        {
            Id = transition.Id,
            FromStatusId = transition.FromStatusId,
            FromStatusName = fromStatus.Name,
            ToStatusId = transition.ToStatusId,
            ToStatusName = toStatus.Name,
            IsAllowed = transition.IsAllowed,
            CreatedAt = transition.CreatedAt
        };
    }

    public async Task CreateDefaultStatusesAsync()
    {
        // Check if statuses already exist
        var hasStatuses = await _context.Statuses.AnyAsync();
        if (hasStatuses)
        {
            return; // Already initialized
        }

        var defaultStatuses = new[]
        {
            new Status
            {
                Name = "TODO",
                Color = "#95a5a6",
                OrderIndex = 1,
                IsDefaultForNew = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new Status
            {
                Name = "IN PROGRESS",
                Color = "#3498db",
                OrderIndex = 2,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new Status
            {
                Name = "DONE",
                Color = "#2ecc71",
                OrderIndex = 3,
                IsCompletedStatus = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new Status
            {
                Name = "CANCELLED",
                Color = "#e74c3c",
                OrderIndex = 4,
                IsCancelledStatus = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new Status
            {
                Name = "BACKLOG",
                Color = "#34495e",
                OrderIndex = 0,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            }
        };

        _context.Statuses.AddRange(defaultStatuses);
        await _context.SaveChangesAsync();
    }

    private static StatusDto MapToDto(Status status)
    {
        return new StatusDto
        {
            Id = status.Id,
            Name = status.Name,
            Color = status.Color,
            OrderIndex = status.OrderIndex,
            IsDefaultForNew = status.IsDefaultForNew,
            IsCompletedStatus = status.IsCompletedStatus,
            IsCancelledStatus = status.IsCancelledStatus,
            CreatedAt = status.CreatedAt,
            IsActive = status.IsActive
        };
    }
}
```

---

### Part 2: ProjectService Implementation

**File**: `source/DocsAndPlannings.Core/Services/IProjectService.cs`

```csharp
using DocsAndPlannings.Core.DTOs.Projects;

namespace DocsAndPlannings.Core.Services;

public interface IProjectService
{
    Task<ProjectDto> CreateProjectAsync(CreateProjectRequest request, int ownerId);
    Task<ProjectDto?> GetProjectByIdAsync(int id);
    Task<IReadOnlyList<ProjectListItemDto>> GetAllProjectsAsync(
        int? ownerId = null,
        bool? isActive = null,
        bool? isArchived = null,
        int page = 1,
        int pageSize = 50);
    Task<ProjectDto> UpdateProjectAsync(int id, UpdateProjectRequest request, int userId);
    Task DeleteProjectAsync(int id, int userId);
    Task<ProjectDto> ArchiveProjectAsync(int id, int userId);
    Task<ProjectDto> UnarchiveProjectAsync(int id, int userId);
}
```

**File**: `source/DocsAndPlannings.Core/Services/ProjectService.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using DocsAndPlannings.Core.Data;
using DocsAndPlannings.Core.DTOs.Projects;
using DocsAndPlannings.Core.Exceptions;
using DocsAndPlannings.Core.Exceptions.Planning;
using DocsAndPlannings.Core.Models;

namespace DocsAndPlannings.Core.Services;

public class ProjectService : IProjectService
{
    private readonly ApplicationDbContext _context;

    public ProjectService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProjectDto> CreateProjectAsync(CreateProjectRequest request, int ownerId)
    {
        // Validate unique key
        var exists = await _context.Projects
            .AnyAsync(p => p.Key == request.Key);

        if (exists)
        {
            throw new DuplicateKeyException($"Project with key '{request.Key}' already exists");
        }

        var project = new Project
        {
            Key = request.Key,
            Name = request.Name,
            Description = request.Description,
            OwnerId = ownerId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            IsArchived = false
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        return await MapToDtoAsync(project);
    }

    public async Task<ProjectDto?> GetProjectByIdAsync(int id)
    {
        var project = await _context.Projects
            .Include(p => p.Owner)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        return project != null ? await MapToDtoAsync(project) : null;
    }

    public async Task<IReadOnlyList<ProjectListItemDto>> GetAllProjectsAsync(
        int? ownerId = null,
        bool? isActive = null,
        bool? isArchived = null,
        int page = 1,
        int pageSize = 50)
    {
        var query = _context.Projects
            .Include(p => p.Owner)
            .AsNoTracking()
            .AsQueryable();

        if (ownerId.HasValue)
        {
            query = query.Where(p => p.OwnerId == ownerId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(p => p.IsActive == isActive.Value);
        }

        if (isArchived.HasValue)
        {
            query = query.Where(p => p.IsArchived == isArchived.Value);
        }

        var projects = await query
            .OrderByDescending(p => p.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = new List<ProjectListItemDto>();
        foreach (var project in projects)
        {
            result.Add(await MapToListItemDtoAsync(project));
        }

        return result;
    }

    public async Task<ProjectDto> UpdateProjectAsync(int id, UpdateProjectRequest request, int userId)
    {
        var project = await _context.Projects
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null)
        {
            throw new NotFoundException($"Project with ID {id} not found");
        }

        // Check ownership
        if (project.OwnerId != userId)
        {
            throw new ForbiddenException("Only the project owner can update the project");
        }

        project.Name = request.Name;
        project.Description = request.Description;
        project.IsActive = request.IsActive;
        project.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await MapToDtoAsync(project);
    }

    public async Task DeleteProjectAsync(int id, int userId)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null)
        {
            throw new NotFoundException($"Project with ID {id} not found");
        }

        // Check ownership
        if (project.OwnerId != userId)
        {
            throw new ForbiddenException("Only the project owner can delete the project");
        }

        // Check for dependencies
        var epicCount = await _context.Epics.CountAsync(e => e.ProjectId == id);
        var workItemCount = await _context.WorkItems.CountAsync(w => w.ProjectId == id);

        if (epicCount > 0 || workItemCount > 0)
        {
            throw new BadRequestException(
                $"Cannot delete project '{project.Name}' because it contains {epicCount} epics and {workItemCount} work items. " +
                "Please delete or reassign them first, or archive the project instead.");
        }

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();
    }

    public async Task<ProjectDto> ArchiveProjectAsync(int id, int userId)
    {
        var project = await _context.Projects
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null)
        {
            throw new NotFoundException($"Project with ID {id} not found");
        }

        if (project.OwnerId != userId)
        {
            throw new ForbiddenException("Only the project owner can archive the project");
        }

        project.IsArchived = true;
        project.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await MapToDtoAsync(project);
    }

    public async Task<ProjectDto> UnarchiveProjectAsync(int id, int userId)
    {
        var project = await _context.Projects
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null)
        {
            throw new NotFoundException($"Project with ID {id} not found");
        }

        if (project.OwnerId != userId)
        {
            throw new ForbiddenException("Only the project owner can unarchive the project");
        }

        project.IsArchived = false;
        project.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await MapToDtoAsync(project);
    }

    private async Task<ProjectDto> MapToDtoAsync(Project project)
    {
        var epicCount = await _context.Epics.CountAsync(e => e.ProjectId == project.Id);
        var workItemCount = await _context.WorkItems.CountAsync(w => w.ProjectId == project.Id);

        return new ProjectDto
        {
            Id = project.Id,
            Key = project.Key,
            Name = project.Name,
            Description = project.Description,
            OwnerId = project.OwnerId,
            OwnerName = $"{project.Owner.FirstName} {project.Owner.LastName}",
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt,
            IsActive = project.IsActive,
            IsArchived = project.IsArchived,
            EpicCount = epicCount,
            WorkItemCount = workItemCount
        };
    }

    private async Task<ProjectListItemDto> MapToListItemDtoAsync(Project project)
    {
        var epicCount = await _context.Epics.CountAsync(e => e.ProjectId == project.Id);
        var workItemCount = await _context.WorkItems.CountAsync(w => w.ProjectId == project.Id);

        return new ProjectListItemDto
        {
            Id = project.Id,
            Key = project.Key,
            Name = project.Name,
            OwnerName = $"{project.Owner.FirstName} {project.Owner.LastName}",
            UpdatedAt = project.UpdatedAt,
            IsActive = project.IsActive,
            IsArchived = project.IsArchived,
            EpicCount = epicCount,
            WorkItemCount = workItemCount
        };
    }
}
```

---

### Part 3: Remaining Services (Simplified Guidance)

Due to complexity, I'm providing guidance rather than full implementations:

#### EpicService

**Key Methods**:
- `CreateEpicAsync()` - Use `IKeyGenerationService.GenerateEpicKeyAsync()`
- `GetEpicByIdAsync()` - Include project, assignee, status
- `UpdateEpicAsync()` - Validate status transition via `IStatusService`
- `DeleteEpicAsync()` - Check for associated work items

**Important**: Epic keys should be format: `{PROJECT_KEY}-EPIC-{number}`

#### WorkItemService

**Key Methods**:
- `CreateWorkItemAsync()` - Use `IKeyGenerationService.GenerateWorkItemKeyAsync()`
- Validate hierarchy:
  - Subtasks can only have Task parents
  - Max 1 level of nesting (Task → Subtask, not Subtask → Sub-subtask)
- `ValidateHierarchy()` - Prevent circular references using graph traversal
- `SearchWorkItemsAsync()` - Implement filtering with pagination

**Circular Reference Detection**:
```csharp
private async Task<bool> WouldCreateCircularReference(int workItemId, int? parentWorkItemId)
{
    if (!parentWorkItemId.HasValue)
    {
        return false;
    }

    var visited = new HashSet<int> { workItemId };
    var currentId = parentWorkItemId.Value;

    while (currentId != 0)
    {
        if (visited.Contains(currentId))
        {
            return true; // Circular reference detected
        }

        visited.Add(currentId);

        var parent = await _context.WorkItems
            .AsNoTracking()
            .Where(w => w.Id == currentId)
            .Select(w => new { w.ParentWorkItemId })
            .FirstOrDefaultAsync();

        if (parent == null)
        {
            break;
        }

        currentId = parent.ParentWorkItemId ?? 0;
    }

    return false;
}
```

#### CommentService

**Key Methods**:
- `CreateCommentAsync()` - Validate work item exists
- `UpdateCommentAsync()` - Check author authorization, set `IsEdited = true`
- `DeleteCommentAsync()` - Check author or admin authorization

---

### Part 4: Controllers Implementation

All controllers should follow this pattern:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DocsAndPlannings.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim!);
    }

    [HttpPost]
    public async Task<ActionResult<ProjectDto>> CreateProject([FromBody] CreateProjectRequest request)
    {
        var userId = GetCurrentUserId();
        var project = await _projectService.CreateProjectAsync(request, userId);
        return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectDto>> GetProject(int id)
    {
        var project = await _projectService.GetProjectByIdAsync(id);
        if (project == null)
        {
            return NotFound();
        }
        return Ok(project);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProjectListItemDto>>> GetProjects(
        [FromQuery] int? ownerId,
        [FromQuery] bool? isActive,
        [FromQuery] bool? isArchived,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var projects = await _projectService.GetAllProjectsAsync(ownerId, isActive, isArchived, page, pageSize);
        return Ok(projects);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProjectDto>> UpdateProject(int id, [FromBody] UpdateProjectRequest request)
    {
        var userId = GetCurrentUserId();
        var project = await _projectService.UpdateProjectAsync(id, request, userId);
        return Ok(project);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteProject(int id)
    {
        var userId = GetCurrentUserId();
        await _projectService.DeleteProjectAsync(id, userId);
        return NoContent();
    }

    [HttpPost("{id}/archive")]
    public async Task<ActionResult<ProjectDto>> ArchiveProject(int id)
    {
        var userId = GetCurrentUserId();
        var project = await _projectService.ArchiveProjectAsync(id, userId);
        return Ok(project);
    }

    [HttpPost("{id}/unarchive")]
    public async Task<ActionResult<ProjectDto>> UnarchiveProject(int id)
    {
        var userId = GetCurrentUserId();
        var project = await _projectService.UnarchiveProjectAsync(id, userId);
        return Ok(project);
    }
}
```

**Create controllers for**:
- `EpicsController` (8 endpoints)
- `WorkItemsController` (10 endpoints including search)
- `CommentsController` (5 endpoints)
- `StatusesController` (7 endpoints, admin-only for create/update/delete)

---

### Part 5: Database Migration

Run this command to create the migration:

```bash
cd source/DocsAndPlannings.Core
dotnet ef migrations add Phase3PlanningModule --startup-project ../DocsAndPlannings.Api
```

**Important**: After migration is created, add seed data for default statuses in the `Up()` method or create a database initializer.

---

### Part 6: Service Registration

Add to `source/DocsAndPlannings.Api/Program.cs`:

```csharp
// Phase 3: Planning services
builder.Services.AddScoped<IKeyGenerationService, KeyGenerationService>();
builder.Services.AddScoped<IStatusService, StatusService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IEpicService, EpicService>();
builder.Services.AddScoped<IWorkItemService, WorkItemService>();
builder.Services.AddScoped<ICommentService, CommentService>();
```

**Seed default statuses** at application startup:

```csharp
// After app.Build() but before app.Run()
using (var scope = app.Services.CreateScope())
{
    var statusService = scope.ServiceProvider.GetRequiredService<IStatusService>();
    await statusService.CreateDefaultStatusesAsync();
}
```

---

### Part 7: Testing Strategy

#### Unit Test Structure

Create test files following this pattern:

**File**: `tests/DocsAndPlannings.Core.Tests/Services/ProjectServiceTests.cs`

```csharp
using Xunit;
using Microsoft.EntityFrameworkCore;
using DocsAndPlannings.Core.Data;
using DocsAndPlannings.Core.Services;
using DocsAndPlannings.Core.DTOs.Projects;
using DocsAndPlannings.Core.Exceptions;
using DocsAndPlannings.Core.Exceptions.Planning;
using DocsAndPlannings.Core.Models;

namespace DocsAndPlannings.Core.Tests.Services;

public class ProjectServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ProjectService _service;

    public ProjectServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _service = new ProjectService(_context);

        // Seed test user
        _context.Users.Add(new User
        {
            Id = 1,
            Email = "test@example.com",
            PasswordHash = "hash",
            FirstName = "Test",
            LastName = "User",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        _context.SaveChanges();
    }

    [Fact]
    public async Task CreateProjectAsync_WithValidData_CreatesProject()
    {
        // Arrange
        var request = new CreateProjectRequest
        {
            Key = "TEST",
            Name = "Test Project",
            Description = "A test project"
        };

        // Act
        var result = await _service.CreateProjectAsync(request, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TEST", result.Key);
        Assert.Equal("Test Project", result.Name);
        Assert.Equal(1, result.OwnerId);
    }

    [Fact]
    public async Task CreateProjectAsync_WithDuplicateKey_ThrowsDuplicateKeyException()
    {
        // Arrange
        _context.Projects.Add(new Project
        {
            Key = "TEST",
            Name = "Existing Project",
            OwnerId = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var request = new CreateProjectRequest
        {
            Key = "TEST",
            Name = "New Project"
        };

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateKeyException>(
            () => _service.CreateProjectAsync(request, 1));
    }

    [Fact]
    public async Task GetProjectByIdAsync_WithValidId_ReturnsProject()
    {
        // Arrange
        var project = new Project
        {
            Key = "TEST",
            Name = "Test Project",
            OwnerId = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetProjectByIdAsync(project.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TEST", result.Key);
    }

    [Fact]
    public async Task GetProjectByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = await _service.GetProjectByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteProjectAsync_WithDependencies_ThrowsBadRequestException()
    {
        // Arrange
        var project = new Project
        {
            Key = "TEST",
            Name = "Test Project",
            OwnerId = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Add an epic (dependency)
        var status = new Status
        {
            Name = "TODO",
            OrderIndex = 1,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        _context.Statuses.Add(status);
        await _context.SaveChangesAsync();

        _context.Epics.Add(new Epic
        {
            ProjectId = project.Id,
            Key = "TEST-EPIC-1",
            Summary = "Test Epic",
            StatusId = status.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(
            () => _service.DeleteProjectAsync(project.Id, 1));
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
```

#### Test Coverage Targets

**StatusService** (25 tests):
- CRUD operations (8 tests)
- Transition validation (10 tests)
- Default status creation (3 tests)
- Error scenarios (4 tests)

**ProjectService** (30 tests):
- Create/Read/Update/Delete (12 tests)
- Archive/Unarchive (4 tests)
- Authorization (6 tests)
- Filtering/Pagination (5 tests)
- Error scenarios (3 tests)

**EpicService** (35 tests):
- CRUD operations (10 tests)
- Key generation (5 tests)
- Status transitions (8 tests)
- Statistics (6 tests)
- Error scenarios (6 tests)

**WorkItemService** (60 tests):
- CRUD operations (15 tests)
- Hierarchy validation (15 tests)
- Circular reference detection (10 tests)
- Search/Filter (12 tests)
- Statistics (4 tests)
- Error scenarios (4 tests)

**CommentService** (20 tests):
- CRUD operations (10 tests)
- Authorization (6 tests)
- Error scenarios (4 tests)

**Controllers** (150 tests):
- 30 tests per controller
- Focus on HTTP status codes, authentication, validation

---

### Part 8: Build & Test Commands

```bash
# Restore packages
dotnet restore

# Build in Release mode (treat warnings as errors)
dotnet build --configuration Release -warnaserror

# Run all tests
dotnet test

# Run tests in Release mode
dotnet test --configuration Release

# Check for code style issues
dotnet format --verify-no-changes

# Apply code style fixes
dotnet format
```

---

### Part 9: Completion Checklist

Before marking Phase 3 complete, verify:

- [ ] All 6 services implemented (Status, KeyGeneration, Project, Epic, WorkItem, Comment)
- [ ] All 5 controllers implemented with all endpoints
- [ ] 340+ unit tests written and passing
- [ ] Database migration created and tested
- [ ] Services registered in DI container
- [ ] Default statuses seeded
- [ ] Build succeeds with `-warnaserror`
- [ ] `dotnet format` succeeds
- [ ] No access violations in tests
- [ ] Code review completed (using code-review-task-creator-SKILL)
- [ ] Bug hunting completed (using bug-hunter-tester-SKILL)
- [ ] Roadmap updated
- [ ] Changes committed with proper git workflow

---

### Part 10: Known Issues & Gotchas

1. **KeyGenerationService Concurrency**: Current implementation uses query-and-increment which has a race condition. For production, consider using a database sequence or a distributed lock.

2. **N+1 Query Problem**: The mapper methods in services load counts separately. For better performance, consider loading counts in the initial query using `GroupJoin` or a single aggregate query.

3. **Status Transition Validation**: Default behavior is permissive (allows transitions if no rule exists). Consider if you want restrictive behavior instead.

4. **Work Item Hierarchy Depth**: Currently allows 1 level (Task → Subtask). If you need deeper nesting, update the validation logic.

5. **Soft Delete**: Work items use hard delete. Consider implementing soft delete (IsDeleted flag) for better data safety.

---

### Part 11: Skills to Use

When implementing the remaining work, use these skills:

1. **csharp-SKILL** - For writing C# code following project standards
2. **bug-hunter-tester-SKILL** - After implementation, to find bugs
3. **code-review-task-creator-SKILL** - After code is written, to review
4. **git-workflow-SKILL** - For committing changes with proper messages

---

## Next Steps

1. **Implement StatusService** (use the code provided above)
2. **Implement ProjectService** (use the code provided above)
3. **Implement EpicService, WorkItemService, CommentService** (follow the patterns)
4. **Implement all 5 Controllers** (follow the ProjectsController pattern)
5. **Write all unit tests** (follow the ProjectServiceTests pattern)
6. **Create database migration** (use EF Core migrations)
7. **Register services** in Program.cs
8. **Run code review** and **bug hunting** skills
9. **Update roadmap** to mark Phase 3 complete
10. **Commit everything** with proper git workflow

---

**Estimated Time to Complete Remaining Work**: 40-50 hours

**Current Progress**: 40% complete (DTOs, Exceptions, KeyGenerationService, IStatusService)

**Priority**: Implement services in order: Status → Project → Epic → WorkItem → Comment
