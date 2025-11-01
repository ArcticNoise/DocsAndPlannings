# Phase 4: Kanban Board - Implementation Guide

**Branch**: `feature/phase-4-kanban-board`
**Status**: ✅ COMPLETED
**Created**: 2025-11-01
**Completed**: 2025-11-01

---

## Overview

Phase 4 implements a Kanban board system that allows visual management of work items through a drag-and-drop interface. The board displays work items organized by status columns, enabling efficient workflow visualization and status updates.

**Key Features**:
- Project-specific board configurations
- Dynamic columns based on project statuses
- Board state retrieval with filtering
- Drag-and-drop status transitions via API
- Board customization (column order, WIP limits)
- Comprehensive unit testing

---

## Architecture Design

### Domain Model

```
Board
├─ Project (1:1 relationship)
├─ BoardColumns (1:N relationship)
│  └─ Status reference
│  └─ Column configuration (order, WIP limit, collapsed state)
└─ Board settings (default view, filters)

BoardColumn
├─ Board (N:1 relationship)
├─ Status (N:1 relationship)
└─ Column properties (OrderIndex, WIPLimit, IsCollapsed)

Board View (Query/DTO)
├─ Board configuration
├─ Columns with work items
└─ Aggregated statistics
```

### API Endpoints (8 total)

**Boards Controller**:
1. `GET /api/boards/{projectId}` - Get board for project
2. `POST /api/boards/{projectId}` - Create/initialize board
3. `PUT /api/boards/{projectId}` - Update board settings
4. `DELETE /api/boards/{projectId}` - Delete board configuration
5. `GET /api/boards/{projectId}/view` - Get full board view with work items
6. `PUT /api/boards/{projectId}/columns/{columnId}` - Update column configuration
7. `POST /api/boards/{projectId}/columns/reorder` - Reorder columns
8. `PATCH /api/boards/{projectId}/items/{workItemId}/move` - Move work item to different status

---

## Task Breakdown

### Sprint 1: Foundation & Models (12 hours)

#### Task 1.1: Create Board and BoardColumn Models
**Estimate**: 3 hours
**Priority**: High (blocks all other work)
**Dependencies**: None

**Description**:
Create database models for Board and BoardColumn entities with proper EF Core configuration.

**Implementation Details**:
- Create `Board` model in `source/DocsAndPlannings.Core/Models/Board.cs`
  - Properties: Id, ProjectId, Name, Description, CreatedAt, UpdatedAt
  - Navigation: Project, BoardColumns
- Create `BoardColumn` model in `source/DocsAndPlannings.Core/Models/BoardColumn.cs`
  - Properties: Id, BoardId, StatusId, OrderIndex, WIPLimit, IsCollapsed
  - Navigation: Board, Status
- Create EF Core configurations with proper constraints and indexes

**Acceptance Criteria**:
- [ ] Board model created with all required properties
- [ ] BoardColumn model created with all required properties
- [ ] EF Core configurations include proper foreign keys
- [ ] Index on ProjectId in Board table
- [ ] Index on BoardId and OrderIndex in BoardColumn table
- [ ] Unique constraint on (BoardId, StatusId) in BoardColumn

**Testing Requirements**:
- [ ] Models compile without warnings
- [ ] EF Core configurations are valid

**File Locations**:
- `source/DocsAndPlannings.Core/Models/Board.cs`
- `source/DocsAndPlannings.Core/Models/BoardColumn.cs`
- `source/DocsAndPlannings.Core/Data/Configurations/BoardConfiguration.cs`
- `source/DocsAndPlannings.Core/Data/Configurations/BoardColumnConfiguration.cs`

---

#### Task 1.2: Create Board DTOs
**Estimate**: 2 hours
**Priority**: High
**Dependencies**: Task 1.1

**Description**:
Create Data Transfer Objects for Board API operations.

