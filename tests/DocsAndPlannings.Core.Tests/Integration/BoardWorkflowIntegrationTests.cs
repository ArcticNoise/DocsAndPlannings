using DocsAndPlannings.Core.Data;
using DocsAndPlannings.Core.DTOs.Boards;
using DocsAndPlannings.Core.DTOs.Epics;
using DocsAndPlannings.Core.DTOs.Projects;
using DocsAndPlannings.Core.DTOs.WorkItems;
using DocsAndPlannings.Core.Models;
using DocsAndPlannings.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace DocsAndPlannings.Core.Tests.Integration;

/// <summary>
/// Integration tests for board workflows that test end-to-end scenarios
/// </summary>
public sealed class BoardWorkflowIntegrationTests : IDisposable
{
    private readonly ApplicationDbContext m_Context;
    private readonly IStatusService m_StatusService;
    private readonly IKeyGenerationService m_KeyGenerationService;
    private readonly IProjectService m_ProjectService;
    private readonly IEpicService m_EpicService;
    private readonly IWorkItemService m_WorkItemService;
    private readonly IBoardService m_BoardService;
    private readonly int m_UserId;

    public BoardWorkflowIntegrationTests()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        m_Context = new ApplicationDbContext(options);
        m_StatusService = new StatusService(m_Context);
        m_KeyGenerationService = new KeyGenerationService(m_Context);
        m_ProjectService = new ProjectService(m_Context);
        m_EpicService = new EpicService(m_Context, m_KeyGenerationService, m_StatusService);
        m_WorkItemService = new WorkItemService(m_Context, m_KeyGenerationService, m_StatusService);
        m_BoardService = new BoardService(m_Context, m_StatusService);

        m_UserId = 1;
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
            Id = m_UserId,
            Email = "owner@example.com",
            PasswordHash = "hash",
            FirstName = "Test",
            LastName = "Owner",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        User assignee = new User
        {
            Id = 2,
            Email = "assignee@example.com",
            PasswordHash = "hash",
            FirstName = "Test",
            LastName = "Assignee",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        // Create test statuses using StatusService
        m_Context.Users.AddRange(owner, assignee);
        m_Context.SaveChanges();

        m_StatusService.CreateDefaultStatusesAsync().Wait();
    }

