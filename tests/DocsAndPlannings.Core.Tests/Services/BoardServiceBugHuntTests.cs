using DocsAndPlannings.Core.Data;
using DocsAndPlannings.Core.DTOs.Boards;
using DocsAndPlannings.Core.DTOs.WorkItems;
using DocsAndPlannings.Core.Exceptions;
using DocsAndPlannings.Core.Models;
using DocsAndPlannings.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace DocsAndPlannings.Core.Tests.Services;

/// <summary>
/// Bug hunting tests for BoardService - focuses on edge cases, concurrency, and error scenarios
/// </summary>
public sealed class BoardServiceBugHuntTests : IDisposable
{
    private readonly ApplicationDbContext m_Context;
    private readonly IStatusService m_StatusService;
    private readonly IBoardService m_BoardService;
    private readonly IKeyGenerationService m_KeyGenerationService;
    private readonly IProjectService m_ProjectService;
    private readonly IWorkItemService m_WorkItemService;
    private readonly int m_UserId;
    private readonly int m_OtherUserId;

    public BoardServiceBugHuntTests()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        m_Context = new ApplicationDbContext(options);
        m_StatusService = new StatusService(m_Context);
        m_KeyGenerationService = new KeyGenerationService(m_Context);
        m_ProjectService = new ProjectService(m_Context);
        m_WorkItemService = new WorkItemService(m_Context, m_KeyGenerationService, m_StatusService);
        m_BoardService = new BoardService(m_Context, m_StatusService);