**Implementation Details**:
- Create `CreateBoardRequest` - Initialize new board
- Create `UpdateBoardRequest` - Update board settings
- Create `BoardDto` - Board entity representation
- Create `BoardColumnDto` - Column configuration
- Create `UpdateBoardColumnRequest` - Update column settings
- Create `ReorderColumnsRequest` - Column reordering
- Create `MoveWorkItemRequest` - Work item movement
- Create `BoardViewDto` - Full board view with work items
- Create `BoardColumnViewDto` - Column with work items

**Acceptance Criteria**:
- [ ] All DTOs created with proper validation attributes
- [ ] DTOs follow project naming conventions
- [ ] XML documentation for all public properties
- [ ] No warnings during compilation

**Testing Requirements**:
- [ ] DTOs serialize/deserialize correctly
- [ ] Validation attributes work as expected

**File Location**:
- `source/DocsAndPlannings.Core/DTOs/Boards/`

---

#### Task 1.3: Create Board Service Interface
**Estimate**: 2 hours
**Priority**: High
**Dependencies**: Task 1.2

**Description**:
Define IBoardService interface with all board management operations.

**Implementation Details**:
```csharp
public interface IBoardService
{
    // Board CRUD
    Task<BoardDto> CreateBoardAsync(int projectId, CreateBoardRequest request, int userId);
    Task<BoardDto?> GetBoardByProjectIdAsync(int projectId);
    Task<BoardDto> UpdateBoardAsync(int projectId, UpdateBoardRequest request, int userId);
    Task DeleteBoardAsync(int projectId, int userId);

    // Board View
    Task<BoardViewDto> GetBoardViewAsync(
        int projectId,
        int[] epicIds = null,
        int[] assigneeIds = null,
        string searchText = null,
        CancellationToken cancellationToken = default);

    // Column Management
    Task<BoardColumnDto> UpdateColumnAsync(int projectId, int columnId, UpdateBoardColumnRequest request, int userId);
    Task ReorderColumnsAsync(int projectId, ReorderColumnsRequest request, int userId);

    // Work Item Movement
    Task MoveWorkItemAsync(int projectId, int workItemId, int toStatusId, int userId);
}
```

**Acceptance Criteria**:
- [ ] Interface defined with all required methods
- [ ] XML documentation for all methods
- [ ] CancellationToken support for async operations
- [ ] Follows project patterns (userId for authorization)

**File Location**:
- `source/DocsAndPlannings.Core/Services/IBoardService.cs`

---

#### Task 1.4: Create Database Migration
**Estimate**: 1 hour
**Priority**: High
**Dependencies**: Task 1.1

**Description**:
Create EF Core migration for Board and BoardColumn tables.

**Implementation Details**:
```bash
cd source/DocsAndPlannings.Core
dotnet ef migrations add Phase4KanbanBoard --startup-project ../DocsAndPlannings.Api
```

**Acceptance Criteria**:
- [ ] Migration created successfully
- [ ] Migration includes Board table creation
- [ ] Migration includes BoardColumn table creation
- [ ] Migration includes proper indexes and constraints
- [ ] Migration can be applied without errors
- [ ] Migration can be reverted without errors

**Testing Requirements**:
- [ ] Test migration Up() on fresh database
- [ ] Test migration Down() to revert changes
- [ ] Verify foreign key constraints work correctly

---

#### Task 1.5: Unit Tests for Models
**Estimate**: 4 hours
**Priority**: Medium
**Dependencies**: Task 1.1, Task 1.4

**Description**:
Create comprehensive unit tests for Board and BoardColumn models.

**Test Coverage** (15 tests):
- Model creation and property assignments (4 tests)
- Navigation property behavior (4 tests)
- EF Core configuration validation (4 tests)
- Constraint enforcement (3 tests)

**Acceptance Criteria**:
- [ ] All model properties tested
- [ ] Navigation properties tested
- [ ] Unique constraints tested
- [ ] Foreign key constraints tested
- [ ] All tests passing