    [Fact]
    public async Task IntegrationTest_CreateBoard_AddWorkItems_GetBoardView_Success()
    {
        // Arrange: Create project
        CreateProjectRequest projectRequest = new CreateProjectRequest
        {
            Key = "INT",
            Name = "Integration Test Project",
            Description = "Test project for integration tests"
        };

        ProjectDto project = await m_ProjectService.CreateProjectAsync(projectRequest, m_UserId);

        // Arrange: Create board
        CreateBoardRequest boardRequest = new CreateBoardRequest
        {
            Name = "Integration Test Board",
            Description = "Test board for workflow"
        };

        BoardDto board = await m_BoardService.CreateBoardAsync(project.Id, boardRequest, m_UserId);

        // Arrange: Create epic
        CreateEpicRequest epicRequest = new CreateEpicRequest
        {
            ProjectId = project.Id,
            Summary = "Test Epic",
            Description = "Epic for integration tests"
        };

        EpicDto epic = await m_EpicService.CreateEpicAsync(epicRequest, m_UserId);

        // Arrange: Create work items
        CreateWorkItemRequest workItem1 = new CreateWorkItemRequest
        {
            ProjectId = project.Id,
            EpicId = epic.Id,
            Type = WorkItemType.Task,
            Summary = "First task",
            Description = "Description of first task",
            Priority = 2 // High priority (1=Highest, 5=Lowest)
        };

        CreateWorkItemRequest workItem2 = new CreateWorkItemRequest
        {
            ProjectId = project.Id,
            EpicId = epic.Id,
            Type = WorkItemType.Bug,
            Summary = "First bug",
            Description = "Description of first bug",
            Priority = 1, // Critical priority
            AssigneeId = 2
        };

        WorkItemDto task = await m_WorkItemService.CreateWorkItemAsync(workItem1, m_UserId);
        WorkItemDto bug = await m_WorkItemService.CreateWorkItemAsync(workItem2, m_UserId);

        // Act: Get board view
        BoardViewDto boardView = await m_BoardService.GetBoardViewAsync(project.Id);

        // Assert: Board structure
        Assert.NotNull(boardView);
        Assert.Equal(board.Id, boardView.BoardId);
        Assert.Equal(project.Id, boardView.ProjectId);
        Assert.Equal(boardRequest.Name, boardView.Name);

        // Assert: Columns created from default statuses (5 default statuses)
        Assert.Equal(5, boardView.Columns.Count);

        // Assert: Work items appear in correct columns (default status should be "TODO")
        BoardColumnViewDto todoColumn = boardView.Columns.First(c => c.StatusName == "TODO");
        Assert.Equal(2, todoColumn.ItemCount);
        Assert.Equal(2, todoColumn.WorkItems.Count);

        // Assert: Work items have correct data
        WorkItemCardDto? taskCard = todoColumn.WorkItems.FirstOrDefault(w => w.Type == WorkItemType.Task);
        Assert.NotNull(taskCard);
        Assert.Equal("First task", taskCard.Summary);
        Assert.Equal(2, taskCard.Priority);

        WorkItemCardDto? bugCard = todoColumn.WorkItems.FirstOrDefault(w => w.Type == WorkItemType.Bug);
        Assert.NotNull(bugCard);
        Assert.Equal("First bug", bugCard.Summary);
        Assert.Equal(1, bugCard.Priority);
        Assert.Equal("Test Assignee", bugCard.AssigneeName);

        // Assert: Total items count
        Assert.Equal(2, boardView.TotalItems);
    }

    [Fact]
    public async Task IntegrationTest_MoveWorkItemsAcrossStatuses_VerifyBoardState_Success()
    {
        // Arrange: Create project and board
        CreateProjectRequest projectRequest = new CreateProjectRequest
        {
            Key = "MOVE",
            Name = "Move Test Project",
            Description = "Test project for moving work items"
        };

        ProjectDto project = await m_ProjectService.CreateProjectAsync(projectRequest, m_UserId);

        CreateBoardRequest boardRequest = new CreateBoardRequest
        {
            Name = "Move Test Board"
        };

        BoardDto board = await m_BoardService.CreateBoardAsync(project.Id, boardRequest, m_UserId);

        // Arrange: Create work items
        CreateWorkItemRequest workItemRequest = new CreateWorkItemRequest
        {
            ProjectId = project.Id,
            Type = WorkItemType.Task,
            Summary = "Task to move",
            Description = "This task will be moved across statuses"
        };

        WorkItemDto workItem = await m_WorkItemService.CreateWorkItemAsync(workItemRequest, m_UserId);

        // Get status IDs
        Status todoStatus = await m_Context.Statuses.FirstAsync(s => s.Name == "TODO");
        Status inProgressStatus = await m_Context.Statuses.FirstAsync(s => s.Name == "IN PROGRESS");
        Status doneStatus = await m_Context.Statuses.FirstAsync(s => s.Name == "DONE");

        // Act & Assert: Move to In Progress
        await m_BoardService.MoveWorkItemAsync(project.Id, workItem.Id, inProgressStatus.Id, m_UserId);

        BoardViewDto boardView1 = await m_BoardService.GetBoardViewAsync(project.Id);
        BoardColumnViewDto inProgressColumn = boardView1.Columns.First(c => c.StatusId == inProgressStatus.Id);
        Assert.Equal(1, inProgressColumn.ItemCount);
        Assert.Single(inProgressColumn.WorkItems);
        Assert.Equal("Task to move", inProgressColumn.WorkItems[0].Summary);

        // Verify work item is no longer in To Do
        BoardColumnViewDto todoColumn = boardView1.Columns.First(c => c.StatusId == todoStatus.Id);
        Assert.Equal(0, todoColumn.ItemCount);
        Assert.Empty(todoColumn.WorkItems);

        // Act & Assert: Move to Done
        await m_BoardService.MoveWorkItemAsync(project.Id, workItem.Id, doneStatus.Id, m_UserId);

        BoardViewDto boardView2 = await m_BoardService.GetBoardViewAsync(project.Id);
        BoardColumnViewDto doneColumn = boardView2.Columns.First(c => c.StatusId == doneStatus.Id);
        Assert.Equal(1, doneColumn.ItemCount);
        Assert.Single(doneColumn.WorkItems);
        Assert.Equal("Task to move", doneColumn.WorkItems[0].Summary);

        // Verify work item is no longer in In Progress
        BoardColumnViewDto inProgressColumn2 = boardView2.Columns.First(c => c.StatusId == inProgressStatus.Id);
        Assert.Equal(0, inProgressColumn2.ItemCount);
        Assert.Empty(inProgressColumn2.WorkItems);
    }

