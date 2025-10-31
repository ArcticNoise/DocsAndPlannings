using System.Security.Claims;
using DocsAndPlannings.Core.DTOs.Epics;
using DocsAndPlannings.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocsAndPlannings.Api.Controllers;

[Route("api/[controller]")]
public sealed class EpicsController : BaseApiController
{
    private readonly IEpicService m_EpicService;

    public EpicsController(IEpicService epicService)
    {
        m_EpicService = epicService ?? throw new ArgumentNullException(nameof(epicService));
    }

    [HttpPost]
    public async Task<ActionResult<EpicDto>> CreateEpic([FromBody] CreateEpicRequest request)
    {
        var userId = GetCurrentUserId();
        var epic = await m_EpicService.CreateEpicAsync(request, userId);
        return CreatedAtAction(nameof(GetEpic), new { id = epic.Id }, epic);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EpicDto>> GetEpic(int id)
    {
        if (id <= 0)
        {
            return BadRequest($"Invalid epic ID: {id}. ID must be a positive integer.");
        }

        var epic = await m_EpicService.GetEpicByIdAsync(id);
        if (epic is null)
        {
            return NotFound();
        }
        return Ok(epic);
    }

    [HttpGet("key/{key}")]
    public async Task<ActionResult<EpicDto>> GetEpicByKey(string key)
    {
        var epic = await m_EpicService.GetEpicByKeyAsync(key);
        if (epic is null)
        {
            return NotFound();
        }
        return Ok(epic);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EpicListItemDto>>> GetEpics(
        [FromQuery] int? projectId,
        [FromQuery] int? statusId,
        [FromQuery] int? assigneeId,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        if (projectId.HasValue && projectId.Value <= 0)
        {
            return BadRequest($"Invalid project ID: {projectId.Value}. ID must be a positive integer.");
        }

        if (statusId.HasValue && statusId.Value <= 0)
        {
            return BadRequest($"Invalid status ID: {statusId.Value}. ID must be a positive integer.");
        }

        if (assigneeId.HasValue && assigneeId.Value <= 0)
        {
            return BadRequest($"Invalid assignee ID: {assigneeId.Value}. ID must be a positive integer.");
        }

        var epics = await m_EpicService.GetAllEpicsAsync(projectId, statusId, assigneeId, isActive, page, pageSize);
        return Ok(epics);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<EpicDto>> UpdateEpic(int id, [FromBody] UpdateEpicRequest request)
    {
        if (id <= 0)
        {
            return BadRequest($"Invalid epic ID: {id}. ID must be a positive integer.");
        }

        var userId = GetCurrentUserId();
        var epic = await m_EpicService.UpdateEpicAsync(id, request, userId);
        return Ok(epic);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteEpic(int id)
    {
        if (id <= 0)
        {
            return BadRequest($"Invalid epic ID: {id}. ID must be a positive integer.");
        }

        var userId = GetCurrentUserId();
        await m_EpicService.DeleteEpicAsync(id, userId);
        return NoContent();
    }

    [HttpPut("{id}/assign")]
    public async Task<ActionResult<EpicDto>> AssignEpic(int id, [FromBody] int? assigneeId)
    {
        if (id <= 0)
        {
            return BadRequest($"Invalid epic ID: {id}. ID must be a positive integer.");
        }

        if (assigneeId.HasValue && assigneeId.Value <= 0)
        {
            return BadRequest($"Invalid assignee ID: {assigneeId.Value}. ID must be a positive integer.");
        }

        var userId = GetCurrentUserId();
        var epic = await m_EpicService.AssignEpicAsync(id, assigneeId, userId);
        return Ok(epic);
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<EpicDto>> UpdateEpicStatus(int id, [FromBody] int statusId)
    {
        if (id <= 0)
        {
            return BadRequest($"Invalid epic ID: {id}. ID must be a positive integer.");
        }

        if (statusId <= 0)
        {
            return BadRequest($"Invalid status ID: {statusId}. ID must be a positive integer.");
        }

        var userId = GetCurrentUserId();
        var epic = await m_EpicService.UpdateEpicStatusAsync(id, statusId, userId);
        return Ok(epic);
    }
}