**File Location**:
- `tests/DocsAndPlannings.Core.Tests/Models/BoardTests.cs`
- `tests/DocsAndPlannings.Core.Tests/Models/BoardColumnTests.cs`

---

### Sprint 2: Service Implementation (16 hours)

#### Task 2.1: Implement BoardService - CRUD Operations
**Estimate**: 6 hours
**Priority**: High
**Dependencies**: Task 1.3

**Description**:
Implement core CRUD operations for board management.

**Implementation Details**:
- `CreateBoardAsync()` - Initialize board with default columns from project statuses
- `GetBoardByProjectIdAsync()` - Retrieve board with columns
- `UpdateBoardAsync()` - Update board settings (name, description)
- `DeleteBoardAsync()` - Delete board and columns (cascade)

**Key Logic**:
```csharp
public async Task<BoardDto> CreateBoardAsync(int projectId, CreateBoardRequest request, int userId)
{
    // Validate project exists and user has access
    var project = await ValidateProjectAccessAsync(projectId, userId);

    // Check if board already exists
    var existingBoard = await GetBoardByProjectIdAsync(projectId);
    if (existingBoard != null)
    {
        throw new BadRequestException($"Board already exists for project '{project.Name}'");
    }

    // Create board
    var board = new Board
    {
        ProjectId = projectId,
        Name = request.Name ?? $"{project.Name} Board",
        Description = request.Description,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    _context.Boards.Add(board);
    await _context.SaveChangesAsync();

    // Initialize columns from active statuses
    var statuses = await _context.Statuses
        .Where(s => s.IsActive)
        .OrderBy(s => s.OrderIndex)
        .ToListAsync();

    var columns = statuses.Select((status, index) => new BoardColumn
    {
        BoardId = board.Id,
        StatusId = status.Id,
        OrderIndex = index,
        WIPLimit = null, // No limit by default
        IsCollapsed = false
    }).ToList();

    _context.BoardColumns.AddRange(columns);
    await _context.SaveChangesAsync();

    return await MapToDtoAsync(board);
}
```

**Acceptance Criteria**:
- [ ] CreateBoardAsync creates board with default columns
- [ ] GetBoardByProjectIdAsync retrieves board with columns
- [ ] UpdateBoardAsync updates settings
- [ ] DeleteBoardAsync removes board and columns
- [ ] Authorization enforced (project owner only)
- [ ] Proper error handling for edge cases

**Testing Requirements**:
- [ ] 20 unit tests for CRUD operations
- [ ] Test authorization scenarios
- [ ] Test error scenarios (duplicate board, not found, etc.)

**File Location**:
- `source/DocsAndPlannings.Core/Services/BoardService.cs`

---

#### Task 2.2: Implement BoardService - Board View
**Estimate**: 5 hours
**Priority**: High
**Dependencies**: Task 2.1

**Description**:
Implement GetBoardViewAsync to retrieve complete board state with work items grouped by columns.