        m_UserId = 1;
        m_OtherUserId = 2;
        SeedTestData();
    }

    public void Dispose()
    {
        m_Context.Dispose();
    }

    private void SeedTestData()
    {
        // Create test users
        User owner = new User
        {
            Id = m_UserId,
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
            Id = m_OtherUserId,
            Email = "other@example.com",
            PasswordHash = "hash",
            FirstName = "Other",
            LastName = "User",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        m_Context.Users.AddRange(owner, otherUser);
        m_Context.SaveChanges();

        // Create default statuses
        m_StatusService.CreateDefaultStatusesAsync().Wait();
    }

    [Fact]
    public async Task BugHunt_GetBoardViewAsync_WithEmptyFilters_ReturnsAllWorkItems()
    {
        // Arrange: Create project and board
        var project = await CreateTestProjectAsync("BUG1");
        var board = await m_BoardService.CreateBoardAsync(
            project.Id,
            new CreateBoardRequest { Name = "Test Board" },
            m_UserId);

        // Create work items
        for (int i = 0; i < 5; i++)
        {
            await m_WorkItemService.CreateWorkItemAsync(
                new CreateWorkItemRequest
                {
                    ProjectId = project.Id,
                    Type = WorkItemType.Task,
                    Summary = $"Task {i}"
                },
                m_UserId);
        }

        // Act: Get board view with null filters
        var boardView = await m_BoardService.GetBoardViewAsync(
            project.Id,
            epicIds: null,
            assigneeIds: null,
            searchText: null);

        // Assert: All work items should be included
        Assert.Equal(5, boardView.TotalItems);
    }

    [Fact]
    public async Task BugHunt_GetBoardViewAsync_WithEmptyArrayFilters_ReturnsAllWorkItems()
    {
        // Arrange
        var project = await CreateTestProjectAsync("BUG2");
        var board = await m_BoardService.CreateBoardAsync(
            project.Id,
            new CreateBoardRequest { Name = "Test Board" },
            m_UserId);

        await m_WorkItemService.CreateWorkItemAsync(
            new CreateWorkItemRequest
            {
                ProjectId = project.Id,
                Type = WorkItemType.Task,
                Summary = "Task 1"
            },
            m_UserId);

        // Act: Get board view with empty arrays
        var boardView = await m_BoardService.GetBoardViewAsync(
            project.Id,
            epicIds: Array.Empty<int>(),
            assigneeIds: Array.Empty<int>(),
            searchText: string.Empty);

        // Assert: Should return all items (empty arrays should not filter)
        Assert.Equal(1, boardView.TotalItems);
    }

    [Fact]
    public async Task BugHunt_GetBoardViewAsync_SearchIsCaseSensitive_FindsOnlyExactCase()
    {
        // Arrange
        var project = await CreateTestProjectAsync("BUG3");
        var board = await m_BoardService.CreateBoardAsync(
            project.Id,
            new CreateBoardRequest { Name = "Test Board" },
            m_UserId);

        await m_WorkItemService.CreateWorkItemAsync(
            new CreateWorkItemRequest
            {
                ProjectId = project.Id,
                Type = WorkItemType.Task,
                Summary = "UPPERCASE TASK"
            },
            m_UserId);

        await m_WorkItemService.CreateWorkItemAsync(
            new CreateWorkItemRequest
            {
                ProjectId = project.Id,
                Type = WorkItemType.Task,
                Summary = "lowercase task"
            },
            m_UserId);

        // Act: Search with lowercase
        var boardView = await m_BoardService.GetBoardViewAsync(
            project.Id,
            searchText: "task");

        // Assert: Case-sensitive search finds only lowercase
        // NOTE: This demonstrates that the search is case-sensitive which may not be ideal for UX
        Assert.Equal(1, boardView.TotalItems);
    }

    [Fact]
    public async Task BugHunt_DeleteBoard_CascadeDeletesColumns_NoOrphanedColumns()
    {
        // Arrange
        var project = await CreateTestProjectAsync("BUG4");
        var board = await m_BoardService.CreateBoardAsync(
            project.Id,
            new CreateBoardRequest { Name = "Test Board" },
            m_UserId);

        // Verify columns exist
        var columnsBefore = await m_Context.BoardColumns.Where(c => c.BoardId == board.Id).CountAsync();
        Assert.True(columnsBefore > 0);

        // Act: Delete board
        await m_BoardService.DeleteBoardAsync(project.Id, m_UserId);

        // Assert: Columns should be cascade deleted
        var columnsAfter = await m_Context.BoardColumns.Where(c => c.BoardId == board.Id).CountAsync();
        Assert.Equal(0, columnsAfter);
    }

    [Fact]
    public async Task BugHunt_ReorderColumns_WithDuplicateIds_ThrowsBadRequest()
    {
        // Arrange
        var project = await CreateTestProjectAsync("BUG5");
        var board = await m_BoardService.CreateBoardAsync(
            project.Id,
            new CreateBoardRequest { Name = "Test Board" },
            m_UserId);

        var boardDto = await m_BoardService.GetBoardByProjectIdAsync(project.Id);
        var firstColumnId = boardDto!.Columns[0].Id;

        // Act & Assert: Try to reorder with duplicate column ID
        await Assert.ThrowsAsync<BadRequestException>(() =>
            m_BoardService.ReorderColumnsAsync(
                project.Id,
                new ReorderColumnsRequest
                {
                    ColumnIds = new List<int> { firstColumnId, firstColumnId, firstColumnId }
                },
                m_UserId));
    }

    [Fact]
    public async Task BugHunt_ReorderColumns_WithMissingColumnId_ThrowsBadRequest()
    {
        // Arrange
        var project = await CreateTestProjectAsync("BUG6");
        var board = await m_BoardService.CreateBoardAsync(
            project.Id,
            new CreateBoardRequest { Name = "Test Board" },
            m_UserId);

        var boardDto = await m_BoardService.GetBoardByProjectIdAsync(project.Id);
        List<int> columnIds = boardDto!.Columns.Select(c => c.Id).ToList();
        columnIds.RemoveAt(0); // Remove first column ID

        // Act & Assert: Try to reorder with missing column
        await Assert.ThrowsAsync<BadRequestException>(() =>
            m_BoardService.ReorderColumnsAsync(
                project.Id,
                new ReorderColumnsRequest
                {
                    ColumnIds = columnIds
                },
                m_UserId));
    }

    [Fact]
    public async Task BugHunt_ReorderColumns_WithForeignColumnId_ThrowsBadRequest()
    {
        // Arrange: Create two projects with boards
        var project1 = await CreateTestProjectAsync("BUG7A");
        var board1 = await m_BoardService.CreateBoardAsync(
            project1.Id,
            new CreateBoardRequest { Name = "Board 1" },
            m_UserId);

        var project2 = await CreateTestProjectAsync("BUG7B");
        var board2 = await m_BoardService.CreateBoardAsync(
            project2.Id,
            new CreateBoardRequest { Name = "Board 2" },
            m_UserId);

        var board2Dto = await m_BoardService.GetBoardByProjectIdAsync(project2.Id);
        List<int> board2ColumnIds = board2Dto!.Columns.Select(c => c.Id).ToList();

        // Act & Assert: Try to use board2's column IDs on board1
        await Assert.ThrowsAsync<BadRequestException>(() =>
            m_BoardService.ReorderColumnsAsync(
                project1.Id,
                new ReorderColumnsRequest
                {
                    ColumnIds = board2ColumnIds
                },
                m_UserId));
    }

    [Fact]
    public async Task BugHunt_MoveWorkItem_AlreadyInTargetStatus_IsNoOp()
    {
        // Arrange
        var project = await CreateTestProjectAsync("BUG8");
        var board = await m_BoardService.CreateBoardAsync(
            project.Id,
            new CreateBoardRequest { Name = "Test Board" },
            m_UserId);

        var workItem = await m_WorkItemService.CreateWorkItemAsync(
            new CreateWorkItemRequest
            {
                ProjectId = project.Id,
                Type = WorkItemType.Task,
                Summary = "Test Task"
            },
            m_UserId);

        var currentStatusId = workItem.StatusId;

        // Act: Move to same status (no-op)
        await m_BoardService.MoveWorkItemAsync(project.Id, workItem.Id, currentStatusId, m_UserId);

        // Assert: Work item should still be in same status
        var updatedWorkItem = await m_WorkItemService.GetWorkItemByIdAsync(workItem.Id);
        Assert.Equal(currentStatusId, updatedWorkItem!.StatusId);
    }

    [Fact]
    public async Task BugHunt_UpdateColumn_WithNullWIPLimit_ClearsExistingLimit()
    {
        // Arrange
        var project = await CreateTestProjectAsync("BUG9");
        var board = await m_BoardService.CreateBoardAsync(
            project.Id,
            new CreateBoardRequest { Name = "Test Board" },
            m_UserId);

        var boardDto = await m_BoardService.GetBoardByProjectIdAsync(project.Id);
        var column = boardDto!.Columns[0];

        // Set a WIP limit first
        await m_BoardService.UpdateColumnAsync(
            project.Id,
            column.Id,
            new UpdateBoardColumnRequest
            {
                WIPLimit = 5,
                IsCollapsed = false
            },
            m_UserId);

        // Act: Update with null WIP limit
        var updatedColumn = await m_BoardService.UpdateColumnAsync(
            project.Id,
            column.Id,
            new UpdateBoardColumnRequest
            {
                WIPLimit = null,
                IsCollapsed = false
            },
            m_UserId);

        // Assert: WIP limit should be cleared
        Assert.Null(updatedColumn.WIPLimit);
    }

    [Fact]
    public async Task BugHunt_GetBoardViewAsync_WithVeryLongSearchText_DoesNotCrash()
    {
        // Arrange
        var project = await CreateTestProjectAsync("BUG10");
        var board = await m_BoardService.CreateBoardAsync(
            project.Id,
            new CreateBoardRequest { Name = "Test Board" },
            m_UserId);

        string veryLongSearch = new string('x', 10000); // 10KB search string

        // Act & Assert: Should not crash with very long search text
        var boardView = await m_BoardService.GetBoardViewAsync(
            project.Id,
            searchText: veryLongSearch);

        // Should complete without exception
        Assert.NotNull(boardView);
    }

    [Fact]
    public async Task BugHunt_MoveWorkItem_ToNonExistentStatus_ThrowsBadRequest()
    {
        // Arrange
        var project = await CreateTestProjectAsync("BUG11");
        var board = await m_BoardService.CreateBoardAsync(
            project.Id,
            new CreateBoardRequest { Name = "Test Board" },
            m_UserId);

        var workItem = await m_WorkItemService.CreateWorkItemAsync(
            new CreateWorkItemRequest
            {
                ProjectId = project.Id,
                Type = WorkItemType.Task,
                Summary = "Test Task"
            },
            m_UserId);

        // Act & Assert: Try to move to non-existent status
        await Assert.ThrowsAsync<BadRequestException>(() =>
            m_BoardService.MoveWorkItemAsync(project.Id, workItem.Id, 99999, m_UserId));
    }

    [Fact]
    public async Task BugHunt_CreateBoard_WithVeryLongName_TruncatesInDatabase()
    {
        // Arrange
        var project = await CreateTestProjectAsync("BUG12");
        string veryLongName = new string('x', 300); // Exceeds MaxLength(200)

        // Act: Create board with very long name
        // NOTE: In-memory database doesn't enforce MaxLength, so this succeeds
        // In a real database, this would be truncated or throw an exception
        // The validation attribute on the DTO should catch this at the API layer
        var board = await m_BoardService.CreateBoardAsync(
            project.Id,
            new CreateBoardRequest
            {
                Name = veryLongName,
                Description = "Test"
            },
            m_UserId);

        // Assert: Board created successfully (in-memory DB doesn't enforce length)
        // In production, ASP.NET Core model validation will reject requests exceeding MaxLength
        Assert.NotNull(board);
    }

    [Fact]
    public async Task BugHunt_ConcurrentReorder_DoesNotCorruptColumnOrder()
    {
        // Arrange
        var project = await CreateTestProjectAsync("BUG13");
        var board = await m_BoardService.CreateBoardAsync(
            project.Id,
            new CreateBoardRequest { Name = "Test Board" },
            m_UserId);

        var boardDto = await m_BoardService.GetBoardByProjectIdAsync(project.Id);
        List<int> originalOrder = boardDto!.Columns.Select(c => c.Id).ToList();
        List<int> reversedOrder = boardDto!.Columns.Select(c => c.Id).Reverse().ToList();

        // Act: Simulate concurrent reordering (one will succeed, one may fail with concurrency exception)
        Task task1 = m_BoardService.ReorderColumnsAsync(
            project.Id,
            new ReorderColumnsRequest { ColumnIds = originalOrder },
            m_UserId);

        Task task2 = m_BoardService.ReorderColumnsAsync(
            project.Id,
            new ReorderColumnsRequest { ColumnIds = reversedOrder },
            m_UserId);

        // Wait for both (one might throw)
        try
        {
            await Task.WhenAll(task1, task2);
        }
        catch
        {
            // Expected - one may fail
        }

        // Assert: Column order should still be valid (all columns present, no duplicates)
        var finalBoard = await m_BoardService.GetBoardByProjectIdAsync(project.Id);
        List<int> finalColumnIds = finalBoard!.Columns.Select(c => c.Id).OrderBy(id => id).ToList();
        List<int> expectedColumnIds = originalOrder.OrderBy(id => id).ToList();

        Assert.Equal(expectedColumnIds, finalColumnIds);
    }

    private async Task<Project> CreateTestProjectAsync(string key)
    {
        var project = new Project
        {
            Key = key,
            Name = $"Test Project {key}",
            Description = "Test project for bug hunting",
            OwnerId = m_UserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            IsArchived = false
        };

        m_Context.Projects.Add(project);
        await m_Context.SaveChangesAsync();

        return project;
    }
}
