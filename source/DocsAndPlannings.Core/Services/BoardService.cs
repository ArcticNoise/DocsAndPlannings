using DocsAndPlannings.Core.Data;
using DocsAndPlannings.Core.DTOs.Boards;
using DocsAndPlannings.Core.Exceptions;
using DocsAndPlannings.Core.Exceptions.Planning;
using DocsAndPlannings.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DocsAndPlannings.Core.Services;

/// <summary>
/// Service for managing Kanban boards
/// </summary>
public sealed class BoardService : IBoardService
{
    private readonly ApplicationDbContext m_Context;
    private readonly IStatusService m_StatusService;

    public BoardService(
        ApplicationDbContext context,
        IStatusService statusService)
    {
        m_Context = context ?? throw new ArgumentNullException(nameof(context));
        m_StatusService = statusService ?? throw new ArgumentNullException(nameof(statusService));
    }

    public async Task<BoardDto> CreateBoardAsync(
        int projectId,
        CreateBoardRequest request,
        int userId,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        // Verify project exists and user has access
        var project = await m_Context.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project is null)
        {
            throw new NotFoundException($"Project with ID {projectId} not found");
        }

        if (project.OwnerId != userId)
        {
            throw new ForbiddenException("Only the project owner can create a board");
        }

        // Check if board already exists for this project
        var existingBoard = await m_Context.Boards
            .AsNoTracking()
            .AnyAsync(b => b.ProjectId == projectId, cancellationToken);

        if (existingBoard)
        {
            throw new BadRequestException($"A board already exists for project '{project.Name}'");
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

        m_Context.Boards.Add(board);
        await m_Context.SaveChangesAsync(cancellationToken);

        // Get all statuses and create columns
        var statuses = await m_Context.Statuses
            .OrderBy(s => s.OrderIndex)
            .ToListAsync(cancellationToken);

        var columns = statuses.Select((status, index) => new BoardColumn
        {
            BoardId = board.Id,
            StatusId = status.Id,
            OrderIndex = index,
            WIPLimit = null,
            IsCollapsed = false,
            RowVersion = Array.Empty<byte>() // Database will generate actual value
        }).ToList();

        m_Context.BoardColumns.AddRange(columns);
        await m_Context.SaveChangesAsync(cancellationToken);

        return await MapToDtoAsync(board.Id, cancellationToken);
    }

    public async Task<BoardDto?> GetBoardByProjectIdAsync(
        int projectId,
        CancellationToken cancellationToken = default)
    {
        var board = await m_Context.Boards
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.ProjectId == projectId, cancellationToken);