**Implementation Details**:
```csharp
public async Task<BoardViewDto> GetBoardViewAsync(
    int projectId,
    int[]? epicIds = null,
    int[]? assigneeIds = null,
    string? searchText = null,
    CancellationToken cancellationToken = default)
{
    var board = await _context.Boards
        .Include(b => b.BoardColumns)
            .ThenInclude(bc => bc.Status)
        .FirstOrDefaultAsync(b => b.ProjectId == projectId, cancellationToken);

    if (board == null)
    {
        throw new NotFoundException($"Board not found for project ID {projectId}");
    }

    // Build work items query with filters
    var workItemsQuery = _context.WorkItems
        .Include(w => w.Status)
        .Include(w => w.Assignee)
        .Include(w => w.Epic)
        .Where(w => w.ProjectId == projectId);

    if (epicIds != null && epicIds.Length > 0)
    {
        workItemsQuery = workItemsQuery.Where(w => w.EpicId != null && epicIds.Contains(w.EpicId.Value));
    }

    if (assigneeIds != null && assigneeIds.Length > 0)
    {
        workItemsQuery = workItemsQuery.Where(w => w.AssigneeId != null && assigneeIds.Contains(w.AssigneeId.Value));
    }

    if (!string.IsNullOrWhiteSpace(searchText))
    {
        workItemsQuery = workItemsQuery.Where(w =>
            w.Summary.Contains(searchText) ||
            w.Description.Contains(searchText));
    }

    var workItems = await workItemsQuery
        .OrderBy(w => w.OrderIndex)
        .ToListAsync(cancellationToken);

    // Group work items by status
    var itemsByStatus = workItems
        .GroupBy(w => w.StatusId)
        .ToDictionary(g => g.Key, g => g.ToList());

    // Build column views
    var columnViews = board.BoardColumns
        .OrderBy(c => c.OrderIndex)
        .Select(column => new BoardColumnViewDto
        {
            Id = column.Id,
            StatusId = column.StatusId,
            StatusName = column.Status.Name,
            StatusColor = column.Status.Color,
            OrderIndex = column.OrderIndex,
            WIPLimit = column.WIPLimit,
            IsCollapsed = column.IsCollapsed,
            WorkItems = itemsByStatus.TryGetValue(column.StatusId, out var items)
                ? items.Select(MapToWorkItemCardDto).ToList()
                : new List<WorkItemCardDto>(),
            ItemCount = itemsByStatus.TryGetValue(column.StatusId, out var countItems)
                ? countItems.Count
                : 0
        })
        .ToList();

    return new BoardViewDto
    {
        BoardId = board.Id,
        ProjectId = board.ProjectId,
        Name = board.Name,
        Description = board.Description,
        Columns = columnViews,
        TotalItems = workItems.Count
    };
}
```

**Acceptance Criteria**:
- [ ] GetBoardViewAsync returns complete board state
- [ ] Work items grouped by status columns
- [ ] Filtering by epic IDs works correctly
- [ ] Filtering by assignee IDs works correctly
- [ ] Search text filtering works correctly
- [ ] Work items ordered within columns
- [ ] Column statistics calculated correctly
- [ ] CancellationToken honored

**Testing Requirements**:
- [ ] 15 unit tests for board view retrieval
- [ ] Test all filter combinations
- [ ] Test empty board scenarios
- [ ] Test performance with large datasets

---

#### Task 2.3: Implement BoardService - Column Management
**Estimate**: 3 hours
**Priority**: Medium
**Dependencies**: Task 2.1

**Description**:
Implement column configuration updates and reordering.

**Implementation Details**:
- `UpdateColumnAsync()` - Update column WIP limit, collapsed state
- `ReorderColumnsAsync()` - Change column display order

**Acceptance Criteria**:
- [ ] UpdateColumnAsync updates column properties
- [ ] ReorderColumnsAsync reorders all columns atomically
- [ ] Validation prevents invalid states
- [ ] Authorization enforced (project owner only)

**Testing Requirements**:
- [ ] 10 unit tests for column management
- [ ] Test reordering edge cases
- [ ] Test concurrent updates

**File Location**:
- `source/DocsAndPlannings.Core/Services/BoardService.cs`

---

#### Task 2.4: Implement BoardService - Work Item Movement
**Estimate**: 2 hours
**Priority**: High
**Dependencies**: Task 2.1

**Description**:
Implement work item status updates via drag-and-drop API.

