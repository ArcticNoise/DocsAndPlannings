using System.Security.Claims;
using DocsAndPlannings.Core.DTOs;
using DocsAndPlannings.Core.DTOs.Epics;
using DocsAndPlannings.Core.DTOs.Projects;
using DocsAndPlannings.Core.DTOs.Statuses;
using DocsAndPlannings.Core.DTOs.WorkItems;
using DocsAndPlannings.Web.Services;
using DocsAndPlannings.Web.ViewModels.Epics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocsAndPlannings.Web.Controllers;

[Authorize]
public sealed class EpicsController : Controller
{
    private readonly IApiClient m_ApiClient;
    private readonly ILogger<EpicsController> m_Logger;

    public EpicsController(
        IApiClient apiClient,
        ILogger<EpicsController> logger)
    {
        m_ApiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IActionResult> Index(
        string? searchQuery = null,
        int? projectId = null,
        int? statusId = null,
        int? assigneeId = null)
    {
        try
        {
            List<ProjectListItemDto> projects = await m_ApiClient.GetAsync<List<ProjectListItemDto>>("/api/projects") ?? [];

            List<EpicListItemDto> allEpics = [];

            if (projectId.HasValue)
            {
                allEpics = await m_ApiClient.GetAsync<List<EpicListItemDto>>($"/api/projects/{projectId.Value}/epics") ?? [];
            }
            else
            {
                foreach (ProjectListItemDto project in projects)
                {
                    List<EpicListItemDto> projectEpics = await m_ApiClient.GetAsync<List<EpicListItemDto>>($"/api/projects/{project.Id}/epics") ?? [];
                    allEpics.AddRange(projectEpics);
                }
            }

            IEnumerable<EpicListItemDto> filteredEpics = allEpics;

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                filteredEpics = filteredEpics.Where(e =>
                    e.Summary.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                    e.Key.Contains(searchQuery, StringComparison.OrdinalIgnoreCase));
            }


            EpicListViewModel viewModel = new EpicListViewModel
            {
                SearchQuery = searchQuery,
                SelectedProjectId = projectId,
                SelectedStatusId = statusId,
                SelectedAssigneeId = assigneeId,
                Epics = filteredEpics.ToList(),
                AvailableProjects = projects
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            m_Logger.LogError(ex, "Error loading epics list");
            TempData["ErrorMessage"] = "Failed to load epics. Please try again.";
            return View(new EpicListViewModel());
        }
    }

    public async Task<IActionResult> Details(int id)
    {
        try
        {
            EpicDto? epic = await m_ApiClient.GetAsync<EpicDto>($"/api/epics/{id}");
            if (epic == null)
            {
                TempData["ErrorMessage"] = "Epic not found.";
                return RedirectToAction(nameof(Index));
            }

            WorkItemSearchRequest searchRequest = new WorkItemSearchRequest
            {
                EpicId = id,
                PageSize = 100
            };

            List<WorkItemListItemDto> workItems = await m_ApiClient.PostAsync<List<WorkItemListItemDto>>(
                "/api/workitems/search",
                searchRequest) ?? [];

            int currentUserId = GetCurrentUserId();

            EpicDetailsViewModel viewModel = new EpicDetailsViewModel
            {
                Epic = epic,
                WorkItems = workItems,
                CanEdit = true,
                CanDelete = true
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            m_Logger.LogError(ex, "Error loading epic details for epic ID {EpicId}", id);
            TempData["ErrorMessage"] = "Failed to load epic details. Please try again.";
            return RedirectToAction(nameof(Index));
        }
    }

    public async Task<IActionResult> Create(int? projectId = null)
    {
        EpicEditorViewModel viewModel = new EpicEditorViewModel
        {
            ProjectId = projectId
        };

        await LoadEditorDataAsync(viewModel);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EpicEditorViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            await LoadEditorDataAsync(viewModel);
            return View(viewModel);
        }

        try
        {
            CreateEpicRequest request = new CreateEpicRequest
            {
                ProjectId = viewModel.ProjectId!.Value,
                Summary = viewModel.Summary,
                Description = viewModel.Description,
                AssigneeId = viewModel.AssigneeId,
                Priority = viewModel.Priority,
                StartDate = viewModel.StartDate,
                DueDate = viewModel.DueDate
            };

            EpicDto? createdEpic = await m_ApiClient.PostAsync<EpicDto>("/api/epics", request);

            if (createdEpic == null)
            {
                ModelState.AddModelError(string.Empty, "Failed to create epic.");
                await LoadEditorDataAsync(viewModel);
                return View(viewModel);
            }

            TempData["SuccessMessage"] = $"Epic '{createdEpic.Key}' created successfully.";
            return RedirectToAction(nameof(Details), new { id = createdEpic.Id });
        }
        catch (Exception ex)
        {
            m_Logger.LogError(ex, "Error creating epic");
            ModelState.AddModelError(string.Empty, "An error occurred while creating the epic. Please try again.");
            await LoadEditorDataAsync(viewModel);
            return View(viewModel);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            EpicDto? epic = await m_ApiClient.GetAsync<EpicDto>($"/api/epics/{id}");
            if (epic == null)
            {
                TempData["ErrorMessage"] = "Epic not found.";
                return RedirectToAction(nameof(Index));
            }

            EpicEditorViewModel viewModel = new EpicEditorViewModel
            {
                Id = epic.Id,
                ProjectId = epic.ProjectId,
                Summary = epic.Summary,
                Description = epic.Description,
                AssigneeId = epic.AssigneeId,
                StatusId = epic.StatusId,
                Priority = epic.Priority,
                StartDate = epic.StartDate,
                DueDate = epic.DueDate
            };

            await LoadEditorDataAsync(viewModel);
            return View(viewModel);
        }
        catch (Exception ex)
        {
            m_Logger.LogError(ex, "Error loading epic for editing, epic ID {EpicId}", id);
            TempData["ErrorMessage"] = "Failed to load epic. Please try again.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EpicEditorViewModel viewModel)
    {
        if (id != viewModel.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await LoadEditorDataAsync(viewModel);
            return View(viewModel);
        }

        try
        {
            UpdateEpicRequest request = new UpdateEpicRequest
            {
                Summary = viewModel.Summary,
                Description = viewModel.Description,
                AssigneeId = viewModel.AssigneeId,
                StatusId = viewModel.StatusId!.Value,
                Priority = viewModel.Priority,
                StartDate = viewModel.StartDate,
                DueDate = viewModel.DueDate
            };

            await m_ApiClient.PutAsync<object>($"/api/epics/{id}", request);

            TempData["SuccessMessage"] = "Epic updated successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            m_Logger.LogError(ex, "Error updating epic ID {EpicId}", id);
            ModelState.AddModelError(string.Empty, "An error occurred while updating the epic. Please try again.");
            await LoadEditorDataAsync(viewModel);
            return View(viewModel);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await m_ApiClient.DeleteAsync($"/api/epics/{id}");

            TempData["SuccessMessage"] = "Epic deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            m_Logger.LogError(ex, "Error deleting epic ID {EpicId}", id);
            TempData["ErrorMessage"] = "Failed to delete epic. Please try again.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    private async Task LoadEditorDataAsync(EpicEditorViewModel viewModel)
    {
        try
        {
            viewModel.AvailableProjects = await m_ApiClient.GetAsync<List<ProjectListItemDto>>("/api/projects") ?? [];

            viewModel.AvailableStatuses = await m_ApiClient.GetAsync<List<StatusDto>>("/api/statuses") ?? [];

            viewModel.AvailableUsers = await m_ApiClient.GetAsync<List<UserDto>>("/api/users") ?? [];
        }
        catch (Exception ex)
        {
            m_Logger.LogError(ex, "Error loading editor data for epic");
        }
    }

    private int GetCurrentUserId()
    {
        string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return 0;
        }

        return userId;
    }
}
