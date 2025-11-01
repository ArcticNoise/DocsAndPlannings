using System.Security.Claims;
using DocsAndPlannings.Core.DTOs;
using DocsAndPlannings.Core.DTOs.Epics;
using DocsAndPlannings.Core.DTOs.Projects;
using DocsAndPlannings.Core.DTOs.Statuses;
using DocsAndPlannings.Core.DTOs.WorkItems;
using DocsAndPlannings.Core.Models;
using DocsAndPlannings.Web.Services;
using DocsAndPlannings.Web.ViewModels.WorkItems;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocsAndPlannings.Web.Controllers;

[Authorize]
public sealed class WorkItemsController : Controller
{
    private readonly IApiClient m_ApiClient;
    private readonly ILogger<WorkItemsController> m_Logger;

    public WorkItemsController(
        IApiClient apiClient,
        ILogger<WorkItemsController> logger)
    {
        m_ApiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IActionResult> Index(
        string? searchText = null,
        int? projectId = null,
        int? epicId = null,
        WorkItemType? type = null,
        int? statusId = null,
        int? assigneeId = null,
        int? reporterId = null,
        int? priority = null,
        int pageNumber = 1,
        int pageSize = 50)
    {
        try
        {
            WorkItemSearchRequest searchRequest = new WorkItemSearchRequest
            {
                SearchText = searchText,
                ProjectId = projectId,
                EpicId = epicId,
                Type = type,
                StatusId = statusId,
                AssigneeId = assigneeId,
                ReporterId = reporterId,
                Priority = priority,
                Page = pageNumber,
                PageSize = pageSize
            };

            List<WorkItemListItemDto> workItems = await m_ApiClient.PostAsync<List<WorkItemListItemDto>>(
                "/api/workitems/search",
                searchRequest) ?? [];

            List<ProjectListItemDto> projects = await m_ApiClient.GetAsync<List<ProjectListItemDto>>("/api/projects") ?? [];

            WorkItemListViewModel viewModel = new WorkItemListViewModel
            {
                SearchText = searchText,
                SelectedProjectId = projectId,
                SelectedEpicId = epicId,
                SelectedType = type,
                SelectedStatusId = statusId,
                SelectedAssigneeId = assigneeId,
                SelectedReporterId = reporterId,
                SelectedPriority = priority,
                PageNumber = pageNumber,
                PageSize = pageSize,
                WorkItems = workItems,
                TotalCount = workItems.Count,
                AvailableProjects = projects
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            m_Logger.LogError(ex, "Error loading work items list");
            TempData["ErrorMessage"] = "Failed to load work items. Please try again.";
            return View(new WorkItemListViewModel());
        }
    }

    public async Task<IActionResult> Details(int id)
    {
        try
        {
            WorkItemDto? workItem = await m_ApiClient.GetAsync<WorkItemDto>($"/api/workitems/{id}");
            if (workItem == null)
            {
                TempData["ErrorMessage"] = "Work item not found.";
                return RedirectToAction(nameof(Index));
            }

            WorkItemSearchRequest childSearchRequest = new WorkItemSearchRequest
            {
                ProjectId = workItem.ProjectId,
                PageSize = 100
            };

            List<WorkItemListItemDto> allProjectWorkItems = await m_ApiClient.PostAsync<List<WorkItemListItemDto>>(
                "/api/workitems/search",
                childSearchRequest) ?? [];

            List<WorkItemListItemDto> childWorkItems = [];

            List<WorkItemCommentDto> comments = await m_ApiClient.GetAsync<List<WorkItemCommentDto>>($"/api/workitems/{id}/comments") ?? [];

            WorkItemDetailsViewModel viewModel = new WorkItemDetailsViewModel
            {
                WorkItem = workItem,
                ChildWorkItems = childWorkItems,
                Comments = comments,
                RenderedDescription = RenderMarkdown(workItem.Description ?? string.Empty),
                CanEdit = true,
                CanDelete = true,
                CanComment = true
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            m_Logger.LogError(ex, "Error loading work item details for work item ID {WorkItemId}", id);
            TempData["ErrorMessage"] = "Failed to load work item details. Please try again.";
            return RedirectToAction(nameof(Index));
        }
    }

    public async Task<IActionResult> Create(int? projectId = null, int? epicId = null, WorkItemType? type = null, int? parentWorkItemId = null)
    {
        WorkItemEditorViewModel viewModel = new WorkItemEditorViewModel
        {
            ProjectId = projectId,
            EpicId = epicId,
            Type = type,
            ParentWorkItemId = parentWorkItemId,
            ReporterId = GetCurrentUserId()
        };

        await LoadEditorDataAsync(viewModel);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(WorkItemEditorViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            await LoadEditorDataAsync(viewModel);
            return View(viewModel);
        }

        try
        {
            CreateWorkItemRequest request = new CreateWorkItemRequest
            {
                ProjectId = viewModel.ProjectId!.Value,
                EpicId = viewModel.EpicId,
                ParentWorkItemId = viewModel.ParentWorkItemId,
                Type = viewModel.Type!.Value,
                Summary = viewModel.Summary,
                Description = viewModel.Description,
                AssigneeId = viewModel.AssigneeId,
                ReporterId = viewModel.ReporterId,
                Priority = viewModel.Priority,
                DueDate = viewModel.DueDate
            };

            WorkItemDto? createdWorkItem = await m_ApiClient.PostAsync<WorkItemDto>("/api/workitems", request);

            if (createdWorkItem == null)
            {
                ModelState.AddModelError(string.Empty, "Failed to create work item.");
                await LoadEditorDataAsync(viewModel);
                return View(viewModel);
            }

            TempData["SuccessMessage"] = $"Work item '{createdWorkItem.Key}' created successfully.";
            return RedirectToAction(nameof(Details), new { id = createdWorkItem.Id });
        }
        catch (Exception ex)
        {
            m_Logger.LogError(ex, "Error creating work item");
            ModelState.AddModelError(string.Empty, "An error occurred while creating the work item. Please try again.");
            await LoadEditorDataAsync(viewModel);
            return View(viewModel);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            WorkItemDto? workItem = await m_ApiClient.GetAsync<WorkItemDto>($"/api/workitems/{id}");
            if (workItem == null)
            {
                TempData["ErrorMessage"] = "Work item not found.";
                return RedirectToAction(nameof(Index));
            }

            WorkItemEditorViewModel viewModel = new WorkItemEditorViewModel
            {
                Id = workItem.Id,
                ProjectId = workItem.ProjectId,
                EpicId = workItem.EpicId,
                ParentWorkItemId = workItem.ParentWorkItemId,
                Type = workItem.Type,
                Summary = workItem.Summary,
                Description = workItem.Description,
                AssigneeId = workItem.AssigneeId,
                ReporterId = workItem.ReporterId,
                StatusId = workItem.StatusId,
                Priority = workItem.Priority,
                DueDate = workItem.DueDate
            };

            await LoadEditorDataAsync(viewModel);
            return View(viewModel);
        }
        catch (Exception ex)
        {
            m_Logger.LogError(ex, "Error loading work item for editing, work item ID {WorkItemId}", id);
            TempData["ErrorMessage"] = "Failed to load work item. Please try again.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, WorkItemEditorViewModel viewModel)
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
            UpdateWorkItemRequest request = new UpdateWorkItemRequest
            {
                Summary = viewModel.Summary,
                Description = viewModel.Description,
                AssigneeId = viewModel.AssigneeId,
                StatusId = viewModel.StatusId!.Value,
                Priority = viewModel.Priority,
                DueDate = viewModel.DueDate
            };

            await m_ApiClient.PutAsync<object>($"/api/workitems/{id}", request);

            TempData["SuccessMessage"] = "Work item updated successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            m_Logger.LogError(ex, "Error updating work item ID {WorkItemId}", id);
            ModelState.AddModelError(string.Empty, "An error occurred while updating the work item. Please try again.");
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
            await m_ApiClient.DeleteAsync($"/api/workitems/{id}");

            TempData["SuccessMessage"] = "Work item deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            m_Logger.LogError(ex, "Error deleting work item ID {WorkItemId}", id);
            TempData["ErrorMessage"] = "Failed to delete work item. Please try again.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    private async Task LoadEditorDataAsync(WorkItemEditorViewModel viewModel)
    {
        try
        {
            viewModel.AvailableProjects = await m_ApiClient.GetAsync<List<ProjectListItemDto>>("/api/projects") ?? [];

            if (viewModel.ProjectId.HasValue)
            {
                viewModel.AvailableEpics = await m_ApiClient.GetAsync<List<EpicListItemDto>>($"/api/projects/{viewModel.ProjectId.Value}/epics") ?? [];

                WorkItemSearchRequest taskSearchRequest = new WorkItemSearchRequest
                {
                    ProjectId = viewModel.ProjectId.Value,
                    Type = WorkItemType.Task,
                    PageSize = 1000
                };

                List<WorkItemListItemDto> tasksAndBugs = await m_ApiClient.PostAsync<List<WorkItemListItemDto>>(
                    "/api/workitems/search",
                    taskSearchRequest) ?? [];

                WorkItemSearchRequest bugSearchRequest = new WorkItemSearchRequest
                {
                    ProjectId = viewModel.ProjectId.Value,
                    Type = WorkItemType.Bug,
                    PageSize = 1000
                };

                List<WorkItemListItemDto> bugs = await m_ApiClient.PostAsync<List<WorkItemListItemDto>>(
                    "/api/workitems/search",
                    bugSearchRequest) ?? [];

                tasksAndBugs.AddRange(bugs);

                if (viewModel.Id.HasValue)
                {
                    tasksAndBugs = tasksAndBugs.Where(wi => wi.Id != viewModel.Id.Value).ToList();
                }

                viewModel.AvailableParentWorkItems = tasksAndBugs;
            }

            viewModel.AvailableStatuses = await m_ApiClient.GetAsync<List<StatusDto>>("/api/statuses") ?? [];

            viewModel.AvailableUsers = await m_ApiClient.GetAsync<List<UserDto>>("/api/users") ?? [];
        }
        catch (Exception ex)
        {
            m_Logger.LogError(ex, "Error loading editor data for work item");
        }
    }

    private static string RenderMarkdown(string markdown)
    {
        string html = System.Net.WebUtility.HtmlEncode(markdown);
        html = html.Replace("\n", "<br>");
        return html;
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
