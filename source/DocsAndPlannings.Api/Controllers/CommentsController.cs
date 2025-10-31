using System.Security.Claims;
using DocsAndPlannings.Core.DTOs.Comments;
using DocsAndPlannings.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocsAndPlannings.Api.Controllers;

[Route("api/[controller]")]
public sealed class CommentsController : BaseApiController
{
    private readonly ICommentService m_CommentService;

    public CommentsController(ICommentService commentService)
    {
        m_CommentService = commentService ?? throw new ArgumentNullException(nameof(commentService));
    }

    [HttpPost("workitem/{workItemId}")]
    public async Task<ActionResult<CommentDto>> CreateComment(int workItemId, [FromBody] CreateCommentRequest request)
    {
        if (workItemId <= 0)
        {
            return BadRequest($"Invalid work item ID: {workItemId}. ID must be a positive integer.");
        }

        var userId = GetCurrentUserId();
        var comment = await m_CommentService.CreateCommentAsync(workItemId, request, userId);
        return CreatedAtAction(nameof(GetComment), new { id = comment.Id }, comment);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CommentDto>> GetComment(int id)
    {
        if (id <= 0)
        {
            return BadRequest($"Invalid comment ID: {id}. ID must be a positive integer.");
        }

        var comment = await m_CommentService.GetCommentByIdAsync(id);
        if (comment is null)
        {
            return NotFound();
        }
        return Ok(comment);
    }

    [HttpGet("workitem/{workItemId}")]
    public async Task<ActionResult<IReadOnlyList<CommentDto>>> GetCommentsByWorkItem(int workItemId)
    {
        if (workItemId <= 0)
        {
            return BadRequest($"Invalid work item ID: {workItemId}. ID must be a positive integer.");
        }

        var comments = await m_CommentService.GetCommentsByWorkItemIdAsync(workItemId);
        return Ok(comments);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CommentDto>> UpdateComment(int id, [FromBody] UpdateCommentRequest request)
    {
        if (id <= 0)
        {
            return BadRequest($"Invalid comment ID: {id}. ID must be a positive integer.");
        }

        var userId = GetCurrentUserId();
        var comment = await m_CommentService.UpdateCommentAsync(id, request, userId);
        return Ok(comment);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteComment(int id)
    {
        if (id <= 0)
        {
            return BadRequest($"Invalid comment ID: {id}. ID must be a positive integer.");
        }

        var userId = GetCurrentUserId();
        await m_CommentService.DeleteCommentAsync(id, userId);
        return NoContent();
    }
}
