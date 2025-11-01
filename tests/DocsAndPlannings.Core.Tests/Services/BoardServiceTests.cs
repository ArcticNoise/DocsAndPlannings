using DocsAndPlannings.Core.Data;
using DocsAndPlannings.Core.DTOs.Boards;
using DocsAndPlannings.Core.Exceptions;
using DocsAndPlannings.Core.Exceptions.Planning;
using DocsAndPlannings.Core.Models;
using DocsAndPlannings.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace DocsAndPlannings.Core.Tests.Services;

public sealed class BoardServiceTests : IDisposable
{
    private readonly ApplicationDbContext m_Context;
    private readonly IStatusService m_StatusService;
    private readonly IBoardService m_BoardService;

    public BoardServiceTests()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        m_Context = new ApplicationDbContext(options);
        m_StatusService = new StatusService(m_Context);
        m_BoardService = new BoardService(m_Context, m_StatusService);

        SeedTestData();
    }

    public void Dispose()
    {
        m_Context.Dispose();
    }

    private void SeedTestData()
    {
        // Create test user
        User owner = new User
        {
            Id = 1,
            Email = "owner@example.com",
            PasswordHash = "hash",
            FirstName = "Test",
            LastName = "Owner",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        User otherUser = new User
        {
            Id = 2,
            Email = "other@example.com",
            PasswordHash = "hash",
            FirstName = "Other",
            LastName = "User",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        // Create test statuses
        Status todoStatus = new Status
        {
            Id = 1,
            Name = "TODO",
            Color = "#95a5a6",
            OrderIndex = 1,
            IsDefaultForNew = true,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        Status inProgressStatus = new Status
        {
            Id = 2,
            Name = "IN PROGRESS",
            Color = "#3498db",
            OrderIndex = 2,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        Status doneStatus = new Status
        {
            Id = 3,
            Name = "DONE",
            Color = "#2ecc71",
            OrderIndex = 3,
            IsCompletedStatus = true,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        // Create test project
        Project project = new Project
        {
            Id = 1,
            Key = "TEST",
            Name = "Test Project",
            Description = "A test project",
            OwnerId = 1,
            IsActive = true,
            IsArchived = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Create status transitions
        StatusTransition todoToInProgress = new StatusTransition
        {
            FromStatusId = 1,
            ToStatusId = 2,
            CreatedAt = DateTime.UtcNow
        };

        StatusTransition inProgressToDone = new StatusTransition
        {
            FromStatusId = 2,
            ToStatusId = 3,
            CreatedAt = DateTime.UtcNow
        };

        StatusTransition inProgressToTodo = new StatusTransition
        {
            FromStatusId = 2,
            ToStatusId = 1,
            CreatedAt = DateTime.UtcNow
        };

        // Add a disallowed transition for testing
        StatusTransition doneToTodoDisallowed = new StatusTransition
        {
            FromStatusId = 3,
            ToStatusId = 1,
            IsAllowed = false,
            CreatedAt = DateTime.UtcNow
        };

        m_Context.Users.AddRange(owner, otherUser);
        m_Context.Statuses.AddRange(todoStatus, inProgressStatus, doneStatus);
        m_Context.StatusTransitions.AddRange(todoToInProgress, inProgressToDone, inProgressToTodo, doneToTodoDisallowed);
        m_Context.Projects.Add(project);
        m_Context.SaveChanges();
    }

    #region CreateBoardAsync Tests

    [Fact]
    public async Task CreateBoardAsync_ValidRequest_CreatesBoard()
    {
        CreateBoardRequest request = new CreateBoardRequest
        {
            Name = "Test Board",
            Description = "Test Description"
        };

        BoardDto result = await m_BoardService.CreateBoardAsync(1, request, 1);

        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal(1, result.ProjectId);
        Assert.Equal("Test Board", result.Name);
        Assert.Equal("Test Description", result.Description);
        Assert.Equal(3, result.Columns.Count); // Should have 3 columns for 3 statuses
    }

    [Fact]
    public async Task CreateBoardAsync_NullName_UsesDefaultName()
    {
        CreateBoardRequest request = new CreateBoardRequest
        {
            Name = null,
            Description = "Test Description"
        };

        BoardDto result = await m_BoardService.CreateBoardAsync(1, request, 1);

        Assert.NotNull(result);
        Assert.Equal("Test Project Board", result.Name);
    }

    [Fact]
    public async Task CreateBoardAsync_CreatesColumnsInOrderByStatusOrderIndex()
    {
        CreateBoardRequest request = new CreateBoardRequest
        {
            Name = "Test Board"
        };

        BoardDto result = await m_BoardService.CreateBoardAsync(1, request, 1);

        Assert.Equal(3, result.Columns.Count);
        Assert.Equal("TODO", result.Columns[0].StatusName);
        Assert.Equal(0, result.Columns[0].OrderIndex);
        Assert.Equal("IN PROGRESS", result.Columns[1].StatusName);
        Assert.Equal(1, result.Columns[1].OrderIndex);
        Assert.Equal("DONE", result.Columns[2].StatusName);
        Assert.Equal(2, result.Columns[2].OrderIndex);
    }

    [Fact]
    public async Task CreateBoardAsync_NullRequest_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            m_BoardService.CreateBoardAsync(1, null!, 1));
    }

    [Fact]
    public async Task CreateBoardAsync_NonExistentProject_ThrowsNotFoundException()
    {
        CreateBoardRequest request = new CreateBoardRequest { Name = "Test Board" };

        await Assert.ThrowsAsync<NotFoundException>(() =>
            m_BoardService.CreateBoardAsync(999, request, 1));
    }

    [Fact]
    public async Task CreateBoardAsync_NotProjectOwner_ThrowsForbiddenException()
    {
        CreateBoardRequest request = new CreateBoardRequest { Name = "Test Board" };

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            m_BoardService.CreateBoardAsync(1, request, 2)); // User 2 is not owner
    }

    [Fact]
    public async Task CreateBoardAsync_BoardAlreadyExists_ThrowsBadRequestException()
    {
        // Create first board
        CreateBoardRequest request = new CreateBoardRequest { Name = "Test Board" };
        await m_BoardService.CreateBoardAsync(1, request, 1);

        // Try to create second board for same project
        CreateBoardRequest request2 = new CreateBoardRequest { Name = "Another Board" };
        await Assert.ThrowsAsync<BadRequestException>(() =>
            m_BoardService.CreateBoardAsync(1, request2, 1));
    }

    [Fact]
    public async Task CreateBoardAsync_ColumnsHaveCorrectStatusColors()
    {
        CreateBoardRequest request = new CreateBoardRequest { Name = "Test Board" };

        BoardDto result = await m_BoardService.CreateBoardAsync(1, request, 1);

        Assert.Equal("#95a5a6", result.Columns[0].StatusColor);
        Assert.Equal("#3498db", result.Columns[1].StatusColor);
        Assert.Equal("#2ecc71", result.Columns[2].StatusColor);
    }

    [Fact]
    public async Task CreateBoardAsync_ColumnsHaveNullWIPLimit()
    {
        CreateBoardRequest request = new CreateBoardRequest { Name = "Test Board" };

        BoardDto result = await m_BoardService.CreateBoardAsync(1, request, 1);

        Assert.All(result.Columns, column => Assert.Null(column.WIPLimit));
    }

    [Fact]
    public async Task CreateBoardAsync_ColumnsAreNotCollapsed()
    {
        CreateBoardRequest request = new CreateBoardRequest { Name = "Test Board" };

        BoardDto result = await m_BoardService.CreateBoardAsync(1, request, 1);

        Assert.All(result.Columns, column => Assert.False(column.IsCollapsed));
    }

    [Fact]
    public async Task CreateBoardAsync_SetsCreatedAndUpdatedAt()
    {
        CreateBoardRequest request = new CreateBoardRequest { Name = "Test Board" };
        DateTime before = DateTime.UtcNow;

        BoardDto result = await m_BoardService.CreateBoardAsync(1, request, 1);

        DateTime after = DateTime.UtcNow;
        Assert.InRange(result.CreatedAt, before, after);
        Assert.InRange(result.UpdatedAt, before, after);
    }

    #endregion

    #region GetBoardByProjectIdAsync Tests

    [Fact]
    public async Task GetBoardByProjectIdAsync_ExistingBoard_ReturnsBoard()
    {
        CreateBoardRequest request = new CreateBoardRequest { Name = "Test Board" };
        BoardDto created = await m_BoardService.CreateBoardAsync(1, request, 1);

        BoardDto? result = await m_BoardService.GetBoardByProjectIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.Equal("Test Board", result.Name);
    }

    [Fact]
    public async Task GetBoardByProjectIdAsync_NonExistentBoard_ReturnsNull()
    {
        BoardDto? result = await m_BoardService.GetBoardByProjectIdAsync(1);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetBoardByProjectIdAsync_ReturnsColumnsInOrder()
    {
        CreateBoardRequest request = new CreateBoardRequest { Name = "Test Board" };
        await m_BoardService.CreateBoardAsync(1, request, 1);

        BoardDto? result = await m_BoardService.GetBoardByProjectIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(3, result.Columns.Count);
        Assert.Equal(0, result.Columns[0].OrderIndex);
        Assert.Equal(1, result.Columns[1].OrderIndex);
        Assert.Equal(2, result.Columns[2].OrderIndex);
    }

    [Fact]
    public async Task GetBoardByProjectIdAsync_IncludesColumnStatusInformation()
    {
        CreateBoardRequest request = new CreateBoardRequest { Name = "Test Board" };
        await m_BoardService.CreateBoardAsync(1, request, 1);

        BoardDto? result = await m_BoardService.GetBoardByProjectIdAsync(1);

        Assert.NotNull(result);
        Assert.All(result.Columns, column =>
        {
            Assert.True(column.StatusId > 0);
            Assert.False(string.IsNullOrEmpty(column.StatusName));
            Assert.False(string.IsNullOrEmpty(column.StatusColor));
        });
    }

    #endregion

    #region UpdateBoardAsync Tests

    [Fact]
    public async Task UpdateBoardAsync_ValidRequest_UpdatesBoard()
    {
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Old Name" };
        await m_BoardService.CreateBoardAsync(1, createRequest, 1);

        UpdateBoardRequest updateRequest = new UpdateBoardRequest
        {
            Name = "New Name",
            Description = "New Description"
        };

        BoardDto result = await m_BoardService.UpdateBoardAsync(1, updateRequest, 1);

        Assert.Equal("New Name", result.Name);
        Assert.Equal("New Description", result.Description);
    }

    [Fact]
    public async Task UpdateBoardAsync_NullRequest_ThrowsArgumentNullException()
    {
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        await m_BoardService.CreateBoardAsync(1, createRequest, 1);

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            m_BoardService.UpdateBoardAsync(1, null!, 1));
    }

    [Fact]
    public async Task UpdateBoardAsync_NonExistentBoard_ThrowsNotFoundException()
    {
        UpdateBoardRequest updateRequest = new UpdateBoardRequest
        {
            Name = "New Name",
            Description = "New Description"
        };

        await Assert.ThrowsAsync<NotFoundException>(() =>
            m_BoardService.UpdateBoardAsync(1, updateRequest, 1));
    }

    [Fact]
    public async Task UpdateBoardAsync_NotProjectOwner_ThrowsForbiddenException()
    {
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        await m_BoardService.CreateBoardAsync(1, createRequest, 1);

        UpdateBoardRequest updateRequest = new UpdateBoardRequest
        {
            Name = "New Name",
            Description = "New Description"
        };

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            m_BoardService.UpdateBoardAsync(1, updateRequest, 2)); // User 2 is not owner
    }

    [Fact]
    public async Task UpdateBoardAsync_UpdatesUpdatedAt()
    {
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Old Name" };
        BoardDto created = await m_BoardService.CreateBoardAsync(1, createRequest, 1);

        await Task.Delay(10); // Ensure time difference

        UpdateBoardRequest updateRequest = new UpdateBoardRequest
        {
            Name = "New Name",
            Description = "New Description"
        };

        BoardDto result = await m_BoardService.UpdateBoardAsync(1, updateRequest, 1);

        Assert.True(result.UpdatedAt > created.UpdatedAt);
    }

    [Fact]
    public async Task UpdateBoardAsync_DoesNotChangeCreatedAt()
    {
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Old Name" };
        BoardDto created = await m_BoardService.CreateBoardAsync(1, createRequest, 1);

        UpdateBoardRequest updateRequest = new UpdateBoardRequest
        {
            Name = "New Name",
            Description = "New Description"
        };

        BoardDto result = await m_BoardService.UpdateBoardAsync(1, updateRequest, 1);

        Assert.Equal(created.CreatedAt, result.CreatedAt);
    }

    [Fact]
    public async Task UpdateBoardAsync_DoesNotChangeColumns()
    {
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Old Name" };
        BoardDto created = await m_BoardService.CreateBoardAsync(1, createRequest, 1);

        UpdateBoardRequest updateRequest = new UpdateBoardRequest
        {
            Name = "New Name",
            Description = "New Description"
        };

        BoardDto result = await m_BoardService.UpdateBoardAsync(1, updateRequest, 1);

        Assert.Equal(created.Columns.Count, result.Columns.Count);
    }

    [Fact]
    public async Task UpdateBoardAsync_UpdatesName()
    {
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Old Name" };
        await m_BoardService.CreateBoardAsync(1, createRequest, 1);

        UpdateBoardRequest updateRequest = new UpdateBoardRequest
        {
            Name = "New Name",
            Description = "Description"
        };

        BoardDto result = await m_BoardService.UpdateBoardAsync(1, updateRequest, 1);

        Assert.Equal("New Name", result.Name);
    }

    [Fact]
    public async Task UpdateBoardAsync_UpdatesDescription()
    {
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        await m_BoardService.CreateBoardAsync(1, createRequest, 1);

        UpdateBoardRequest updateRequest = new UpdateBoardRequest
        {
            Name = "Test Board",
            Description = "New Description"
        };

        BoardDto result = await m_BoardService.UpdateBoardAsync(1, updateRequest, 1);

        Assert.Equal("New Description", result.Description);
    }

    [Fact]
    public async Task UpdateBoardAsync_CanSetDescriptionToNull()
    {
        CreateBoardRequest createRequest = new CreateBoardRequest
        {
            Name = "Test Board",
            Description = "Old Description"
        };
        await m_BoardService.CreateBoardAsync(1, createRequest, 1);

        UpdateBoardRequest updateRequest = new UpdateBoardRequest
        {
            Name = "Test Board",
            Description = null
        };

        BoardDto result = await m_BoardService.UpdateBoardAsync(1, updateRequest, 1);

        Assert.Null(result.Description);
    }

    #endregion

    #region DeleteBoardAsync Tests

    [Fact]
    public async Task DeleteBoardAsync_ExistingBoard_DeletesBoard()
    {
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        await m_BoardService.CreateBoardAsync(1, createRequest, 1);

        await m_BoardService.DeleteBoardAsync(1, 1);

        BoardDto? result = await m_BoardService.GetBoardByProjectIdAsync(1);
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteBoardAsync_NonExistentBoard_ThrowsNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            m_BoardService.DeleteBoardAsync(1, 1));
    }

    [Fact]
    public async Task DeleteBoardAsync_NotProjectOwner_ThrowsForbiddenException()
    {
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        await m_BoardService.CreateBoardAsync(1, createRequest, 1);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            m_BoardService.DeleteBoardAsync(1, 2)); // User 2 is not owner
    }

    [Fact]
    public async Task DeleteBoardAsync_DeletesCascadeColumns()
    {
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        BoardDto created = await m_BoardService.CreateBoardAsync(1, createRequest, 1);

        await m_BoardService.DeleteBoardAsync(1, 1);

        // Verify columns are also deleted
        int columnCount = await m_Context.BoardColumns.CountAsync(c => c.BoardId == created.Id);
        Assert.Equal(0, columnCount);
    }

    [Fact]
    public async Task DeleteBoardAsync_DoesNotDeleteProject()
    {
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        await m_BoardService.CreateBoardAsync(1, createRequest, 1);

        await m_BoardService.DeleteBoardAsync(1, 1);

        Project? project = await m_Context.Projects.FindAsync(1);
        Assert.NotNull(project);
    }

    [Fact]
    public async Task DeleteBoardAsync_DoesNotDeleteStatuses()
    {
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        await m_BoardService.CreateBoardAsync(1, createRequest, 1);

        int statusCountBefore = await m_Context.Statuses.CountAsync();
        await m_BoardService.DeleteBoardAsync(1, 1);
        int statusCountAfter = await m_Context.Statuses.CountAsync();

        Assert.Equal(statusCountBefore, statusCountAfter);
    }

    [Fact]
    public async Task DeleteBoardAsync_AllowsRecreatingBoard()
    {
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        await m_BoardService.CreateBoardAsync(1, createRequest, 1);

        await m_BoardService.DeleteBoardAsync(1, 1);

        // Should be able to create a new board
        CreateBoardRequest newRequest = new CreateBoardRequest { Name = "New Board" };
        BoardDto result = await m_BoardService.CreateBoardAsync(1, newRequest, 1);

        Assert.NotNull(result);
        Assert.Equal("New Board", result.Name);
    }

    [Fact]
    public async Task DeleteBoardAsync_CompletesSuccessfully()
    {
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        await m_BoardService.CreateBoardAsync(1, createRequest, 1);

        // Should not throw
        await m_BoardService.DeleteBoardAsync(1, 1);
    }

    #endregion

    #region GetBoardViewAsync Tests

    [Fact]
    public async Task GetBoardViewAsync_NoFilters_ReturnsAllWorkItems()
    {
        // Create board
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        BoardDto board = await m_BoardService.CreateBoardAsync(1, createRequest, 1);

        // Create work items
        WorkItem item1 = new WorkItem
        {
            ProjectId = 1,
            Key = "TEST-1",
            Type = WorkItemType.Task,
            Summary = "Task 1",
            StatusId = 1,
            Priority = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        WorkItem item2 = new WorkItem
        {
            ProjectId = 1,
            Key = "TEST-2",
            Type = WorkItemType.Bug,
            Summary = "Bug 1",
            StatusId = 2,
            Priority = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        m_Context.WorkItems.AddRange(item1, item2);
        await m_Context.SaveChangesAsync();

        BoardViewDto result = await m_BoardService.GetBoardViewAsync(1);

        Assert.NotNull(result);
        Assert.Equal(2, result.TotalItems);
        Assert.Equal(board.Id, result.BoardId);
        Assert.Equal(1, result.ProjectId);
    }

    [Fact]
    public async Task GetBoardViewAsync_GroupsWorkItemsByStatus()
    {
        // Create board
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        await m_BoardService.CreateBoardAsync(1, createRequest, 1);

        // Create work items in different statuses
        WorkItem item1 = new WorkItem
        {
            ProjectId = 1,
            Key = "TEST-1",
            Type = WorkItemType.Task,
            Summary = "Task in TODO",
            StatusId = 1,
            Priority = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        WorkItem item2 = new WorkItem
        {
            ProjectId = 1,
            Key = "TEST-2",
            Type = WorkItemType.Task,
            Summary = "Task in Progress",
            StatusId = 2,
            Priority = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        m_Context.WorkItems.AddRange(item1, item2);
        await m_Context.SaveChangesAsync();

        BoardViewDto result = await m_BoardService.GetBoardViewAsync(1);

        Assert.Equal(1, result.Columns[0].ItemCount); // TODO column
        Assert.Equal(1, result.Columns[1].ItemCount); // IN PROGRESS column
        Assert.Equal(0, result.Columns[2].ItemCount); // DONE column
    }

    [Fact]
    public async Task GetBoardViewAsync_FiltersByEpicIds()
    {
        // Create board
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        await m_BoardService.CreateBoardAsync(1, createRequest, 1);

        // Create epic
        Epic epic = new Epic
        {
            ProjectId = 1,
            Key = "TEST-EPIC-1",
            Summary = "Test Epic",
            StatusId = 1,
            Priority = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        m_Context.Epics.Add(epic);
        await m_Context.SaveChangesAsync();

        // Create work items
        WorkItem item1 = new WorkItem
        {
            ProjectId = 1,
            EpicId = epic.Id,
            Key = "TEST-1",
            Type = WorkItemType.Task,
            Summary = "Task with Epic",
            StatusId = 1,
            Priority = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        WorkItem item2 = new WorkItem
        {
            ProjectId = 1,
            Key = "TEST-2",
            Type = WorkItemType.Task,
            Summary = "Task without Epic",
            StatusId = 1,
            Priority = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        m_Context.WorkItems.AddRange(item1, item2);
        await m_Context.SaveChangesAsync();

        BoardViewDto result = await m_BoardService.GetBoardViewAsync(1, epicIds: new[] { epic.Id });

        Assert.Equal(1, result.TotalItems);
        Assert.Equal("TEST-1", result.Columns[0].WorkItems[0].Key);
    }

    [Fact]
    public async Task GetBoardViewAsync_FiltersByAssigneeIds()
    {
        // Create board
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        await m_BoardService.CreateBoardAsync(1, createRequest, 1);

        // Create work items
        WorkItem item1 = new WorkItem
        {
            ProjectId = 1,
            AssigneeId = 1,
            Key = "TEST-1",
            Type = WorkItemType.Task,
            Summary = "Task assigned to user 1",
            StatusId = 1,
            Priority = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        WorkItem item2 = new WorkItem
        {
            ProjectId = 1,
            AssigneeId = 2,
            Key = "TEST-2",
            Type = WorkItemType.Task,
            Summary = "Task assigned to user 2",
            StatusId = 1,
            Priority = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        m_Context.WorkItems.AddRange(item1, item2);
        await m_Context.SaveChangesAsync();

        BoardViewDto result = await m_BoardService.GetBoardViewAsync(1, assigneeIds: new[] { 1 });

        Assert.Equal(1, result.TotalItems);
        Assert.Equal("TEST-1", result.Columns[0].WorkItems[0].Key);
    }

    [Fact]
    public async Task GetBoardViewAsync_FiltersBySearchText()
    {
        // Create board
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        await m_BoardService.CreateBoardAsync(1, createRequest, 1);

        // Create work items
        WorkItem item1 = new WorkItem
        {
            ProjectId = 1,
            Key = "TEST-1",
            Type = WorkItemType.Task,
            Summary = "Fix login bug",
            StatusId = 1,
            Priority = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        WorkItem item2 = new WorkItem
        {
            ProjectId = 1,
            Key = "TEST-2",
            Type = WorkItemType.Task,
            Summary = "Add logout feature",
            StatusId = 1,
            Priority = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        m_Context.WorkItems.AddRange(item1, item2);
        await m_Context.SaveChangesAsync();

        BoardViewDto result = await m_BoardService.GetBoardViewAsync(1, searchText: "login");

        Assert.Equal(1, result.TotalItems);
        Assert.Equal("TEST-1", result.Columns[0].WorkItems[0].Key);
    }

    [Fact]
    public async Task GetBoardViewAsync_SearchTextMatchesKey()
    {
        // Create board
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        await m_BoardService.CreateBoardAsync(1, createRequest, 1);

        // Create work items
        WorkItem item1 = new WorkItem
        {
            ProjectId = 1,
            Key = "SPECIAL-123",
            Type = WorkItemType.Task,
            Summary = "Regular task",
            StatusId = 1,
            Priority = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        WorkItem item2 = new WorkItem
        {
            ProjectId = 1,
            Key = "TEST-456",
            Type = WorkItemType.Task,
            Summary = "Another task",
            StatusId = 1,
            Priority = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        m_Context.WorkItems.AddRange(item1, item2);
        await m_Context.SaveChangesAsync();

        BoardViewDto result = await m_BoardService.GetBoardViewAsync(1, searchText: "SPECIAL");

        Assert.Equal(1, result.TotalItems);
        Assert.Equal("SPECIAL-123", result.Columns[0].WorkItems[0].Key);
    }

    [Fact]
    public async Task GetBoardViewAsync_NonExistentBoard_ThrowsNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            m_BoardService.GetBoardViewAsync(1));
    }

    [Fact]
    public async Task GetBoardViewAsync_IncludesAssigneeName()
    {
        // Create board
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        await m_BoardService.CreateBoardAsync(1, createRequest, 1);

        // Create work item with assignee
        WorkItem item = new WorkItem
        {
            ProjectId = 1,
            AssigneeId = 1,
            Key = "TEST-1",
            Type = WorkItemType.Task,
            Summary = "Assigned task",
            StatusId = 1,
            Priority = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        m_Context.WorkItems.Add(item);
        await m_Context.SaveChangesAsync();

        BoardViewDto result = await m_BoardService.GetBoardViewAsync(1);

        Assert.Equal("Test Owner", result.Columns[0].WorkItems[0].AssigneeName);
    }

    [Fact]
    public async Task GetBoardViewAsync_UnassignedWorkItem_HasNullAssigneeName()
    {
        // Create board
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        await m_BoardService.CreateBoardAsync(1, createRequest, 1);

        // Create work item without assignee
        WorkItem item = new WorkItem
        {
            ProjectId = 1,
            Key = "TEST-1",
            Type = WorkItemType.Task,
            Summary = "Unassigned task",
            StatusId = 1,
            Priority = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        m_Context.WorkItems.Add(item);
        await m_Context.SaveChangesAsync();

        BoardViewDto result = await m_BoardService.GetBoardViewAsync(1);

        Assert.Null(result.Columns[0].WorkItems[0].AssigneeName);
    }

    [Fact]
    public async Task GetBoardViewAsync_CombinesAllFilters()
    {
        // Create board
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        await m_BoardService.CreateBoardAsync(1, createRequest, 1);

        // Create epic
        Epic epic = new Epic
        {
            ProjectId = 1,
            Key = "TEST-EPIC-1",
            Summary = "Test Epic",
            StatusId = 1,
            Priority = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        m_Context.Epics.Add(epic);
        await m_Context.SaveChangesAsync();

        // Create work items
        WorkItem item1 = new WorkItem
        {
            ProjectId = 1,
            EpicId = epic.Id,
            AssigneeId = 1,
            Key = "TEST-1",
            Type = WorkItemType.Task,
            Summary = "login feature",
            StatusId = 1,
            Priority = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        WorkItem item2 = new WorkItem
        {
            ProjectId = 1,
            EpicId = epic.Id,
            AssigneeId = 2,
            Key = "TEST-2",
            Type = WorkItemType.Task,
            Summary = "login bug",
            StatusId = 1,
            Priority = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        m_Context.WorkItems.AddRange(item1, item2);
        await m_Context.SaveChangesAsync();

        BoardViewDto result = await m_BoardService.GetBoardViewAsync(
            1,
            epicIds: new[] { epic.Id },
            assigneeIds: new[] { 1 },
            searchText: "login");

        Assert.Equal(1, result.TotalItems);
        Assert.Equal("TEST-1", result.Columns[0].WorkItems[0].Key);
    }

    [Fact]
    public async Task GetBoardViewAsync_EmptyBoard_ReturnsZeroItems()
    {
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        await m_BoardService.CreateBoardAsync(1, createRequest, 1);

        BoardViewDto result = await m_BoardService.GetBoardViewAsync(1);

        Assert.Equal(0, result.TotalItems);
        Assert.All(result.Columns, column => Assert.Equal(0, column.ItemCount));
    }

    #endregion

    #region UpdateColumnAsync Tests

    [Fact]
    public async Task UpdateColumnAsync_ValidRequest_UpdatesColumn()
    {
        // Create board
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        BoardDto board = await m_BoardService.CreateBoardAsync(1, createRequest, 1);
        int columnId = board.Columns[0].Id;

        UpdateBoardColumnRequest request = new UpdateBoardColumnRequest
        {
            WIPLimit = 5,
            IsCollapsed = true
        };

        BoardColumnDto result = await m_BoardService.UpdateColumnAsync(1, columnId, request, 1);

        Assert.Equal(5, result.WIPLimit);
        Assert.True(result.IsCollapsed);
    }

    [Fact]
    public async Task UpdateColumnAsync_NullRequest_ThrowsArgumentNullException()
    {
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        BoardDto board = await m_BoardService.CreateBoardAsync(1, createRequest, 1);
        int columnId = board.Columns[0].Id;

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            m_BoardService.UpdateColumnAsync(1, columnId, null!, 1));
    }

    [Fact]
    public async Task UpdateColumnAsync_NonExistentBoard_ThrowsNotFoundException()
    {
        UpdateBoardColumnRequest request = new UpdateBoardColumnRequest
        {
            WIPLimit = 5,
            IsCollapsed = true
        };

        await Assert.ThrowsAsync<NotFoundException>(() =>
            m_BoardService.UpdateColumnAsync(999, 1, request, 1));
    }

    [Fact]
    public async Task UpdateColumnAsync_NonExistentColumn_ThrowsNotFoundException()
    {
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        await m_BoardService.CreateBoardAsync(1, createRequest, 1);

        UpdateBoardColumnRequest request = new UpdateBoardColumnRequest
        {
            WIPLimit = 5,
            IsCollapsed = true
        };

        await Assert.ThrowsAsync<NotFoundException>(() =>
            m_BoardService.UpdateColumnAsync(1, 999, request, 1));
    }

    [Fact]
    public async Task UpdateColumnAsync_NotProjectOwner_ThrowsForbiddenException()
    {
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        BoardDto board = await m_BoardService.CreateBoardAsync(1, createRequest, 1);
        int columnId = board.Columns[0].Id;

        UpdateBoardColumnRequest request = new UpdateBoardColumnRequest
        {
            WIPLimit = 5,
            IsCollapsed = true
        };

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            m_BoardService.UpdateColumnAsync(1, columnId, request, 2)); // User 2 is not owner
    }

    [Fact]
    public async Task UpdateColumnAsync_CanSetWIPLimitToNull()
    {
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        BoardDto board = await m_BoardService.CreateBoardAsync(1, createRequest, 1);
        int columnId = board.Columns[0].Id;

        UpdateBoardColumnRequest request = new UpdateBoardColumnRequest
        {
            WIPLimit = null,
            IsCollapsed = false
        };

        BoardColumnDto result = await m_BoardService.UpdateColumnAsync(1, columnId, request, 1);

        Assert.Null(result.WIPLimit);
    }

    [Fact]
    public async Task UpdateColumnAsync_DoesNotChangeOtherColumns()
    {
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        BoardDto board = await m_BoardService.CreateBoardAsync(1, createRequest, 1);
        int columnId = board.Columns[0].Id;

        UpdateBoardColumnRequest request = new UpdateBoardColumnRequest
        {
            WIPLimit = 5,
            IsCollapsed = true
        };

        await m_BoardService.UpdateColumnAsync(1, columnId, request, 1);

        BoardDto updatedBoard = (await m_BoardService.GetBoardByProjectIdAsync(1))!;
        Assert.Null(updatedBoard.Columns[1].WIPLimit);
        Assert.False(updatedBoard.Columns[1].IsCollapsed);
    }

    [Fact]
    public async Task UpdateColumnAsync_PreservesStatusInformation()
    {
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        BoardDto board = await m_BoardService.CreateBoardAsync(1, createRequest, 1);
        int columnId = board.Columns[0].Id;
        string originalStatusName = board.Columns[0].StatusName;

        UpdateBoardColumnRequest request = new UpdateBoardColumnRequest
        {
            WIPLimit = 5,
            IsCollapsed = true
        };

        BoardColumnDto result = await m_BoardService.UpdateColumnAsync(1, columnId, request, 1);

        Assert.Equal(originalStatusName, result.StatusName);
    }

    [Fact]
    public async Task UpdateColumnAsync_PreservesOrderIndex()
    {
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        BoardDto board = await m_BoardService.CreateBoardAsync(1, createRequest, 1);
        int columnId = board.Columns[1].Id;
        int originalOrderIndex = board.Columns[1].OrderIndex;

        UpdateBoardColumnRequest request = new UpdateBoardColumnRequest
        {
            WIPLimit = 5,
            IsCollapsed = true
        };

        BoardColumnDto result = await m_BoardService.UpdateColumnAsync(1, columnId, request, 1);

        Assert.Equal(originalOrderIndex, result.OrderIndex);
    }

    #endregion

    #region ReorderColumnsAsync Tests

    [Fact]
    public async Task ReorderColumnsAsync_ValidRequest_ReordersColumns()
    {
        // Create board
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        BoardDto board = await m_BoardService.CreateBoardAsync(1, createRequest, 1);

        // Reverse the order
        ReorderColumnsRequest request = new ReorderColumnsRequest
        {
            ColumnIds = new[] { board.Columns[2].Id, board.Columns[1].Id, board.Columns[0].Id }
        };

        await m_BoardService.ReorderColumnsAsync(1, request, 1);

        BoardDto result = (await m_BoardService.GetBoardByProjectIdAsync(1))!;
        Assert.Equal("DONE", result.Columns[0].StatusName);
        Assert.Equal("IN PROGRESS", result.Columns[1].StatusName);
        Assert.Equal("TODO", result.Columns[2].StatusName);
    }

    [Fact]
    public async Task ReorderColumnsAsync_NullRequest_ThrowsArgumentNullException()
    {
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        await m_BoardService.CreateBoardAsync(1, createRequest, 1);

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            m_BoardService.ReorderColumnsAsync(1, null!, 1));
    }

    [Fact]
    public async Task ReorderColumnsAsync_NonExistentBoard_ThrowsNotFoundException()
    {
        ReorderColumnsRequest request = new ReorderColumnsRequest
        {
            ColumnIds = new[] { 1, 2, 3 }
        };

        await Assert.ThrowsAsync<NotFoundException>(() =>
            m_BoardService.ReorderColumnsAsync(999, request, 1));
    }

    [Fact]
    public async Task ReorderColumnsAsync_NotProjectOwner_ThrowsForbiddenException()
    {
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        BoardDto board = await m_BoardService.CreateBoardAsync(1, createRequest, 1);

        ReorderColumnsRequest request = new ReorderColumnsRequest
        {
            ColumnIds = new[] { board.Columns[0].Id, board.Columns[1].Id, board.Columns[2].Id }
        };

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            m_BoardService.ReorderColumnsAsync(1, request, 2)); // User 2 is not owner
    }

    [Fact]
    public async Task ReorderColumnsAsync_WrongColumnCount_ThrowsBadRequestException()
    {
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        BoardDto board = await m_BoardService.CreateBoardAsync(1, createRequest, 1);

        ReorderColumnsRequest request = new ReorderColumnsRequest
        {
            ColumnIds = new[] { board.Columns[0].Id, board.Columns[1].Id }
        };

        await Assert.ThrowsAsync<BadRequestException>(() =>
            m_BoardService.ReorderColumnsAsync(1, request, 1));
    }

    [Fact]
    public async Task ReorderColumnsAsync_InvalidColumnId_ThrowsBadRequestException()
    {
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        BoardDto board = await m_BoardService.CreateBoardAsync(1, createRequest, 1);

        ReorderColumnsRequest request = new ReorderColumnsRequest
        {
            ColumnIds = new[] { board.Columns[0].Id, board.Columns[1].Id, 999 }
        };

        await Assert.ThrowsAsync<BadRequestException>(() =>
            m_BoardService.ReorderColumnsAsync(1, request, 1));
    }

    [Fact]
    public async Task ReorderColumnsAsync_SetsCorrectOrderIndexes()
    {
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        BoardDto board = await m_BoardService.CreateBoardAsync(1, createRequest, 1);

        ReorderColumnsRequest request = new ReorderColumnsRequest
        {
            ColumnIds = new[] { board.Columns[2].Id, board.Columns[0].Id, board.Columns[1].Id }
        };

        await m_BoardService.ReorderColumnsAsync(1, request, 1);

        BoardDto result = (await m_BoardService.GetBoardByProjectIdAsync(1))!;
        Assert.Equal(0, result.Columns[0].OrderIndex);
        Assert.Equal(1, result.Columns[1].OrderIndex);
        Assert.Equal(2, result.Columns[2].OrderIndex);
    }

    [Fact]
    public async Task ReorderColumnsAsync_PreservesColumnProperties()
    {
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        BoardDto board = await m_BoardService.CreateBoardAsync(1, createRequest, 1);

        // Update a column first
        UpdateBoardColumnRequest updateRequest = new UpdateBoardColumnRequest
        {
            WIPLimit = 5,
            IsCollapsed = true
        };
        await m_BoardService.UpdateColumnAsync(1, board.Columns[0].Id, updateRequest, 1);

        // Reorder columns
        ReorderColumnsRequest request = new ReorderColumnsRequest
        {
            ColumnIds = new[] { board.Columns[2].Id, board.Columns[0].Id, board.Columns[1].Id }
        };
        await m_BoardService.ReorderColumnsAsync(1, request, 1);

        // Verify properties preserved
        BoardDto result = (await m_BoardService.GetBoardByProjectIdAsync(1))!;
        BoardColumnDto movedColumn = result.Columns.First(c => c.Id == board.Columns[0].Id);
        Assert.Equal(5, movedColumn.WIPLimit);
        Assert.True(movedColumn.IsCollapsed);
    }

    [Fact]
    public async Task ReorderColumnsAsync_AllowsSameOrder()
    {
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        BoardDto board = await m_BoardService.CreateBoardAsync(1, createRequest, 1);

        // Same order
        ReorderColumnsRequest request = new ReorderColumnsRequest
        {
            ColumnIds = new[] { board.Columns[0].Id, board.Columns[1].Id, board.Columns[2].Id }
        };

        await m_BoardService.ReorderColumnsAsync(1, request, 1);

        BoardDto result = (await m_BoardService.GetBoardByProjectIdAsync(1))!;
        Assert.Equal("TODO", result.Columns[0].StatusName);
        Assert.Equal("IN PROGRESS", result.Columns[1].StatusName);
        Assert.Equal("DONE", result.Columns[2].StatusName);
    }

    [Fact]
    public async Task ReorderColumnsAsync_CompletesSuccessfully()
    {
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        BoardDto board = await m_BoardService.CreateBoardAsync(1, createRequest, 1);

        ReorderColumnsRequest request = new ReorderColumnsRequest
        {
            ColumnIds = new[] { board.Columns[1].Id, board.Columns[0].Id, board.Columns[2].Id }
        };

        // Should not throw
        await m_BoardService.ReorderColumnsAsync(1, request, 1);
    }

    #endregion

    #region MoveWorkItemAsync Tests

    [Fact]
    public async Task MoveWorkItemAsync_ValidTransition_MovesWorkItem()
    {
        // Create board
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        await m_BoardService.CreateBoardAsync(1, createRequest, 1);

        // Create work item
        WorkItem item = new WorkItem
        {
            ProjectId = 1,
            Key = "TEST-1",
            Type = WorkItemType.Task,
            Summary = "Test Task",
            StatusId = 1,
            Priority = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        m_Context.WorkItems.Add(item);
        await m_Context.SaveChangesAsync();

        await m_BoardService.MoveWorkItemAsync(1, item.Id, 2, 1);

        WorkItem? updated = await m_Context.WorkItems.FindAsync(item.Id);
        Assert.Equal(2, updated!.StatusId);
    }

    [Fact]
    public async Task MoveWorkItemAsync_NonExistentBoard_ThrowsNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            m_BoardService.MoveWorkItemAsync(999, 1, 2, 1));
    }

    [Fact]
    public async Task MoveWorkItemAsync_StatusNotOnBoard_ThrowsBadRequestException()
    {
        // Create board
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        await m_BoardService.CreateBoardAsync(1, createRequest, 1);

        // Create work item
        WorkItem item = new WorkItem
        {
            ProjectId = 1,
            Key = "TEST-1",
            Type = WorkItemType.Task,
            Summary = "Test Task",
            StatusId = 1,
            Priority = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        m_Context.WorkItems.Add(item);
        await m_Context.SaveChangesAsync();

        await Assert.ThrowsAsync<BadRequestException>(() =>
            m_BoardService.MoveWorkItemAsync(1, item.Id, 999, 1));
    }

    [Fact]
    public async Task MoveWorkItemAsync_NonExistentWorkItem_ThrowsNotFoundException()
    {
        // Create board
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        await m_BoardService.CreateBoardAsync(1, createRequest, 1);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            m_BoardService.MoveWorkItemAsync(1, 999, 2, 1));
    }

    [Fact]
    public async Task MoveWorkItemAsync_InvalidTransition_ThrowsBadRequestException()
    {
        // Create board
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        await m_BoardService.CreateBoardAsync(1, createRequest, 1);

        // Create work item in DONE status
        WorkItem item = new WorkItem
        {
            ProjectId = 1,
            Key = "TEST-1",
            Type = WorkItemType.Task,
            Summary = "Test Task",
            StatusId = 3, // DONE
            Priority = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        m_Context.WorkItems.Add(item);
        await m_Context.SaveChangesAsync();

        // Try to move from DONE to TODO - this transition is not allowed
        await Assert.ThrowsAsync<BadRequestException>(() =>
            m_BoardService.MoveWorkItemAsync(1, item.Id, 1, 1));
    }

    [Fact]
    public async Task MoveWorkItemAsync_AlreadyInTargetStatus_NoOp()
    {
        // Create board
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        await m_BoardService.CreateBoardAsync(1, createRequest, 1);

        // Create work item
        WorkItem item = new WorkItem
        {
            ProjectId = 1,
            Key = "TEST-1",
            Type = WorkItemType.Task,
            Summary = "Test Task",
            StatusId = 1,
            Priority = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow.AddMinutes(-10)
        };
        m_Context.WorkItems.Add(item);
        await m_Context.SaveChangesAsync();

        DateTime beforeUpdate = item.UpdatedAt;

        await m_BoardService.MoveWorkItemAsync(1, item.Id, 1, 1);

        WorkItem? updated = await m_Context.WorkItems.FindAsync(item.Id);
        Assert.Equal(1, updated!.StatusId);
        // UpdatedAt should not change for no-op
        Assert.Equal(beforeUpdate, updated.UpdatedAt);
    }

    [Fact]
    public async Task MoveWorkItemAsync_UpdatesUpdatedAt()
    {
        // Create board
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        await m_BoardService.CreateBoardAsync(1, createRequest, 1);

        // Create work item
        WorkItem item = new WorkItem
        {
            ProjectId = 1,
            Key = "TEST-1",
            Type = WorkItemType.Task,
            Summary = "Test Task",
            StatusId = 1,
            Priority = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow.AddMinutes(-10)
        };
        m_Context.WorkItems.Add(item);
        await m_Context.SaveChangesAsync();

        DateTime beforeMove = DateTime.UtcNow;
        await m_BoardService.MoveWorkItemAsync(1, item.Id, 2, 1);
        DateTime afterMove = DateTime.UtcNow;

        WorkItem? updated = await m_Context.WorkItems.FindAsync(item.Id);
        Assert.InRange(updated!.UpdatedAt, beforeMove, afterMove);
    }

    [Fact]
    public async Task MoveWorkItemAsync_ValidatesTransition()
    {
        // Create board
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        await m_BoardService.CreateBoardAsync(1, createRequest, 1);

        // Create work item
        WorkItem item = new WorkItem
        {
            ProjectId = 1,
            Key = "TEST-1",
            Type = WorkItemType.Task,
            Summary = "Test Task",
            StatusId = 1,
            Priority = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        m_Context.WorkItems.Add(item);
        await m_Context.SaveChangesAsync();

        // Should successfully move due to valid transition
        await m_BoardService.MoveWorkItemAsync(1, item.Id, 2, 1);

        WorkItem? updated = await m_Context.WorkItems.FindAsync(item.Id);
        Assert.Equal(2, updated!.StatusId);
    }

    [Fact]
    public async Task MoveWorkItemAsync_CompletesSuccessfully()
    {
        // Create board
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        await m_BoardService.CreateBoardAsync(1, createRequest, 1);

        // Create work item
        WorkItem item = new WorkItem
        {
            ProjectId = 1,
            Key = "TEST-1",
            Type = WorkItemType.Task,
            Summary = "Test Task",
            StatusId = 1,
            Priority = 3,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        m_Context.WorkItems.Add(item);
        await m_Context.SaveChangesAsync();

        // Should not throw
        await m_BoardService.MoveWorkItemAsync(1, item.Id, 2, 1);
    }

    #endregion
}