**Implementation Details**:
```csharp
public async Task MoveWorkItemAsync(int projectId, int workItemId, int toStatusId, int userId)
{
    // Validate board exists
    var board = await GetBoardByProjectIdAsync(projectId);
    if (board == null)
    {
        throw new NotFoundException($"Board not found for project ID {projectId}");
    }

    // Get work item
    var workItem = await _context.WorkItems
        .FirstOrDefaultAsync(w => w.Id == workItemId && w.ProjectId == projectId);

    if (workItem == null)
    {
        throw new NotFoundException($"Work item ID {workItemId} not found in project ID {projectId}");
    }

    // Validate status transition using StatusService
    var isValidTransition = await _statusService.ValidateTransitionAsync(workItem.StatusId, toStatusId);
    if (!isValidTransition)
    {
        var fromStatus = await _context.Statuses.FindAsync(workItem.StatusId);
        var toStatus = await _context.Statuses.FindAsync(toStatusId);
        throw new InvalidStatusTransitionException(fromStatus?.Name, toStatus?.Name);
    }

    // Update work item status
    workItem.StatusId = toStatusId;
    workItem.UpdatedAt = DateTime.UtcNow;

    await _context.SaveChangesAsync();
}
```

**Acceptance Criteria**:
- [ ] MoveWorkItemAsync updates work item status
- [ ] Status transition validation enforced
- [ ] Work item must belong to project
- [ ] Proper error messages for invalid transitions

**Testing Requirements**:
- [ ] 8 unit tests for work item movement
- [ ] Test valid transitions
- [ ] Test invalid transitions
- [ ] Test authorization scenarios

---

### Sprint 3: API & Testing (12 hours)

#### Task 3.1: Implement BoardsController
**Estimate**: 4 hours
**Priority**: High
**Dependencies**: Task 2.1, Task 2.2, Task 2.3, Task 2.4

**Description**:
Create REST API controller for board operations.

**Implementation Details**:
Create controller with 8 endpoints following project patterns:
1. GET /api/boards/{projectId}
2. POST /api/boards/{projectId}
3. PUT /api/boards/{projectId}
4. DELETE /api/boards/{projectId}
5. GET /api/boards/{projectId}/view
6. PUT /api/boards/{projectId}/columns/{columnId}
7. POST /api/boards/{projectId}/columns/reorder
8. PATCH /api/boards/{projectId}/items/{workItemId}/move

**Acceptance Criteria**:
- [ ] All 8 endpoints implemented
- [ ] Proper HTTP status codes (200, 201, 204, 400, 401, 403, 404)
- [ ] [Authorize] attribute applied
- [ ] Request validation with model binding
- [ ] XML documentation for all endpoints
- [ ] CancellationToken support where applicable

**File Location**:
- `source/DocsAndPlannings.Api/Controllers/BoardsController.cs`

---

#### Task 3.2: Service Registration & Configuration
**Estimate**: 1 hour
**Priority**: High
**Dependencies**: Task 2.1

**Description**:
Register BoardService in dependency injection container.

**Implementation Details**:
Update `source/DocsAndPlannings.Api/Program.cs`:
```csharp
// Phase 4: Board services
builder.Services.AddScoped<IBoardService, BoardService>();
```

**Acceptance Criteria**:
- [ ] IBoardService registered in DI container
- [ ] Service resolves correctly
- [ ] No circular dependencies

---

#### Task 3.3: Unit Tests for BoardService
**Estimate**: 5 hours
**Priority**: High
**Dependencies**: Task 2.1, Task 2.2, Task 2.3, Task 2.4

**Description**:
Comprehensive unit tests for all BoardService methods.

**Test Coverage** (53 tests):
- CreateBoardAsync (10 tests)
  - Valid creation with default columns
  - Duplicate board prevention
  - Authorization checks
  - Project not found scenarios
- GetBoardByProjectIdAsync (5 tests)
  - Board retrieval with columns
  - Non-existent board
  - Eager loading validation
- UpdateBoardAsync (8 tests)
  - Valid updates
  - Authorization checks
  - Not found scenarios
- DeleteBoardAsync (6 tests)
  - Valid deletion
  - Cascade delete of columns
  - Authorization checks
