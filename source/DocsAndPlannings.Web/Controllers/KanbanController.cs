using System.Security.Claims;
using DocsAndPlannings.Core.DTOs;
using DocsAndPlannings.Core.DTOs.Boards;
using DocsAndPlannings.Core.DTOs.Epics;
using DocsAndPlannings.Core.DTOs.Projects;
using DocsAndPlannings.Core.DTOs.Statuses;
using DocsAndPlannings.Core.DTOs.WorkItems;
using DocsAndPlannings.Web.Services;
using DocsAndPlannings.Web.ViewModels.Kanban;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocsAndPlannings.Web.Controllers;

[Authorize]
public sealed class KanbanController : Controller
{
    private readonly IApiClient m_ApiClient;
    private readonly ILogger<KanbanController> m_Logger;

    public KanbanController(
        IApiClient apiClient,
        ILogger<KanbanController> logger)
    {
        m_ApiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Display the kanban board for a project
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(
        int projectId,
        string? searchText = null,
        int? epicId = null,
        int? assigneeId = null)
    {
        try
        {
            ProjectDto? project = await m_ApiClient.GetAsync<ProjectDto>($"/api/projects/{projectId}");
            if (project == null)
            {
                TempData["ErrorMessage"] = "Project not found.";
                return RedirectToAction("Index", "Projects");
            }

            BoardViewDto? board = null;
            List<string> queryParams = new List<string>();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                queryParams.Add($"searchText={Uri.EscapeDataString(searchText)}");
            }
            if (epicId.HasValue)
            {
                queryParams.Add($"epicId={epicId.Value}");
            }
            if (assigneeId.HasValue)
            {
                queryParams.Add($"assigneeId={assigneeId.Value}");
            }

            string queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : string.Empty;
            board = await m_ApiClient.GetAsync<BoardViewDto>($"/api/projects/{projectId}/board/view{queryString}");

            List<EpicListItemDto> epics = await m_ApiClient.GetAsync<List<EpicListItemDto>>($"/api/projects/{projectId}/epics") ?? [];
            List<UserDto> users = await m_ApiClient.GetAsync<List<UserDto>>("/api/users") ?? [];

            KanbanBoardViewModel viewModel = new KanbanBoardViewModel
            {
                ProjectId = project.Id,
                ProjectName = project.Name,
                ProjectKey = project.Key,
                Board = board,
                SearchText = searchText,
                SelectedEpicId = epicId,
                SelectedAssigneeId = assigneeId,
                AvailableEpics = epics,
                AvailableUsers = users
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            m_Logger.LogError(ex, "Error loading kanban board for project {ProjectId}", projectId);
            TempData["ErrorMessage"] = "An error occurred while loading the kanban board.";
            return RedirectToAction("Index", "Projects");
        }
    }

    /// <summary>
    /// Move a work item to a different status (column)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> MoveWorkItem(int projectId, int workItemId, [FromBody] MoveWorkItemRequest request)
    {
        try
        {
            await m_ApiClient.PutAsync<object>($"/api/projects/{projectId}/board/workitems/{workItemId}/move", request);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            m_Logger.LogError(ex, "Error moving work item {WorkItemId} to status {StatusId}", workItemId, request.ToStatusId);
            return Json(new { success = false, message = "Failed to move work item. " + ex.Message });
        }
    }

    /// <summary>
    /// Update column configuration (WIP limit, collapsed state)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> UpdateColumn(int projectId, int columnId, [FromBody] UpdateColumnViewModel model)
    {
        try
        {
            UpdateBoardColumnRequest request = new UpdateBoardColumnRequest
            {
                WIPLimit = model.WIPLimit,
                IsCollapsed = model.IsCollapsed
            };

            await m_ApiClient.PutAsync<object>($"/api/projects/{projectId}/board/columns/{columnId}", request);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            m_Logger.LogError(ex, "Error updating column {ColumnId}", columnId);
            return Json(new { success = false, message = "Failed to update column. " + ex.Message });
        }
    }

    /// <summary>
    /// Display board settings page
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Settings(int projectId)
    {
        try
        {
            ProjectDto? project = await m_ApiClient.GetAsync<ProjectDto>($"/api/projects/{projectId}");
            if (project == null)
            {
                TempData["ErrorMessage"] = "Project not found.";
                return RedirectToAction("Index", "Projects");
            }

            BoardDto? board = await m_ApiClient.GetAsync<BoardDto>($"/api/projects/{projectId}/board");
            List<StatusDto> statuses = await m_ApiClient.GetAsync<List<StatusDto>>("/api/statuses") ?? [];

            BoardSettingsViewModel viewModel = new BoardSettingsViewModel
            {
                Id = board?.Id,
                ProjectId = project.Id,
                ProjectName = project.Name,
                ProjectKey = project.Key,
                Name = board?.Name ?? $"{project.Name} Board",
                Description = board?.Description,
                AvailableStatuses = statuses,
                Columns = board?.Columns ?? Array.Empty<BoardColumnDto>()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            m_Logger.LogError(ex, "Error loading board settings for project {ProjectId}", projectId);
            TempData["ErrorMessage"] = "An error occurred while loading board settings.";
            return RedirectToAction("Index", "Projects");
        }
    }

    /// <summary>
    /// Create or update board configuration
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveSettings(BoardSettingsViewModel model)
    {
        if (!ModelState.IsValid)
        {
            List<StatusDto> statuses = await m_ApiClient.GetAsync<List<StatusDto>>("/api/statuses") ?? [];
            BoardDto? board = model.Id.HasValue
                ? await m_ApiClient.GetAsync<BoardDto>($"/api/projects/{model.ProjectId}/board")
                : null;

            model.AvailableStatuses = statuses;
            model.Columns = board?.Columns ?? Array.Empty<BoardColumnDto>();
            return View("Settings", model);
        }

        try
        {
            if (model.IsEdit)
            {
                UpdateBoardRequest request = new UpdateBoardRequest
                {
                    Name = model.Name,
                    Description = model.Description
                };

                await m_ApiClient.PutAsync<object>($"/api/projects/{model.ProjectId}/board", request);
                TempData["SuccessMessage"] = "Board settings updated successfully.";
            }
            else
            {
                CreateBoardRequest request = new CreateBoardRequest
                {
                    Name = model.Name,
                    Description = model.Description
                };

                await m_ApiClient.PostAsync<object>($"/api/projects/{model.ProjectId}/board", request);
                TempData["SuccessMessage"] = "Board created successfully.";
            }

            return RedirectToAction(nameof(Index), new { projectId = model.ProjectId });
        }
        catch (Exception ex)
        {
            m_Logger.LogError(ex, "Error saving board settings for project {ProjectId}", model.ProjectId);
            ModelState.AddModelError(string.Empty, "An error occurred while saving board settings.");

            List<StatusDto> statuses = await m_ApiClient.GetAsync<List<StatusDto>>("/api/statuses") ?? [];
            BoardDto? board = model.Id.HasValue
                ? await m_ApiClient.GetAsync<BoardDto>($"/api/projects/{model.ProjectId}/board")
                : null;

            model.AvailableStatuses = statuses;
            model.Columns = board?.Columns ?? Array.Empty<BoardColumnDto>();
            return View("Settings", model);
        }
    }

    /// <summary>
    /// Delete board configuration
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteBoard(int projectId)
    {
        try
        {
            await m_ApiClient.DeleteAsync($"/api/projects/{projectId}/board");
            TempData["SuccessMessage"] = "Board deleted successfully.";
            return RedirectToAction("Details", "Projects", new { id = projectId });
        }
        catch (Exception ex)
        {
            m_Logger.LogError(ex, "Error deleting board for project {ProjectId}", projectId);
            TempData["ErrorMessage"] = "An error occurred while deleting the board.";
            return RedirectToAction(nameof(Settings), new { projectId });
        }
    }

    /// <summary>
    /// Get work item details for quick edit modal
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetWorkItemForEdit(int id)
    {
        try
        {
            WorkItemDto? workItem = await m_ApiClient.GetAsync<WorkItemDto>($"/api/workitems/{id}");
            if (workItem == null)
            {
                return NotFound();
            }

            List<StatusDto> statuses = await m_ApiClient.GetAsync<List<StatusDto>>("/api/statuses") ?? [];
            List<UserDto> users = await m_ApiClient.GetAsync<List<UserDto>>("/api/users") ?? [];

            QuickEditWorkItemViewModel viewModel = new QuickEditWorkItemViewModel
            {
                Id = workItem.Id,
                Key = workItem.Key,
                Summary = workItem.Summary,
                StatusId = workItem.StatusId,
                AssigneeId = workItem.AssigneeId,
                Priority = workItem.Priority,
                AvailableStatuses = statuses,
                AvailableUsers = users
            };

            return Json(viewModel);
        }
        catch (Exception ex)
        {
            m_Logger.LogError(ex, "Error loading work item {Id} for edit", id);
            return StatusCode(500, new { message = "An error occurred while loading the work item." });
        }
    }

    /// <summary>
    /// Quick update work item from kanban board
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> QuickUpdateWorkItem([FromBody] QuickEditWorkItemViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { success = false, message = "Invalid data", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        try
        {
            UpdateWorkItemRequest request = new UpdateWorkItemRequest
            {
                Summary = model.Summary,
                StatusId = model.StatusId,
                AssigneeId = model.AssigneeId,
                Priority = model.Priority
            };

            await m_ApiClient.PutAsync<object>($"/api/workitems/{model.Id}", request);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            m_Logger.LogError(ex, "Error updating work item {Id}", model.Id);
            return Json(new { success = false, message = "Failed to update work item. " + ex.Message });
        }
    }

    private int GetCurrentUserId()
    {
        string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            throw new InvalidOperationException("User ID not found in claims");
        }
        return userId;
    }
}