    [Fact]
    public async Task IntegrationTest_FilterBoardByEpics_VerifyResults_Success()
    {
        // Arrange: Create project and board
        CreateProjectRequest projectRequest = new CreateProjectRequest
        {
            Key = "FILT",
            Name = "Filter Test Project",
            Description = "Test project for filtering"
        };

        ProjectDto project = await m_ProjectService.CreateProjectAsync(projectRequest, m_UserId);

        CreateBoardRequest boardRequest = new CreateBoardRequest
        {
            Name = "Filter Test Board"
        };

        BoardDto board = await m_BoardService.CreateBoardAsync(project.Id, boardRequest, m_UserId);

        // Arrange: Create two epics
        CreateEpicRequest epic1Request = new CreateEpicRequest
        {
            ProjectId = project.Id,
            Summary = "Epic 1",
            Description = "First epic"
        };

        CreateEpicRequest epic2Request = new CreateEpicRequest
        {
            ProjectId = project.Id,
            Summary = "Epic 2",
            Description = "Second epic"
        };

        EpicDto epic1 = await m_EpicService.CreateEpicAsync(epic1Request, m_UserId);
        EpicDto epic2 = await m_EpicService.CreateEpicAsync(epic2Request, m_UserId);

        // Arrange: Create work items in different epics
        CreateWorkItemRequest epic1Task1 = new CreateWorkItemRequest
        {
            ProjectId = project.Id,
            EpicId = epic1.Id,
            Type = WorkItemType.Task,
            Summary = "Epic 1 Task 1"
        };

        CreateWorkItemRequest epic1Task2 = new CreateWorkItemRequest
        {
            ProjectId = project.Id,
            EpicId = epic1.Id,
            Type = WorkItemType.Task,
            Summary = "Epic 1 Task 2"
        };

        CreateWorkItemRequest epic2Task1 = new CreateWorkItemRequest
        {
            ProjectId = project.Id,
            EpicId = epic2.Id,
            Type = WorkItemType.Task,
            Summary = "Epic 2 Task 1"
        };

        CreateWorkItemRequest noEpicTask = new CreateWorkItemRequest
        {
            ProjectId = project.Id,
            Type = WorkItemType.Task,
            Summary = "No Epic Task"
        };

        await m_WorkItemService.CreateWorkItemAsync(epic1Task1, m_UserId);
        await m_WorkItemService.CreateWorkItemAsync(epic1Task2, m_UserId);
        await m_WorkItemService.CreateWorkItemAsync(epic2Task1, m_UserId);
        await m_WorkItemService.CreateWorkItemAsync(noEpicTask, m_UserId);

        // Act: Get board view without filter
        BoardViewDto unfilteredView = await m_BoardService.GetBoardViewAsync(project.Id);
        Assert.Equal(4, unfilteredView.TotalItems);

        // Act: Filter by Epic 1
        BoardViewDto epic1View = await m_BoardService.GetBoardViewAsync(project.Id, epicIds: new[] { epic1.Id });

        // Assert: Only Epic 1 work items visible
        Assert.Equal(2, epic1View.TotalItems);
        BoardColumnViewDto epic1TodoColumn = epic1View.Columns.First(c => c.StatusName == "TODO");
        Assert.Equal(2, epic1TodoColumn.ItemCount);
        Assert.All(epic1TodoColumn.WorkItems, w => Assert.StartsWith("Epic 1", w.Summary));

        // Act: Filter by Epic 2
        BoardViewDto epic2View = await m_BoardService.GetBoardViewAsync(project.Id, epicIds: new[] { epic2.Id });

        // Assert: Only Epic 2 work items visible
        Assert.Equal(1, epic2View.TotalItems);
        BoardColumnViewDto epic2TodoColumn = epic2View.Columns.First(c => c.StatusName == "TODO");
        Assert.Equal(1, epic2TodoColumn.ItemCount);
        Assert.Equal("Epic 2 Task 1", epic2TodoColumn.WorkItems[0].Summary);

        // Act: Filter by both epics
        BoardViewDto bothEpicsView = await m_BoardService.GetBoardViewAsync(
            project.Id,
            epicIds: new[] { epic1.Id, epic2.Id });

        // Assert: All epic work items visible, but not no-epic task
        Assert.Equal(3, bothEpicsView.TotalItems);
        BoardColumnViewDto bothEpicsTodoColumn = bothEpicsView.Columns.First(c => c.StatusName == "TODO");
        Assert.Equal(3, bothEpicsTodoColumn.ItemCount);
        Assert.DoesNotContain(bothEpicsTodoColumn.WorkItems, w => w.Summary == "No Epic Task");
    }