- GetBoardViewAsync (15 tests)
  - Full board view retrieval
  - Epic filtering
  - Assignee filtering
  - Search text filtering
  - Combined filters
  - Empty board scenarios
  - Column statistics
- UpdateColumnAsync (5 tests)
  - WIP limit updates
  - Collapsed state updates
  - Authorization checks
- ReorderColumnsAsync (6 tests)
  - Valid reordering
  - Atomic updates
  - Invalid order scenarios
- MoveWorkItemAsync (8 tests)
  - Valid status transitions
  - Invalid transitions
  - Work item not in project
  - Authorization checks

**Acceptance Criteria**:
- [ ] 53 tests implemented
- [ ] All tests passing
- [ ] Code coverage >90%
- [ ] Edge cases covered
- [ ] Error scenarios tested

**File Location**:
- `tests/DocsAndPlannings.Core.Tests/Services/BoardServiceTests.cs`

---

#### Task 3.4: Unit Tests for BoardsController
**Estimate**: 2 hours
**Priority**: High
**Dependencies**: Task 3.1

**Description**:
Unit tests for BoardsController endpoints.

**Test Coverage** (24 tests):
- GET /api/boards/{projectId} (3 tests)
- POST /api/boards/{projectId} (4 tests)
- PUT /api/boards/{projectId} (3 tests)
- DELETE /api/boards/{projectId} (3 tests)
- GET /api/boards/{projectId}/view (4 tests)
- PUT /api/boards/{projectId}/columns/{columnId} (3 tests)
- POST /api/boards/{projectId}/columns/reorder (2 tests)
- PATCH /api/boards/{projectId}/items/{workItemId}/move (2 tests)

**Acceptance Criteria**:
- [ ] 24 tests implemented
- [ ] HTTP status codes validated
- [ ] Authentication/authorization tested
- [ ] Request validation tested
- [ ] All tests passing

**File Location**:
- `tests/DocsAndPlannings.Api.Tests/Controllers/BoardsControllerTests.cs`

---

### Sprint 4: Quality & Documentation (8 hours)

#### Task 4.1: Integration Testing
**Estimate**: 3 hours
**Priority**: Medium
**Dependencies**: Task 3.1, Task 3.2

**Description**:
End-to-end integration tests for board workflows.

**Test Scenarios** (5 integration tests):
1. Create board → Add work items → Get board view
2. Move work items across statuses → Verify board state
3. Filter board by epics → Verify results
4. Reorder columns → Verify persistence
5. Update column WIP limits → Verify enforcement

**Acceptance Criteria**:
- [ ] 5 integration tests implemented
- [ ] Tests use real database (SQLite in-memory)
- [ ] Full request/response cycle tested
- [ ] All tests passing

**File Location**:
- `tests/DocsAndPlannings.Integration.Tests/BoardWorkflowTests.cs`

---

#### Task 4.2: Code Review
**Estimate**: 2 hours
**Priority**: High
**Dependencies**: All implementation tasks

**Description**:
Use code-review-task-creator-SKILL to review all Phase 4 code.

**Review Focus**:
- [ ] Mandatory rules compliance (CLAUDE.md)
- [ ] Architectural patterns consistency
- [ ] Error handling completeness
- [ ] Performance considerations
- [ ] Security validation
- [ ] XML documentation quality

**Acceptance Criteria**:
- [ ] Code review skill executed
- [ ] All critical issues resolved
- [ ] All high-priority issues resolved
- [ ] Medium/low issues documented

---

#### Task 4.3: Bug Hunting & Testing
**Estimate**: 2 hours
**Priority**: High
**Dependencies**: Task 4.2

**Description**:
Use bug-hunter-tester-SKILL to find and fix bugs.

**Testing Focus**:
- [ ] Concurrent board updates
- [ ] Large dataset performance
- [ ] Edge cases in filtering
- [ ] Authorization bypass attempts
- [ ] SQL injection vectors
- [ ] Memory leaks in long-running scenarios