        return board != null ? await MapToDtoAsync(board.Id, cancellationToken) : null;
    }

    public async Task<BoardDto> UpdateBoardAsync(
        int projectId,
        UpdateBoardRequest request,
        int userId,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var board = await m_Context.Boards
            .Include(b => b.Project)
            .FirstOrDefaultAsync(b => b.ProjectId == projectId, cancellationToken);

        if (board is null)
        {
            throw new NotFoundException($"Board for project ID {projectId} not found");
        }

        // Check ownership
        if (board.Project.OwnerId != userId)
        {
            throw new ForbiddenException("Only the project owner can update the board");
        }

        board.Name = request.Name;
        board.Description = request.Description;
        board.UpdatedAt = DateTime.UtcNow;

        await m_Context.SaveChangesAsync(cancellationToken);

        return await MapToDtoAsync(board.Id, cancellationToken);
    }

    public async Task DeleteBoardAsync(
        int projectId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var board = await m_Context.Boards
            .Include(b => b.Project)
            .FirstOrDefaultAsync(b => b.ProjectId == projectId, cancellationToken);

        if (board is null)
        {
            throw new NotFoundException($"Board for project ID {projectId} not found");
        }

        // Check ownership
        if (board.Project.OwnerId != userId)
        {
            throw new ForbiddenException("Only the project owner can delete the board");
        }

        m_Context.Boards.Remove(board);
        await m_Context.SaveChangesAsync(cancellationToken);
    }

    public async Task<BoardViewDto> GetBoardViewAsync(
        int projectId,
        int[]? epicIds = null,
        int[]? assigneeIds = null,
        string? searchText = null,
        CancellationToken cancellationToken = default)
    {
        // Get board with columns
        var board = await m_Context.Boards
            .Include(b => b.BoardColumns)
            .ThenInclude(c => c.Status)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.ProjectId == projectId, cancellationToken);

        if (board is null)
        {
            throw new NotFoundException($"Board for project ID {projectId} not found");
        }

        // Build work items query
        var workItemsQuery = m_Context.WorkItems
            .Include(w => w.Assignee)
            .Where(w => w.ProjectId == projectId);

        // Apply epic filter
        if (epicIds != null && epicIds.Length > 0)
        {
            workItemsQuery = workItemsQuery.Where(w => w.EpicId.HasValue && epicIds.Contains(w.EpicId.Value));
        }

        // Apply assignee filter
        if (assigneeIds != null && assigneeIds.Length > 0)
        {
            workItemsQuery = workItemsQuery.Where(w => w.AssigneeId.HasValue && assigneeIds.Contains(w.AssigneeId.Value));
        }

        // Apply search text filter
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            workItemsQuery = workItemsQuery.Where(w =>
                w.Key.Contains(searchText) ||
                w.Summary.Contains(searchText));
        }

        // Execute query and group by status
        var workItems = await workItemsQuery
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var workItemsByStatus = workItems
            .GroupBy(w => w.StatusId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Build column DTOs
        var columns = board.BoardColumns
            .OrderBy(c => c.OrderIndex)
            .Select(column =>
            {
                var columnWorkItems = workItemsByStatus.ContainsKey(column.StatusId)
                    ? workItemsByStatus[column.StatusId]
                    : new List<WorkItem>();

                var workItemCards = columnWorkItems
                    .Select(w => new WorkItemCardDto
                    {
                        Id = w.Id,
                        Key = w.Key,
                        Summary = w.Summary,
                        AssigneeName = w.Assignee != null ? $"{w.Assignee.FirstName} {w.Assignee.LastName}" : null,
                        Type = w.Type,
                        Priority = w.Priority,
                        StatusId = w.StatusId,
                        OrderIndex = null // Will be implemented in Sprint 2.4
                    })
                    .ToList();

                return new BoardColumnViewDto
                {
                    Id = column.Id,
                    StatusId = column.StatusId,
                    StatusName = column.Status.Name,
                    StatusColor = column.Status.Color ?? "#808080",
                    OrderIndex = column.OrderIndex,
                    WIPLimit = column.WIPLimit,
                    IsCollapsed = column.IsCollapsed,
                    WorkItems = workItemCards,
                    ItemCount = workItemCards.Count
                };
            })
            .ToList();

        var totalItems = workItems.Count;

        return new BoardViewDto
        {
            BoardId = board.Id,
            ProjectId = board.ProjectId,
            Name = board.Name,
            Description = board.Description,
            Columns = columns,
            TotalItems = totalItems
        };
    }

    public async Task<BoardColumnDto> UpdateColumnAsync(
        int projectId,
        int columnId,
        UpdateBoardColumnRequest request,
        int userId,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        // Get board with ownership check
        var board = await m_Context.Boards
            .Include(b => b.Project)
            .Include(b => b.BoardColumns)
            .ThenInclude(c => c.Status)
            .FirstOrDefaultAsync(b => b.ProjectId == projectId, cancellationToken);

        if (board is null)
        {
            throw new NotFoundException($"Board for project ID {projectId} not found");
        }

        // Check ownership
        if (board.Project.OwnerId != userId)
        {
            throw new ForbiddenException("Only the project owner can update board columns");
        }

        // Find the column
        var column = board.BoardColumns.FirstOrDefault(c => c.Id == columnId);
        if (column is null)
        {
            throw new NotFoundException($"Column with ID {columnId} not found in this board");
        }

        // Update column properties
        column.WIPLimit = request.WIPLimit;
        column.IsCollapsed = request.IsCollapsed;

        await m_Context.SaveChangesAsync(cancellationToken);

        return new BoardColumnDto
        {
            Id = column.Id,
            BoardId = column.BoardId,
            StatusId = column.StatusId,
            StatusName = column.Status.Name,
            StatusColor = column.Status.Color ?? "#808080",
            OrderIndex = column.OrderIndex,
            WIPLimit = column.WIPLimit,
            IsCollapsed = column.IsCollapsed
        };
    }

    public async Task ReorderColumnsAsync(
        int projectId,
        ReorderColumnsRequest request,
        int userId,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        // Get board with ownership check
        var board = await m_Context.Boards
            .Include(b => b.Project)
            .Include(b => b.BoardColumns)
            .FirstOrDefaultAsync(b => b.ProjectId == projectId, cancellationToken);

        if (board is null)
        {
            throw new NotFoundException($"Board for project ID {projectId} not found");
        }

        // Check ownership
        if (board.Project.OwnerId != userId)
        {
            throw new ForbiddenException("Only the project owner can reorder board columns");
        }

        // Validate that all column IDs belong to this board
        var columnIds = request.ColumnIds.ToList();
        var boardColumnIds = board.BoardColumns.Select(c => c.Id).ToHashSet();

        if (columnIds.Count != boardColumnIds.Count)
        {
            throw new BadRequestException($"Column count mismatch: expected {boardColumnIds.Count}, got {columnIds.Count}");
        }

        foreach (var columnId in columnIds)
        {
            if (!boardColumnIds.Contains(columnId))
            {
                throw new BadRequestException($"Column with ID {columnId} does not belong to this board");
            }
        }

        // Update order indexes
        for (int i = 0; i < columnIds.Count; i++)
        {
            var column = board.BoardColumns.First(c => c.Id == columnIds[i]);
            column.OrderIndex = i;
        }

        await m_Context.SaveChangesAsync(cancellationToken);
    }

    public Task MoveWorkItemAsync(
        int projectId,
        int workItemId,
        int toStatusId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        // Implementation in Sprint 2.4
        throw new NotImplementedException("MoveWorkItemAsync will be implemented in Sprint 2.4");
    }

    private async Task<BoardDto> MapToDtoAsync(int boardId, CancellationToken cancellationToken = default)
    {
        var board = await m_Context.Boards
            .Include(b => b.BoardColumns)
            .ThenInclude(c => c.Status)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == boardId, cancellationToken);

        if (board is null)
        {
            throw new NotFoundException($"Board with ID {boardId} not found");
        }

        var columns = board.BoardColumns
            .OrderBy(c => c.OrderIndex)
            .Select(c => new BoardColumnDto
            {
                Id = c.Id,
                BoardId = c.BoardId,
                StatusId = c.StatusId,
                StatusName = c.Status.Name,
                StatusColor = c.Status.Color ?? "#808080",
                OrderIndex = c.OrderIndex,
                WIPLimit = c.WIPLimit,
                IsCollapsed = c.IsCollapsed
            })
            .ToList();

        return new BoardDto
        {
            Id = board.Id,
            ProjectId = board.ProjectId,
            Name = board.Name,
            Description = board.Description,
            CreatedAt = board.CreatedAt,
            UpdatedAt = board.UpdatedAt,
            Columns = columns
        };
    }
}
