using DocsAndPlannings.Core.DTOs.Boards;

namespace DocsAndPlannings.Core.Services;

/// <summary>
/// Service interface for managing Kanban boards
/// </summary>
public interface IBoardService
{
    /// <summary>
    /// Creates a new board for a project
    /// </summary>
    /// <param name="projectId">The project identifier</param>
    /// <param name="request">The create board request</param>
    /// <param name="userId">The user identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created board</returns>
    Task<BoardDto> CreateBoardAsync(int projectId, CreateBoardRequest request, int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a board by project identifier
    /// </summary>
    /// <param name="projectId">The project identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The board, or null if not found</returns>
    Task<BoardDto?> GetBoardByProjectIdAsync(int projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a board's settings
    /// </summary>
    /// <param name="projectId">The project identifier</param>
    /// <param name="request">The update board request</param>
    /// <param name="userId">The user identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated board</returns>
    Task<BoardDto> UpdateBoardAsync(int projectId, UpdateBoardRequest request, int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a board
    /// </summary>
    /// <param name="projectId">The project identifier</param>
    /// <param name="userId">The user identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task DeleteBoardAsync(int projectId, int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a complete board view with work items grouped by columns
    /// </summary>
    /// <param name="projectId">The project identifier</param>
    /// <param name="epicIds">Optional epic IDs to filter by</param>
    /// <param name="assigneeIds">Optional assignee IDs to filter by</param>
    /// <param name="searchText">Optional search text for filtering</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The complete board view</returns>
    Task<BoardViewDto> GetBoardViewAsync(
        int projectId,
        int[]? epicIds = null,
        int[]? assigneeIds = null,
        string? searchText = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a board column configuration
    /// </summary>
    /// <param name="projectId">The project identifier</param>
    /// <param name="columnId">The column identifier</param>
    /// <param name="request">The update column request</param>
    /// <param name="userId">The user identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated column</returns>
    Task<BoardColumnDto> UpdateColumnAsync(int projectId, int columnId, UpdateBoardColumnRequest request, int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reorders board columns
    /// </summary>
    /// <param name="projectId">The project identifier</param>
    /// <param name="request">The reorder columns request</param>
    /// <param name="userId">The user identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task ReorderColumnsAsync(int projectId, ReorderColumnsRequest request, int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Moves a work item to a different status
    /// </summary>
    /// <param name="projectId">The project identifier</param>
    /// <param name="workItemId">The work item identifier</param>
    /// <param name="toStatusId">The target status identifier</param>
    /// <param name="userId">The user identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task MoveWorkItemAsync(int projectId, int workItemId, int toStatusId, int userId, CancellationToken cancellationToken = default);
}