**Acceptance Criteria**:
- [ ] Bug hunting skill executed
- [ ] All critical bugs fixed
- [ ] All high-priority bugs fixed
- [ ] Medium/low bugs documented

---

#### Task 4.4: Documentation & Roadmap Update
**Estimate**: 1 hour
**Priority**: Medium
**Dependencies**: All tasks

**Description**:
Update project documentation and roadmap.

**Updates Required**:
- [ ] Update roadmap.md with Phase 4 completion
- [ ] Add API endpoint documentation
- [ ] Update database schema documentation
- [ ] Create Phase 4 completion summary

**File Locations**:
- `plans/roadmap.md`
- `docs/api-endpoints.md`
- `docs/database-schema.md`

---

## Summary

### Total Effort Estimate: 48 hours

**Sprint Breakdown**:
- Sprint 1: Foundation & Models (12 hours)
- Sprint 2: Service Implementation (16 hours)
- Sprint 3: API & Testing (12 hours)
- Sprint 4: Quality & Documentation (8 hours)

### Deliverables

**Code Artifacts** (14 files):
- 2 Model classes
- 2 EF Core configurations
- 9 DTOs
- 1 Service interface
- 1 Service implementation
- 1 Controller
- 1 Database migration

**Tests** (92 tests):
- 15 Model tests
- 53 Service tests
- 24 Controller tests
- 5 Integration tests

**API Endpoints**: 8 REST endpoints

**Database Tables**: 2 new tables (Board, BoardColumn)

---

## Dependencies

**External Dependencies**:
- Phase 3 must be complete (Projects, Epics, WorkItems, Statuses)
- IProjectService for project validation
- IStatusService for transition validation
- IWorkItemService integration

**Internal Dependencies**:
- All Sprint 1 tasks must complete before Sprint 2
- Service implementation (Sprint 2) must complete before API (Sprint 3)
- Implementation must complete before quality checks (Sprint 4)

---

## Risks & Mitigation

### Risk 1: Performance with Large Boards
**Impact**: High
**Probability**: Medium
**Mitigation**:
- Implement pagination for board view
- Add caching for board state
- Optimize queries with proper indexes
- Load test with 1000+ work items

### Risk 2: Concurrent Updates
**Impact**: Medium
**Probability**: Medium
**Mitigation**:
- Use optimistic concurrency (RowVersion)
- Implement conflict detection
- Add retry logic for transient failures

### Risk 3: Status Transition Complexity
**Impact**: Low
**Probability**: Low
**Mitigation**:
- Leverage existing StatusService validation
- Comprehensive test coverage for edge cases
- Clear error messages for users

---

## Success Criteria

Phase 4 is complete when:
- [ ] All 14 code files implemented
- [ ] All 92 tests passing
- [ ] All 8 API endpoints functional
- [ ] Database migration applied successfully
- [ ] Build succeeds with -warnaserror
- [ ] dotnet format succeeds
- [ ] Code review completed with no critical issues
- [ ] Bug hunting completed with no critical bugs
- [ ] Roadmap updated
- [ ] Changes committed with proper git workflow

---

## Skills Usage Plan

1. **csharp-SKILL** - For all C# code implementation
2. **code-review-task-creator-SKILL** - After code complete (Task 4.2)
3. **bug-hunter-tester-SKILL** - After code review (Task 4.3)
4. **git-workflow-SKILL** - For all commits and PR creation

---

## Next Steps

1. Review and validate this plan
2. Use dev-plan-validator-SKILL to validate completeness
3. Create feature branch: `feature/phase-4-kanban-board`
4. Begin implementation with Task 1.1
5. Track progress with TodoWrite tool

---

**Created By**: dev-plan-manager-SKILL
**Date**: 2025-11-01
**Estimated Completion**: Sprint 4 completion + quality gates
