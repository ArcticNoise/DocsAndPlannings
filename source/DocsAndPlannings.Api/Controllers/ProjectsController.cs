using System.Security.Claims;
using DocsAndPlannings.Core.DTOs.Projects;
using DocsAndPlannings.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocsAndPlannings.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ProjectsController : ControllerBase
{
    private readonly IProjectService m_ProjectService;

    public ProjectsController(IProjectService projectService)
    {
        m_ProjectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim!);
    }

    [HttpPost]
    public async Task<ActionResult<ProjectDto>> CreateProject([FromBody] CreateProjectRequest request)
    {
        var userId = GetCurrentUserId();
        var project = await m_ProjectService.CreateProjectAsync(request, userId);
        return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectDto>> GetProject(int id)
    {
        var project = await m_ProjectService.GetProjectByIdAsync(id);
        if (project is null)
        {
            return NotFound();
        }
        return Ok(project);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProjectListItemDto>>> GetProjects(
        [FromQuery] int? ownerId,
        [FromQuery] bool? isActive,
        [FromQuery] bool? isArchived,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var projects = await m_ProjectService.GetAllProjectsAsync(ownerId, isActive, isArchived, page, pageSize);
        return Ok(projects);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProjectDto>> UpdateProject(int id, [FromBody] UpdateProjectRequest request)
    {
        var userId = GetCurrentUserId();
        var project = await m_ProjectService.UpdateProjectAsync(id, request, userId);
        return Ok(project);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteProject(int id)
    {
        var userId = GetCurrentUserId();
        await m_ProjectService.DeleteProjectAsync(id, userId);
        return NoContent();
    }

    [HttpPost("{id}/archive")]
    public async Task<ActionResult<ProjectDto>> ArchiveProject(int id)
    {
        var userId = GetCurrentUserId();
        var project = await m_ProjectService.ArchiveProjectAsync(id, userId);
        return Ok(project);
    }

    [HttpPost("{id}/unarchive")]
    public async Task<ActionResult<ProjectDto>> UnarchiveProject(int id)
    {
        var userId = GetCurrentUserId();
        var project = await m_ProjectService.UnarchiveProjectAsync(id, userId);
        return Ok(project);
    }
}
