using System.Security.Claims;
using DocsAndPlannings.Core.DTOs.Epics;
using DocsAndPlannings.Core.DTOs.Projects;
using DocsAndPlannings.Core.DTOs.WorkItems;
using DocsAndPlannings.Web.Services;
using DocsAndPlannings.Web.ViewModels.Projects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocsAndPlannings.Web.Controllers;

[Authorize]
public sealed class ProjectsController : Controller
{
    private readonly IApiClient m_ApiClient;
    private readonly ILogger<ProjectsController> m_Logger;

    public ProjectsController(
        IApiClient apiClient,
        ILogger<ProjectsController> logger)
    {
        m_ApiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IActionResult> Index(
        string? searchQuery = null,
        bool activeOnly = true,
        bool showArchived = false)
    {
        try
        {
            List<ProjectListItemDto> allProjects = await m_ApiClient.GetAsync<List<ProjectListItemDto>>("/api/projects") ?? [];

            IEnumerable<ProjectListItemDto> filteredProjects = allProjects;

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                filteredProjects = filteredProjects.Where(p =>
                    p.Name.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                    p.Key.Contains(searchQuery, StringComparison.OrdinalIgnoreCase));
            }

            if (activeOnly)
            {
                filteredProjects = filteredProjects.Where(p => p.IsActive);
            }

            if (!showArchived)
            {
                filteredProjects = filteredProjects.Where(p => !p.IsArchived);
            }

            ProjectListViewModel viewModel = new ProjectListViewModel
            {
                SearchQuery = searchQuery,
                ActiveOnly = activeOnly,
                ShowArchived = showArchived,
                Projects = filteredProjects.ToList()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            m_Logger.LogError(ex, "Error loading projects list");
            TempData["ErrorMessage"] = "Failed to load projects. Please try again.";
            return View(new ProjectListViewModel());
        }
    }

    public async Task<IActionResult> Details(int id)
    {
        try
        {
            ProjectDto? project = await m_ApiClient.GetAsync<ProjectDto>($"/api/projects/{id}");
            if (project == null)
            {
                TempData["ErrorMessage"] = "Project not found.";
                return RedirectToAction(nameof(Index));
            }

            List<EpicListItemDto> epics = await m_ApiClient.GetAsync<List<EpicListItemDto>>($"/api/projects/{id}/epics") ?? [];

            WorkItemSearchRequest searchRequest = new WorkItemSearchRequest
            {
                ProjectId = id,
                PageSize = 10
            };

            List<WorkItemListItemDto> recentWorkItems = await m_ApiClient.PostAsync<List<WorkItemListItemDto>>(
                "/api/workitems/search",
                searchRequest) ?? [];

            int currentUserId = GetCurrentUserId();

            ProjectDetailsViewModel viewModel = new ProjectDetailsViewModel
            {
                Project = project,
                Epics = epics,
                RecentWorkItems = recentWorkItems,
                CanEdit = project.OwnerId == currentUserId,
                CanDelete = project.OwnerId == currentUserId
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            m_Logger.LogError(ex, "Error loading project details for project ID {ProjectId}", id);
            TempData["ErrorMessage"] = "Failed to load project details. Please try again.";
            return RedirectToAction(nameof(Index));
        }
    }

    public IActionResult Create()
    {
        ProjectEditorViewModel viewModel = new ProjectEditorViewModel();
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProjectEditorViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        try
        {
            CreateProjectRequest request = new CreateProjectRequest
            {
                Key = viewModel.Key.ToUpperInvariant(),
                Name = viewModel.Name,
                Description = viewModel.Description
            };

            ProjectDto? createdProject = await m_ApiClient.PostAsync<ProjectDto>("/api/projects", request);

            if (createdProject == null)
            {
                ModelState.AddModelError(string.Empty, "Failed to create project.");
                return View(viewModel);
            }

            TempData["SuccessMessage"] = $"Project '{createdProject.Name}' created successfully.";
            return RedirectToAction(nameof(Details), new { id = createdProject.Id });
        }
        catch (Exception ex)
        {
            m_Logger.LogError(ex, "Error creating project");
            ModelState.AddModelError(string.Empty, "An error occurred while creating the project. Please try again.");
            return View(viewModel);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            ProjectDto? project = await m_ApiClient.GetAsync<ProjectDto>($"/api/projects/{id}");
            if (project == null)
            {
                TempData["ErrorMessage"] = "Project not found.";
                return RedirectToAction(nameof(Index));
            }

            int currentUserId = GetCurrentUserId();
            if (project.OwnerId != currentUserId)
            {
                TempData["ErrorMessage"] = "You do not have permission to edit this project.";
                return RedirectToAction(nameof(Details), new { id });
            }

            ProjectEditorViewModel viewModel = new ProjectEditorViewModel
            {
                Id = project.Id,
                Key = project.Key,
                Name = project.Name,
                Description = project.Description,
                IsActive = project.IsActive,
                IsArchived = project.IsArchived
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            m_Logger.LogError(ex, "Error loading project for editing, project ID {ProjectId}", id);
            TempData["ErrorMessage"] = "Failed to load project. Please try again.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProjectEditorViewModel viewModel)
    {
        if (id != viewModel.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        try
        {
            UpdateProjectRequest request = new UpdateProjectRequest
            {
                Name = viewModel.Name,
                Description = viewModel.Description,
                IsActive = viewModel.IsActive
            };

            await m_ApiClient.PutAsync<object>($"/api/projects/{id}", request);

            TempData["SuccessMessage"] = "Project updated successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            m_Logger.LogError(ex, "Error updating project ID {ProjectId}", id);
            ModelState.AddModelError(string.Empty, "An error occurred while updating the project. Please try again.");
            return View(viewModel);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await m_ApiClient.DeleteAsync($"/api/projects/{id}");

            TempData["SuccessMessage"] = "Project deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            m_Logger.LogError(ex, "Error deleting project ID {ProjectId}", id);
            TempData["ErrorMessage"] = "Failed to delete project. Please try again.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Archive(int id)
    {
        try
        {
            await m_ApiClient.PostAsync<object>($"/api/projects/{id}/archive", new { });

            TempData["SuccessMessage"] = "Project archived successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            m_Logger.LogError(ex, "Error archiving project ID {ProjectId}", id);
            TempData["ErrorMessage"] = "Failed to archive project. Please try again.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unarchive(int id)
    {
        try
        {
            await m_ApiClient.PostAsync<object>($"/api/projects/{id}/unarchive", new { });

            TempData["SuccessMessage"] = "Project unarchived successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            m_Logger.LogError(ex, "Error unarchiving project ID {ProjectId}", id);
            TempData["ErrorMessage"] = "Failed to unarchive project. Please try again.";
            return RedirectToAction(nameof(Details), new { id });
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
