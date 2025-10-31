using DocsAndPlannings.Core.DTOs.Statuses;
using DocsAndPlannings.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocsAndPlannings.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class StatusesController : ControllerBase
{
    private readonly IStatusService m_StatusService;

    public StatusesController(IStatusService statusService)
    {
        m_StatusService = statusService ?? throw new ArgumentNullException(nameof(statusService));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StatusDto>>> GetAllStatuses()
    {
        var statuses = await m_StatusService.GetAllStatusesAsync();
        return Ok(statuses);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StatusDto>> GetStatus(int id)
    {
        var status = await m_StatusService.GetStatusByIdAsync(id);
        if (status is null)
        {
            return NotFound();
        }
        return Ok(status);
    }

    [HttpPost]
    public async Task<ActionResult<StatusDto>> CreateStatus([FromBody] CreateStatusRequest request)
    {
        var status = await m_StatusService.CreateStatusAsync(request);
        return CreatedAtAction(nameof(GetStatus), new { id = status.Id }, status);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<StatusDto>> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        var status = await m_StatusService.UpdateStatusAsync(id, request);
        return Ok(status);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteStatus(int id)
    {
        await m_StatusService.DeleteStatusAsync(id);
        return NoContent();
    }

    [HttpGet("{id}/transitions")]
    public async Task<ActionResult<IReadOnlyList<StatusDto>>> GetAllowedTransitions(int id)
    {
        var transitions = await m_StatusService.GetAllowedTransitionsAsync(id);
        return Ok(transitions);
    }

    [HttpPost("transitions")]
    public async Task<ActionResult<StatusTransitionDto>> CreateStatusTransition([FromBody] CreateStatusTransitionRequest request)
    {
        var transition = await m_StatusService.CreateStatusTransitionAsync(request);
        return Ok(transition);
    }
}