    [Fact]
    public async Task IntegrationTest_ReorderColumns_VerifyPersistence_Success()
    {
        // Arrange: Create project and board
        CreateProjectRequest projectRequest = new CreateProjectRequest
        {
            Key = "ORD",
            Name = "Order Test Project",
            Description = "Test project for column ordering"
        };

        ProjectDto project = await m_ProjectService.CreateProjectAsync(projectRequest, m_UserId);

        CreateBoardRequest boardRequest = new CreateBoardRequest
        {
            Name = "Order Test Board"
        };

        BoardDto board = await m_BoardService.CreateBoardAsync(project.Id, boardRequest, m_UserId);

        // Arrange: Get initial board view to see column order
        BoardViewDto initialView = await m_BoardService.GetBoardViewAsync(project.Id);
        List<BoardColumnViewDto> initialColumns = initialView.Columns.ToList();

        // Verify initial order (should be ordered by OrderIndex)
        Assert.Equal(5, initialColumns.Count);
        for (int i = 0; i < initialColumns.Count; i++)
        {
            Assert.Equal(i, initialColumns[i].OrderIndex);
        }

        // Arrange: Create reorder request (reverse the order)
        ReorderColumnsRequest reorderRequest = new ReorderColumnsRequest
        {
            ColumnIds = new List<int>
            {
                initialColumns[4].Id,
                initialColumns[3].Id,
                initialColumns[2].Id,
                initialColumns[1].Id,
                initialColumns[0].Id
            }
        };

        // Act: Reorder columns
        await m_BoardService.ReorderColumnsAsync(project.Id, reorderRequest, m_UserId);

        // Assert: Get board view again and verify new order
        BoardViewDto reorderedView = await m_BoardService.GetBoardViewAsync(project.Id);
        List<BoardColumnViewDto> reorderedColumns = reorderedView.Columns.ToList();

        Assert.Equal(5, reorderedColumns.Count);

        // Verify columns are in reverse order
        Assert.Equal(initialColumns[4].StatusId, reorderedColumns[0].StatusId);
        Assert.Equal(initialColumns[3].StatusId, reorderedColumns[1].StatusId);
        Assert.Equal(initialColumns[2].StatusId, reorderedColumns[2].StatusId);
        Assert.Equal(initialColumns[1].StatusId, reorderedColumns[3].StatusId);
        Assert.Equal(initialColumns[0].StatusId, reorderedColumns[4].StatusId);

        // Verify OrderIndex values are correct
        for (int i = 0; i < reorderedColumns.Count; i++)
        {
            Assert.Equal(i, reorderedColumns[i].OrderIndex);
        }

        // Assert: Verify persistence by checking database directly
        List<BoardColumn> columnsInDb = await m_Context.BoardColumns
            .Where(bc => bc.BoardId == board.Id)
            .OrderBy(bc => bc.OrderIndex)
            .ToListAsync();

        Assert.Equal(5, columnsInDb.Count);

        // Verify database has persisted the new order
        for (int i = 0; i < columnsInDb.Count; i++)
        {
            Assert.Equal(reorderedColumns[i].StatusId, columnsInDb[i].StatusId);
            Assert.Equal(i, columnsInDb[i].OrderIndex);
        }
    }

