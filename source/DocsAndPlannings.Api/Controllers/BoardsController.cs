using DocsAndPlannings.Core.DTOs.Boards;
using DocsAndPlannings.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace DocsAndPlannings.Api.Controllers;

[Route("api/projects/{projectId}/board")]
public sealed class BoardsController : BaseApiController
{
    private readonly IBoardService m_BoardService;

    public BoardsController(IBoardService boardService)
    {
        m_BoardService = boardService ?? throw new ArgumentNullException(nameof(boardService));
    }

    /// <summary>
    /// Creates a new board for a project
    /// </summary>
    /// <param name="projectId">The project identifier</param>
    /// <param name="request">The create board request</param>
    /// <returns>The created board</returns>
    [HttpPost]
    public async Task<ActionResult<BoardDto>> CreateBoard(
        int projectId,
        [FromBody] CreateBoardRequest request,
        CancellationToken cancellationToken = default)
    {
        if (projectId <= 0)
        {
            return BadRequest($"Invalid project ID: {projectId}. ID must be a positive integer.");
        }

        var userId = GetCurrentUserId();
        var board = await m_BoardService.CreateBoardAsync(projectId, request, userId, cancellationToken);
        return CreatedAtAction(nameof(GetBoard), new { projectId }, board);
    }

    /// <summary>
    /// Gets a board by project identifier
    /// </summary>
    /// <param name="projectId">The project identifier</param>
    /// <returns>The board, or 404 if not found</returns>
    [HttpGet]
    public async Task<ActionResult<BoardDto>> GetBoard(
        int projectId,
        CancellationToken cancellationToken = default)
    {
        if (projectId <= 0)
        {
            return BadRequest($"Invalid project ID: {projectId}. ID must be a positive integer.");
        }

        var board = await m_BoardService.GetBoardByProjectIdAsync(projectId, cancellationToken);
        if (board is null)
        {
            return NotFound();
        }
        return Ok(board);
    }

    /// <summary>
    /// Updates a board's settings
    /// </summary>
    /// <param name="projectId">The project identifier</param>
    /// <param name="request">The update board request</param>
    /// <returns>The updated board</returns>
    [HttpPut]
    public async Task<ActionResult<BoardDto>> UpdateBoard(
        int projectId,
        [FromBody] UpdateBoardRequest request,
        CancellationToken cancellationToken = default)
    {
        if (projectId <= 0)
        {
            return BadRequest($"Invalid project ID: {projectId}. ID must be a positive integer.");
        }

        var userId = GetCurrentUserId();
        var board = await m_BoardService.UpdateBoardAsync(projectId, request, userId, cancellationToken);
        return Ok(board);
    }

    /// <summary>
    /// Deletes a board
    /// </summary>
    /// <param name="projectId">The project identifier</param>
    /// <returns>204 No Content on success</returns>
    [HttpDelete]
    public async Task<ActionResult> DeleteBoard(
        int projectId,
        CancellationToken cancellationToken = default)
    {
        if (projectId <= 0)
        {
            return BadRequest($"Invalid project ID: {projectId}. ID must be a positive integer.");
        }

        var userId = GetCurrentUserId();
        await m_BoardService.DeleteBoardAsync(projectId, userId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Gets a complete board view with work items grouped by columns
    /// </summary>
    /// <param name="projectId">The project identifier</param>
    /// <param name="epicIds">Optional epic IDs to filter by</param>
    /// <param name="assigneeIds">Optional assignee IDs to filter by</param>
    /// <param name="searchText">Optional search text for filtering</param>
    /// <returns>The complete board view</returns>
    [HttpGet("view")]
    public async Task<ActionResult<BoardViewDto>> GetBoardView(
        int projectId,
        [FromQuery] int[]? epicIds = null,
        [FromQuery] int[]? assigneeIds = null,
        [FromQuery] string? searchText = null,
        CancellationToken cancellationToken = default)
    {
        if (projectId <= 0)
        {
            return BadRequest($"Invalid project ID: {projectId}. ID must be a positive integer.");
        }

        var boardView = await m_BoardService.GetBoardViewAsync(
            projectId,
            epicIds,
            assigneeIds,
            searchText,
            cancellationToken);
        return Ok(boardView);
    }

    /// <summary>
    /// Updates a board column configuration
    /// </summary>
    /// <param name="projectId">The project identifier</param>
    /// <param name="columnId">The column identifier</param>
    /// <param name="request">The update column request</param>
    /// <returns>The updated column</returns>
    [HttpPut("columns/{columnId}")]
    public async Task<ActionResult<BoardColumnDto>> UpdateColumn(
        int projectId,
        int columnId,
        [FromBody] UpdateBoardColumnRequest request,
        CancellationToken cancellationToken = default)
    {
        if (projectId <= 0)
        {
            return BadRequest($"Invalid project ID: {projectId}. ID must be a positive integer.");
        }

        if (columnId <= 0)
        {
            return BadRequest($"Invalid column ID: {columnId}. ID must be a positive integer.");
        }

        var userId = GetCurrentUserId();
        var column = await m_BoardService.UpdateColumnAsync(projectId, columnId, request, userId, cancellationToken);
        return Ok(column);
    }

    /// <summary>
    /// Reorders board columns
    /// </summary>
    /// <param name="projectId">The project identifier</param>
    /// <param name="request">The reorder columns request</param>
    /// <returns>204 No Content on success</returns>
    [HttpPut("columns/reorder")]
    public async Task<ActionResult> ReorderColumns(
        int projectId,
        [FromBody] ReorderColumnsRequest request,
        CancellationToken cancellationToken = default)
    {
        if (projectId <= 0)
        {
            return BadRequest($"Invalid project ID: {projectId}. ID must be a positive integer.");
        }

        var userId = GetCurrentUserId();
        await m_BoardService.ReorderColumnsAsync(projectId, request, userId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Moves a work item to a different status
    /// </summary>
    /// <param name="projectId">The project identifier</param>
    /// <param name="workItemId">The work item identifier</param>
    /// <param name="toStatusId">The target status identifier</param>
    /// <returns>204 No Content on success</returns>
    [HttpPut("workitems/{workItemId}/move")]
    public async Task<ActionResult> MoveWorkItem(
        int projectId,
        int workItemId,
        [FromQuery] int toStatusId,
        CancellationToken cancellationToken = default)
    {
        if (projectId <= 0)
        {
            return BadRequest($"Invalid project ID: {projectId}. ID must be a positive integer.");
        }

        if (workItemId <= 0)
        {
            return BadRequest($"Invalid work item ID: {workItemId}. ID must be a positive integer.");
        }

        if (toStatusId <= 0)
        {
            return BadRequest($"Invalid status ID: {toStatusId}. ID must be a positive integer.");
        }

        var userId = GetCurrentUserId();
        await m_BoardService.MoveWorkItemAsync(projectId, workItemId, toStatusId, userId, cancellationToken);
        return NoContent();
    }
}
