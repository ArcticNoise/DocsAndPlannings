using System.Security.Claims;
using DocsAndPlannings.Core.DTOs.WorkItems;
using DocsAndPlannings.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocsAndPlannings.Api.Controllers;

[Route("api/[controller]")]
public sealed class WorkItemsController : BaseApiController
{
    private readonly IWorkItemService m_WorkItemService;

    public WorkItemsController(IWorkItemService workItemService)
    {
        m_WorkItemService = workItemService ?? throw new ArgumentNullException(nameof(workItemService));
    }

    [HttpPost]
    public async Task<ActionResult<WorkItemDto>> CreateWorkItem([FromBody] CreateWorkItemRequest request)
    {
        var userId = GetCurrentUserId();
        var workItem = await m_WorkItemService.CreateWorkItemAsync(request, userId);
        return CreatedAtAction(nameof(GetWorkItem), new { id = workItem.Id }, workItem);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<WorkItemDto>> GetWorkItem(int id)
    {
        var workItem = await m_WorkItemService.GetWorkItemByIdAsync(id);
        if (workItem is null)
        {
            return NotFound();
        }
        return Ok(workItem);
    }

    [HttpGet("key/{key}")]
    public async Task<ActionResult<WorkItemDto>> GetWorkItemByKey(string key)
    {
        var workItem = await m_WorkItemService.GetWorkItemByKeyAsync(key);
        if (workItem is null)
        {
            return NotFound();
        }
        return Ok(workItem);
    }

    [HttpPost("search")]
    public async Task<ActionResult<IReadOnlyList<WorkItemListItemDto>>> SearchWorkItems([FromBody] WorkItemSearchRequest request)
    {
        var workItems = await m_WorkItemService.SearchWorkItemsAsync(request);
        return Ok(workItems);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<WorkItemDto>> UpdateWorkItem(int id, [FromBody] UpdateWorkItemRequest request)
    {
        var userId = GetCurrentUserId();
        var workItem = await m_WorkItemService.UpdateWorkItemAsync(id, request, userId);
        return Ok(workItem);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteWorkItem(int id)
    {
        var userId = GetCurrentUserId();
        await m_WorkItemService.DeleteWorkItemAsync(id, userId);
        return NoContent();
    }

    [HttpPut("{id}/assign")]
    public async Task<ActionResult<WorkItemDto>> AssignWorkItem(int id, [FromBody] int? assigneeId)
    {
        var userId = GetCurrentUserId();
        var workItem = await m_WorkItemService.AssignWorkItemAsync(id, assigneeId, userId);
        return Ok(workItem);
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<WorkItemDto>> UpdateWorkItemStatus(int id, [FromBody] int statusId)
    {
        var userId = GetCurrentUserId();
        var workItem = await m_WorkItemService.UpdateWorkItemStatusAsync(id, statusId, userId);
        return Ok(workItem);
    }

    [HttpPut("{id}/parent")]
    public async Task<ActionResult<WorkItemDto>> UpdateWorkItemParent(int id, [FromBody] int? parentWorkItemId)
    {
        var userId = GetCurrentUserId();
        var workItem = await m_WorkItemService.UpdateWorkItemParentAsync(id, parentWorkItemId, userId);
        return Ok(workItem);
    }
}
