using System.Security.Claims;
using DocsAndPlannings.Api.Controllers;
using DocsAndPlannings.Core.Data;
using DocsAndPlannings.Core.DTOs.Boards;
using DocsAndPlannings.Core.Exceptions;
using DocsAndPlannings.Core.Exceptions.Planning;
using DocsAndPlannings.Core.Models;
using DocsAndPlannings.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DocsAndPlannings.Core.Tests.Controllers;

public sealed class BoardsControllerTests : IDisposable
{
    private readonly ApplicationDbContext m_Context;
    private readonly IStatusService m_StatusService;
    private readonly IBoardService m_BoardService;
    private readonly BoardsController m_Controller;

    public BoardsControllerTests()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        m_Context = new ApplicationDbContext(options);
        m_StatusService = new StatusService(m_Context);
        m_BoardService = new BoardService(m_Context, m_StatusService);
        m_Controller = new BoardsController(m_BoardService);

        SetupControllerContext();
        SeedTestData();
    }

    public void Dispose()
    {
        m_Context.Dispose();
    }

    private void SetupControllerContext()
    {
        ClaimsPrincipal user = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, "User")
        ], "TestAuth"));

        m_Controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    private void SeedTestData()
    {
        User owner = new User
        {
            Id = 1,
            Email = "owner@example.com",
            PasswordHash = "hash",
            FirstName = "Owner",
            LastName = "User",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        User otherUser = new User
        {
            Id = 2,
            Email = "other@example.com",
            PasswordHash = "hash",
            FirstName = "Other",
            LastName = "User",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Status todoStatus = new Status
        {
            Id = 1,
            Name = "TODO",
            Color = "#95a5a6",
            OrderIndex = 1,
            CreatedAt = DateTime.UtcNow
        };

        Status inProgressStatus = new Status
        {
            Id = 2,
            Name = "IN PROGRESS",
            Color = "#3498db",
            OrderIndex = 2,
            CreatedAt = DateTime.UtcNow
        };

        Status doneStatus = new Status
        {
            Id = 3,
            Name = "DONE",
            Color = "#2ecc71",
            OrderIndex = 3,
            CreatedAt = DateTime.UtcNow
        };

        Project project = new Project
        {
            Id = 1,
            Key = "TEST",
            Name = "Test Project",
            OwnerId = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        m_Context.Users.AddRange(owner, otherUser);
        m_Context.Statuses.AddRange(todoStatus, inProgressStatus, doneStatus);
        m_Context.Projects.Add(project);
        m_Context.SaveChanges();
    }

    #region CreateBoard Tests

    [Fact]
    public async Task CreateBoard_ValidRequest_ReturnsCreatedAtAction()
    {
        CreateBoardRequest request = new CreateBoardRequest
        {
            Name = "Test Board",
            Description = "Test Description"
        };

        ActionResult<BoardDto> result = await m_Controller.CreateBoard(1, request);

        CreatedAtActionResult createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        BoardDto board = Assert.IsType<BoardDto>(createdResult.Value);
        Assert.Equal("Test Board", board.Name);
        Assert.Equal("Test Description", board.Description);
        Assert.Equal(1, board.ProjectId);
        Assert.Equal("GetBoard", createdResult.ActionName);
    }

    [Fact]
    public async Task CreateBoard_InvalidProjectId_ReturnsBadRequest()
    {
        CreateBoardRequest request = new CreateBoardRequest
        {
            Name = "Test Board"
        };

        ActionResult<BoardDto> result = await m_Controller.CreateBoard(0, request);

        BadRequestObjectResult badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Contains("Invalid project ID", badRequestResult.Value?.ToString());
    }

    [Fact]
    public async Task CreateBoard_NegativeProjectId_ReturnsBadRequest()
    {
        CreateBoardRequest request = new CreateBoardRequest
        {
            Name = "Test Board"
        };

        ActionResult<BoardDto> result = await m_Controller.CreateBoard(-1, request);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    #endregion

    #region GetBoard Tests

    [Fact]
    public async Task GetBoard_ExistingBoard_ReturnsOkWithBoard()
    {
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        await m_Controller.CreateBoard(1, createRequest);

        ActionResult<BoardDto> result = await m_Controller.GetBoard(1);

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        BoardDto board = Assert.IsType<BoardDto>(okResult.Value);
        Assert.Equal("Test Board", board.Name);
    }

    [Fact]
    public async Task GetBoard_NonExistingBoard_ReturnsNotFound()
    {
        ActionResult<BoardDto> result = await m_Controller.GetBoard(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetBoard_InvalidProjectId_ReturnsBadRequest()
    {
        ActionResult<BoardDto> result = await m_Controller.GetBoard(0);

        BadRequestObjectResult badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Contains("Invalid project ID", badRequestResult.Value?.ToString());
    }

    #endregion

    #region UpdateBoard Tests

    [Fact]
    public async Task UpdateBoard_ValidRequest_ReturnsOkWithUpdatedBoard()
    {
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Old Name" };
        await m_Controller.CreateBoard(1, createRequest);

        UpdateBoardRequest updateRequest = new UpdateBoardRequest
        {
            Name = "New Name",
            Description = "New Description"
        };

        ActionResult<BoardDto> result = await m_Controller.UpdateBoard(1, updateRequest);

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        BoardDto board = Assert.IsType<BoardDto>(okResult.Value);
        Assert.Equal("New Name", board.Name);
        Assert.Equal("New Description", board.Description);
    }

    [Fact]
    public async Task UpdateBoard_InvalidProjectId_ReturnsBadRequest()
    {
        UpdateBoardRequest request = new UpdateBoardRequest { Name = "New Name" };

        ActionResult<BoardDto> result = await m_Controller.UpdateBoard(0, request);

        BadRequestObjectResult badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Contains("Invalid project ID", badRequestResult.Value?.ToString());
    }

    [Fact]
    public async Task UpdateBoard_NonExistingBoard_ThrowsNotFoundException()
    {
        UpdateBoardRequest request = new UpdateBoardRequest { Name = "New Name" };

        await Assert.ThrowsAsync<NotFoundException>(() =>
            m_Controller.UpdateBoard(999, request));
    }

    #endregion

    #region DeleteBoard Tests

    [Fact]
    public async Task DeleteBoard_ExistingBoard_ReturnsNoContent()
    {
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        await m_Controller.CreateBoard(1, createRequest);

        ActionResult result = await m_Controller.DeleteBoard(1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteBoard_InvalidProjectId_ReturnsBadRequest()
    {
        ActionResult result = await m_Controller.DeleteBoard(0);

        BadRequestObjectResult badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Invalid project ID", badRequestResult.Value?.ToString());
    }

    [Fact]
    public async Task DeleteBoard_NonExistingBoard_ThrowsNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            m_Controller.DeleteBoard(999));
    }

    #endregion

    #region GetBoardView Tests

    [Fact]
    public async Task GetBoardView_ExistingBoard_ReturnsOkWithBoardView()
    {
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        await m_Controller.CreateBoard(1, createRequest);

        ActionResult<BoardViewDto> result = await m_Controller.GetBoardView(1);

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        BoardViewDto boardView = Assert.IsType<BoardViewDto>(okResult.Value);
        Assert.NotNull(boardView);
        Assert.Equal(3, boardView.Columns.Count); // 3 statuses
    }

    [Fact]
    public async Task GetBoardView_WithEpicFilter_ReturnsOkWithFilteredView()
    {
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        await m_Controller.CreateBoard(1, createRequest);

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

        ActionResult<BoardViewDto> result = await m_Controller.GetBoardView(1, epicIds: new[] { epic.Id });

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        BoardViewDto boardView = Assert.IsType<BoardViewDto>(okResult.Value);
        Assert.NotNull(boardView);
    }

    [Fact]
    public async Task GetBoardView_WithSearchText_ReturnsOkWithFilteredView()
    {
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        await m_Controller.CreateBoard(1, createRequest);

        ActionResult<BoardViewDto> result = await m_Controller.GetBoardView(1, searchText: "test");

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        BoardViewDto boardView = Assert.IsType<BoardViewDto>(okResult.Value);
        Assert.NotNull(boardView);
    }

    [Fact]
    public async Task GetBoardView_InvalidProjectId_ReturnsBadRequest()
    {
        ActionResult<BoardViewDto> result = await m_Controller.GetBoardView(0);

        BadRequestObjectResult badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Contains("Invalid project ID", badRequestResult.Value?.ToString());
    }

    #endregion

    #region UpdateColumn Tests

    [Fact]
    public async Task UpdateColumn_ValidRequest_ReturnsOkWithUpdatedColumn()
    {
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        ActionResult<BoardDto> createResult = await m_Controller.CreateBoard(1, createRequest);
        CreatedAtActionResult createdResult = Assert.IsType<CreatedAtActionResult>(createResult.Result);
        BoardDto board = Assert.IsType<BoardDto>(createdResult.Value);
        int columnId = board.Columns[0].Id;

        UpdateBoardColumnRequest updateRequest = new UpdateBoardColumnRequest
        {
            WIPLimit = 5,
            IsCollapsed = true
        };

        ActionResult<BoardColumnDto> result = await m_Controller.UpdateColumn(1, columnId, updateRequest);

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        BoardColumnDto column = Assert.IsType<BoardColumnDto>(okResult.Value);
        Assert.Equal(5, column.WIPLimit);
        Assert.True(column.IsCollapsed);
    }

    [Fact]
    public async Task UpdateColumn_InvalidProjectId_ReturnsBadRequest()
    {
        UpdateBoardColumnRequest request = new UpdateBoardColumnRequest { WIPLimit = 5, IsCollapsed = false };

        ActionResult<BoardColumnDto> result = await m_Controller.UpdateColumn(0, 1, request);

        BadRequestObjectResult badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Contains("Invalid project ID", badRequestResult.Value?.ToString());
    }

    [Fact]
    public async Task UpdateColumn_InvalidColumnId_ReturnsBadRequest()
    {
        UpdateBoardColumnRequest request = new UpdateBoardColumnRequest { WIPLimit = 5, IsCollapsed = false };

        ActionResult<BoardColumnDto> result = await m_Controller.UpdateColumn(1, 0, request);

        BadRequestObjectResult badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Contains("Invalid column ID", badRequestResult.Value?.ToString());
    }

    [Fact]
    public async Task UpdateColumn_NonExistingColumn_ThrowsNotFoundException()
    {
        UpdateBoardColumnRequest request = new UpdateBoardColumnRequest { WIPLimit = 5, IsCollapsed = false };

        await Assert.ThrowsAsync<NotFoundException>(() =>
            m_Controller.UpdateColumn(1, 999, request));
    }

    #endregion

    #region ReorderColumns Tests

    [Fact]
    public async Task ReorderColumns_ValidRequest_ReturnsNoContent()
    {
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        ActionResult<BoardDto> createResult = await m_Controller.CreateBoard(1, createRequest);
        CreatedAtActionResult createdResult = Assert.IsType<CreatedAtActionResult>(createResult.Result);
        BoardDto board = Assert.IsType<BoardDto>(createdResult.Value);

        ReorderColumnsRequest reorderRequest = new ReorderColumnsRequest
        {
            ColumnIds = new[] { board.Columns[2].Id, board.Columns[1].Id, board.Columns[0].Id }
        };

        ActionResult result = await m_Controller.ReorderColumns(1, reorderRequest);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task ReorderColumns_InvalidProjectId_ReturnsBadRequest()
    {
        ReorderColumnsRequest request = new ReorderColumnsRequest
        {
            ColumnIds = new[] { 1, 2, 3 }
        };

        ActionResult result = await m_Controller.ReorderColumns(0, request);

        BadRequestObjectResult badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Invalid project ID", badRequestResult.Value?.ToString());
    }

    [Fact]
    public async Task ReorderColumns_NonExistingBoard_ThrowsNotFoundException()
    {
        ReorderColumnsRequest request = new ReorderColumnsRequest
        {
            ColumnIds = new[] { 1, 2, 3 }
        };

        await Assert.ThrowsAsync<NotFoundException>(() =>
            m_Controller.ReorderColumns(999, request));
    }

    #endregion

    #region MoveWorkItem Tests

    [Fact]
    public async Task MoveWorkItem_ValidRequest_ReturnsNoContent()
    {
        CreateBoardRequest createRequest = new CreateBoardRequest { Name = "Test Board" };
        await m_Controller.CreateBoard(1, createRequest);

        WorkItem workItem = new WorkItem
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
        m_Context.WorkItems.Add(workItem);
        await m_Context.SaveChangesAsync();

        ActionResult result = await m_Controller.MoveWorkItem(1, workItem.Id, 2);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task MoveWorkItem_InvalidProjectId_ReturnsBadRequest()
    {
        ActionResult result = await m_Controller.MoveWorkItem(0, 1, 2);

        BadRequestObjectResult badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Invalid project ID", badRequestResult.Value?.ToString());
    }

    [Fact]
    public async Task MoveWorkItem_InvalidWorkItemId_ReturnsBadRequest()
    {
        ActionResult result = await m_Controller.MoveWorkItem(1, 0, 2);

        BadRequestObjectResult badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Invalid work item ID", badRequestResult.Value?.ToString());
    }

    [Fact]
    public async Task MoveWorkItem_InvalidStatusId_ReturnsBadRequest()
    {
        ActionResult result = await m_Controller.MoveWorkItem(1, 1, 0);

        BadRequestObjectResult badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Invalid status ID", badRequestResult.Value?.ToString());
    }

    [Fact]
    public async Task MoveWorkItem_NonExistingWorkItem_ThrowsNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            m_Controller.MoveWorkItem(1, 999, 2));
    }

    #endregion
}
