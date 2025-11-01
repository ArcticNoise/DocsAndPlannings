using DocsAndPlannings.Core.Data;
using DocsAndPlannings.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DocsAndPlannings.Core.Tests.Data;

/// <summary>
/// Unit tests for Board and BoardColumn models
/// </summary>
public class BoardModelsTests : IDisposable
{
    private readonly ApplicationDbContext _context;

    public BoardModelsTests()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new ApplicationDbContext(options);
    }

    [Fact]
    public void CanCreateBoard()
    {
        User owner = CreateTestUser("owner@example.com");
        _context.Users.Add(owner);
        _context.SaveChanges();

        Project project = CreateTestProject("TEST", "Test Project", owner.Id);
        _context.Projects.Add(project);
        _context.SaveChanges();

        Board board = new Board
        {
            ProjectId = project.Id,
            Name = "Test Board",
            Description = "A test Kanban board",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Boards.Add(board);
        int result = _context.SaveChanges();

        Assert.Equal(1, result);
        Assert.True(board.Id > 0);
        Assert.Equal("Test Board", board.Name);
        Assert.Equal("A test Kanban board", board.Description);
    }

    [Fact]
    public void CanUpdateBoard()
    {
        User owner = CreateTestUser("owner@example.com");
        _context.Users.Add(owner);
        _context.SaveChanges();

        Project project = CreateTestProject("TEST", "Test Project", owner.Id);
        _context.Projects.Add(project);
        _context.SaveChanges();

        Board board = new Board
        {
            ProjectId = project.Id,
            Name = "Original Name",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Boards.Add(board);
        _context.SaveChanges();

        board.Name = "Updated Name";
        board.Description = "Updated Description";
        board.UpdatedAt = DateTime.UtcNow;

        int result = _context.SaveChanges();

        Assert.Equal(1, result);
        Assert.Equal("Updated Name", board.Name);
        Assert.Equal("Updated Description", board.Description);
    }

    [Fact]
    public void BoardNameIsRequired()
    {
        User owner = CreateTestUser("owner@example.com");
        _context.Users.Add(owner);
        _context.SaveChanges();

        Project project = CreateTestProject("TEST", "Test Project", owner.Id);
        _context.Projects.Add(project);
        _context.SaveChanges();

        Board board = new Board
        {
            ProjectId = project.Id,
            Name = null!,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Boards.Add(board);

        Assert.Throws<DbUpdateException>(() => _context.SaveChanges());
    }

    [Fact]
    public void BoardDescriptionIsOptional()
    {
        User owner = CreateTestUser("owner@example.com");
        _context.Users.Add(owner);
        _context.SaveChanges();

        Project project = CreateTestProject("TEST", "Test Project", owner.Id);
        _context.Projects.Add(project);
        _context.SaveChanges();

        Board board = new Board
        {
            ProjectId = project.Id,
            Name = "Test Board",
            Description = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Boards.Add(board);
        int result = _context.SaveChanges();

        Assert.Equal(1, result);
        Assert.Null(board.Description);
    }

    [Fact]
    public void BoardHasProjectRelationship()
    {
        User owner = CreateTestUser("owner@example.com");
        _context.Users.Add(owner);
        _context.SaveChanges();

        Project project = CreateTestProject("TEST", "Test Project", owner.Id);
        _context.Projects.Add(project);
        _context.SaveChanges();

        Board board = new Board
        {
            ProjectId = project.Id,
            Name = "Test Board",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Boards.Add(board);
        _context.SaveChanges();

        Board? retrievedBoard = _context.Boards
            .Include(b => b.Project)
            .FirstOrDefault(b => b.Id == board.Id);

        Assert.NotNull(retrievedBoard);
        Assert.NotNull(retrievedBoard.Project);
        Assert.Equal(project.Id, retrievedBoard.ProjectId);
        Assert.Equal("Test Project", retrievedBoard.Project.Name);
    }

    [Fact]
    public void MultipleProjectsCanHaveBoards()
    {
        User owner = CreateTestUser("owner@example.com");
        _context.Users.Add(owner);
        _context.SaveChanges();

        Project project1 = CreateTestProject("TEST1", "Test Project 1", owner.Id);
        Project project2 = CreateTestProject("TEST2", "Test Project 2", owner.Id);
        _context.Projects.AddRange(project1, project2);
        _context.SaveChanges();

        Board board1 = new Board
        {
            ProjectId = project1.Id,
            Name = "Board 1",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Board board2 = new Board
        {
            ProjectId = project2.Id,
            Name = "Board 2",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Boards.AddRange(board1, board2);
        int result = _context.SaveChanges();

        Assert.Equal(2, result);
        Assert.Equal(project1.Id, board1.ProjectId);
        Assert.Equal(project2.Id, board2.ProjectId);
    }

    [Fact]
    public void DeletingProjectCascadesDeleteBoard()
    {
        User owner = CreateTestUser("owner@example.com");
        _context.Users.Add(owner);
        _context.SaveChanges();

        Project project = CreateTestProject("TEST", "Test Project", owner.Id);
        _context.Projects.Add(project);
        _context.SaveChanges();

        Board board = new Board
        {
            ProjectId = project.Id,
            Name = "Test Board",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Boards.Add(board);
        _context.SaveChanges();

        int projectId = project.Id;

        _context.Projects.Remove(project);
        _context.SaveChanges();

        Board? deletedBoard = _context.Boards.FirstOrDefault(b => b.ProjectId == projectId);

        Assert.Null(deletedBoard);
    }

    [Fact]
    public void CanCreateBoardColumn()
    {
        User owner = CreateTestUser("owner@example.com");
        _context.Users.Add(owner);
        _context.SaveChanges();

        Project project = CreateTestProject("TEST", "Test Project", owner.Id);
        _context.Projects.Add(project);
        _context.SaveChanges();

        Board board = new Board
        {
            ProjectId = project.Id,
            Name = "Test Board",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Status status = CreateTestStatus("TODO", 1);

        _context.Boards.Add(board);
        _context.Statuses.Add(status);
        _context.SaveChanges();

        BoardColumn column = new BoardColumn
        {
            BoardId = board.Id,
            StatusId = status.Id,
            OrderIndex = 0,
            WIPLimit = 5,
            IsCollapsed = false,
            RowVersion = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
        };

        _context.BoardColumns.Add(column);
        int result = _context.SaveChanges();

        Assert.Equal(1, result);
        Assert.True(column.Id > 0);
        Assert.Equal(0, column.OrderIndex);
        Assert.Equal(5, column.WIPLimit);
        Assert.False(column.IsCollapsed);
        Assert.NotNull(column.RowVersion);
    }

    [Fact]
    public void CanUpdateBoardColumn()
    {
        User owner = CreateTestUser("owner@example.com");
        _context.Users.Add(owner);
        _context.SaveChanges();

        Project project = CreateTestProject("TEST", "Test Project", owner.Id);
        _context.Projects.Add(project);
        _context.SaveChanges();

        Board board = new Board
        {
            ProjectId = project.Id,
            Name = "Test Board",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Status status = CreateTestStatus("TODO", 1);

        _context.Boards.Add(board);
        _context.Statuses.Add(status);
        _context.SaveChanges();

        BoardColumn column = new BoardColumn
        {
            BoardId = board.Id,
            StatusId = status.Id,
            OrderIndex = 0,
            WIPLimit = 5,
            IsCollapsed = false,
            RowVersion = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
        };

        _context.BoardColumns.Add(column);
        _context.SaveChanges();

        byte[] originalRowVersion = column.RowVersion.ToArray();

        column.WIPLimit = 10;
        column.IsCollapsed = true;

        int result = _context.SaveChanges();

        Assert.Equal(1, result);
        Assert.Equal(10, column.WIPLimit);
        Assert.True(column.IsCollapsed);
    }

    [Fact]
    public void BoardColumnHasStatusRelationship()
    {
        User owner = CreateTestUser("owner@example.com");
        _context.Users.Add(owner);
        _context.SaveChanges();

        Project project = CreateTestProject("TEST", "Test Project", owner.Id);
        _context.Projects.Add(project);
        _context.SaveChanges();

        Board board = new Board
        {
            ProjectId = project.Id,
            Name = "Test Board",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Status status = CreateTestStatus("TODO", 1);

        _context.Boards.Add(board);
        _context.Statuses.Add(status);
        _context.SaveChanges();

        BoardColumn column = new BoardColumn
        {
            BoardId = board.Id,
            StatusId = status.Id,
            OrderIndex = 0,
            RowVersion = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
        };

        _context.BoardColumns.Add(column);
        _context.SaveChanges();

        BoardColumn? retrievedColumn = _context.BoardColumns
            .Include(c => c.Status)
            .FirstOrDefault(c => c.Id == column.Id);

        Assert.NotNull(retrievedColumn);
        Assert.NotNull(retrievedColumn.Status);
        Assert.Equal(status.Id, retrievedColumn.StatusId);
        Assert.Equal("TODO", retrievedColumn.Status.Name);
    }

    [Fact]
    public void BoardColumnHasBoardRelationship()
    {
        User owner = CreateTestUser("owner@example.com");
        _context.Users.Add(owner);
        _context.SaveChanges();

        Project project = CreateTestProject("TEST", "Test Project", owner.Id);
        _context.Projects.Add(project);
        _context.SaveChanges();

        Board board = new Board
        {
            ProjectId = project.Id,
            Name = "Test Board",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Status status = CreateTestStatus("TODO", 1);

        _context.Boards.Add(board);
        _context.Statuses.Add(status);
        _context.SaveChanges();

        BoardColumn column = new BoardColumn
        {
            BoardId = board.Id,
            StatusId = status.Id,
            OrderIndex = 0,
            RowVersion = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
        };

        _context.BoardColumns.Add(column);
        _context.SaveChanges();

        BoardColumn? retrievedColumn = _context.BoardColumns
            .Include(c => c.Board)
            .FirstOrDefault(c => c.Id == column.Id);

        Assert.NotNull(retrievedColumn);
        Assert.NotNull(retrievedColumn.Board);
        Assert.Equal(board.Id, retrievedColumn.BoardId);
        Assert.Equal("Test Board", retrievedColumn.Board.Name);
    }

    [Fact]
    public void BoardCanHaveMultipleColumns()
    {
        User owner = CreateTestUser("owner@example.com");
        _context.Users.Add(owner);
        _context.SaveChanges();

        Project project = CreateTestProject("TEST", "Test Project", owner.Id);
        _context.Projects.Add(project);
        _context.SaveChanges();

        Board board = new Board
        {
            ProjectId = project.Id,
            Name = "Test Board",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Status status1 = CreateTestStatus("TODO", 1);
        Status status2 = CreateTestStatus("IN_PROGRESS", 2);
        Status status3 = CreateTestStatus("DONE", 3);

        _context.Boards.Add(board);
        _context.Statuses.AddRange(status1, status2, status3);
        _context.SaveChanges();

        BoardColumn column1 = new BoardColumn
        {
            BoardId = board.Id,
            StatusId = status1.Id,
            OrderIndex = 0,
            RowVersion = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
        };

        BoardColumn column2 = new BoardColumn
        {
            BoardId = board.Id,
            StatusId = status2.Id,
            OrderIndex = 1,
            RowVersion = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
        };

        BoardColumn column3 = new BoardColumn
        {
            BoardId = board.Id,
            StatusId = status3.Id,
            OrderIndex = 2,
            RowVersion = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
        };

        _context.BoardColumns.AddRange(column1, column2, column3);
        int result = _context.SaveChanges();

        Assert.Equal(3, result);

        Board? boardWithColumns = _context.Boards
            .Include(b => b.BoardColumns)
            .FirstOrDefault(b => b.Id == board.Id);

        Assert.NotNull(boardWithColumns);
        Assert.Equal(3, boardWithColumns.BoardColumns.Count);
    }

    [Fact]
    public void DeletingBoardCascadesDeleteColumns()
    {
        User owner = CreateTestUser("owner@example.com");
        _context.Users.Add(owner);
        _context.SaveChanges();

        Project project = CreateTestProject("TEST", "Test Project", owner.Id);
        _context.Projects.Add(project);
        _context.SaveChanges();

        Board board = new Board
        {
            ProjectId = project.Id,
            Name = "Test Board",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Status status = CreateTestStatus("TODO", 1);

        _context.Boards.Add(board);
        _context.Statuses.Add(status);
        _context.SaveChanges();

        BoardColumn column = new BoardColumn
        {
            BoardId = board.Id,
            StatusId = status.Id,
            OrderIndex = 0,
            RowVersion = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
        };

        _context.BoardColumns.Add(column);
        _context.SaveChanges();

        int boardId = board.Id;

        _context.Boards.Remove(board);
        _context.SaveChanges();

        BoardColumn? deletedColumn = _context.BoardColumns.FirstOrDefault(c => c.BoardId == boardId);

        Assert.Null(deletedColumn);
    }

    [Fact]
    public void RowVersionIsGenerated()
    {
        User owner = CreateTestUser("owner@example.com");
        _context.Users.Add(owner);
        _context.SaveChanges();

        Project project = CreateTestProject("TEST", "Test Project", owner.Id);
        _context.Projects.Add(project);
        _context.SaveChanges();

        Board board = new Board
        {
            ProjectId = project.Id,
            Name = "Test Board",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Status status = CreateTestStatus("TODO", 1);

        _context.Boards.Add(board);
        _context.Statuses.Add(status);
        _context.SaveChanges();

        BoardColumn column = new BoardColumn
        {
            BoardId = board.Id,
            StatusId = status.Id,
            OrderIndex = 0,
            RowVersion = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
        };

        _context.BoardColumns.Add(column);
        _context.SaveChanges();

        Assert.NotNull(column.RowVersion);
        Assert.NotEmpty(column.RowVersion);
    }

    [Fact]
    public void CanReorderColumns()
    {
        User owner = CreateTestUser("owner@example.com");
        _context.Users.Add(owner);
        _context.SaveChanges();

        Project project = CreateTestProject("TEST", "Test Project", owner.Id);
        _context.Projects.Add(project);
        _context.SaveChanges();

        Board board = new Board
        {
            ProjectId = project.Id,
            Name = "Test Board",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Status status1 = CreateTestStatus("TODO", 1);
        Status status2 = CreateTestStatus("IN_PROGRESS", 2);
        Status status3 = CreateTestStatus("DONE", 3);

        _context.Boards.Add(board);
        _context.Statuses.AddRange(status1, status2, status3);
        _context.SaveChanges();

        BoardColumn column1 = new BoardColumn
        {
            BoardId = board.Id,
            StatusId = status1.Id,
            OrderIndex = 0,
            RowVersion = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
        };

        BoardColumn column2 = new BoardColumn
        {
            BoardId = board.Id,
            StatusId = status2.Id,
            OrderIndex = 1,
            RowVersion = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
        };

        BoardColumn column3 = new BoardColumn
        {
            BoardId = board.Id,
            StatusId = status3.Id,
            OrderIndex = 2,
            RowVersion = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
        };

        _context.BoardColumns.AddRange(column1, column2, column3);
        _context.SaveChanges();

        column1.OrderIndex = 2;
        column2.OrderIndex = 0;
        column3.OrderIndex = 1;

        int result = _context.SaveChanges();

        Assert.Equal(3, result);
        Assert.Equal(2, column1.OrderIndex);
        Assert.Equal(0, column2.OrderIndex);
        Assert.Equal(1, column3.OrderIndex);
    }

    private User CreateTestUser(string email)
    {
        return new User
        {
            Email = email,
            PasswordHash = "hash123",
            FirstName = "Test",
            LastName = "User",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private Project CreateTestProject(string key, string name, int ownerId)
    {
        return new Project
        {
            Key = key,
            Name = name,
            OwnerId = ownerId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private Status CreateTestStatus(string name, int orderIndex)
    {
        return new Status
        {
            Name = name,
            OrderIndex = orderIndex,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