    [Fact]
    public async Task IntegrationTest_UpdateColumnWIPLimits_VerifyConfiguration_Success()
    {
        // Arrange: Create project and board
        CreateProjectRequest projectRequest = new CreateProjectRequest
        {
            Key = "WIP",
            Name = "WIP Test Project",
            Description = "Test project for WIP limits"
        };

        ProjectDto project = await m_ProjectService.CreateProjectAsync(projectRequest, m_UserId);

        CreateBoardRequest boardRequest = new CreateBoardRequest
        {
            Name = "WIP Test Board"
        };

        BoardDto board = await m_BoardService.CreateBoardAsync(project.Id, boardRequest, m_UserId);

        // Arrange: Get board view and select a column
        BoardViewDto initialView = await m_BoardService.GetBoardViewAsync(project.Id);
        BoardColumnViewDto targetColumn = initialView.Columns.First(c => c.StatusName == "IN PROGRESS");

        // Verify initial WIP limit is null (no limit)
        Assert.Null(targetColumn.WIPLimit);
        Assert.False(targetColumn.IsCollapsed);

        // Arrange: Create update request
        UpdateBoardColumnRequest updateRequest = new UpdateBoardColumnRequest
        {
            WIPLimit = 5,
            IsCollapsed = true
        };

        // Act: Update column configuration
        BoardColumnDto updatedColumn = await m_BoardService.UpdateColumnAsync(
            project.Id,
            targetColumn.Id,
            updateRequest,
            m_UserId);

        // Assert: Column updated correctly
        Assert.Equal(5, updatedColumn.WIPLimit);
        Assert.True(updatedColumn.IsCollapsed);

        // Assert: Get board view again and verify column configuration persisted
        BoardViewDto updatedView = await m_BoardService.GetBoardViewAsync(project.Id);
        BoardColumnViewDto verifyColumn = updatedView.Columns.First(c => c.Id == targetColumn.Id);

        Assert.Equal(5, verifyColumn.WIPLimit);
        Assert.True(verifyColumn.IsCollapsed);

        // Act: Update column to remove WIP limit
        UpdateBoardColumnRequest removeWIPRequest = new UpdateBoardColumnRequest
        {
            WIPLimit = null,
            IsCollapsed = false
        };

        BoardColumnDto finalColumn = await m_BoardService.UpdateColumnAsync(
            project.Id,
            targetColumn.Id,
            removeWIPRequest,
            m_UserId);

        // Assert: WIP limit removed
        Assert.Null(finalColumn.WIPLimit);
        Assert.False(finalColumn.IsCollapsed);

        // Assert: Verify persistence after removal
        BoardViewDto finalView = await m_BoardService.GetBoardViewAsync(project.Id);
        BoardColumnViewDto finalVerifyColumn = finalView.Columns.First(c => c.Id == targetColumn.Id);

        Assert.Null(finalVerifyColumn.WIPLimit);
        Assert.False(finalVerifyColumn.IsCollapsed);
    }
}
