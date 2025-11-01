using System.Security.Claims;
using DocsAndPlannings.Core.DTOs.Documents;
using DocsAndPlannings.Core.DTOs.Projects;
using DocsAndPlannings.Core.DTOs.WorkItems;
using DocsAndPlannings.Web.Services;
using DocsAndPlannings.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocsAndPlannings.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IApiClient apiClient,
        ILogger<DashboardController> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    // GET: /Dashboard
    public async Task<IActionResult> Index()
    {
        try
        {
            string? userName = User.FindFirstValue(ClaimTypes.Name);
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            DashboardViewModel viewModel = new DashboardViewModel
            {
                UserName = userName ?? "User"
            };

            // Fetch stats from API
            try
            {
                // Get documents count
                List<DocumentListItemDto>? documents = await _apiClient.GetAsync<List<DocumentListItemDto>>("/api/documents?pageSize=1000");
                viewModel.Stats.TotalDocuments = documents?.Count ?? 0;

                // Get projects count
                List<ProjectListItemDto>? projects = await _apiClient.GetAsync<List<ProjectListItemDto>>("/api/projects?pageSize=1000");
                viewModel.Stats.TotalProjects = projects?.Count ?? 0;

                // Get work items assigned to current user
                WorkItemSearchRequest searchRequest = new WorkItemSearchRequest
                {
                    AssigneeId = userId,
                    PageSize = 1000
                };

                List<WorkItemListItemDto>? workItems = await _apiClient.PostAsync<List<WorkItemListItemDto>>(
                    "/api/workitems/search",
                    searchRequest);

                viewModel.Stats.AssignedWorkItems = workItems?.Count ?? 0;
                viewModel.Stats.ActiveWorkItems = workItems?.Count(w => w.StatusName != "Done" && w.StatusName != "Cancelled") ?? 0;

                // Create recent activity items from documents (most recent 5)
                if (documents != null && documents.Any())
                {
                    foreach (DocumentListItemDto doc in documents.OrderByDescending(d => d.UpdatedAt).Take(5))
                    {
                        viewModel.RecentActivity.Add(new RecentActivityItem
                        {
                            Type = "Document",
                            Title = doc.Title,
                            Description = $"Updated {GetRelativeTime(doc.UpdatedAt)}",
                            Timestamp = doc.UpdatedAt,
                            Icon = "bi-file-text",
                            Url = $"/Documents/Details/{doc.Id}"
                        });
                    }
                }

                // Add work items to recent activity (most recent 3)
                if (workItems != null && workItems.Any())
                {
                    foreach (WorkItemListItemDto item in workItems.OrderByDescending(w => w.UpdatedAt).Take(3))
                    {
                        string icon = item.Type.ToString() switch
                        {
                            "Task" => "bi-check-circle",
                            "Bug" => "bi-bug",
                            "Subtask" => "bi-list-check",
                            _ => "bi-card-checklist"
                        };

                        viewModel.RecentActivity.Add(new RecentActivityItem
                        {
                            Type = item.Type.ToString(),
                            Title = item.Summary,
                            Description = $"{item.StatusName} - Updated {GetRelativeTime(item.UpdatedAt)}",
                            Timestamp = item.UpdatedAt,
                            Icon = icon,
                            Url = $"/WorkItems/Details/{item.Id}"
                        });
                    }
                }

                // Sort activity by timestamp (most recent first)
                viewModel.RecentActivity = viewModel.RecentActivity
                    .OrderByDescending(a => a.Timestamp)
                    .Take(8)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error fetching dashboard data, showing empty stats");
                // Continue with empty stats
            }

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard");
            TempData["ErrorMessage"] = "An error occurred while loading the dashboard.";
            return View(new DashboardViewModel { UserName = "User" });
        }
    }

    private static string GetRelativeTime(DateTime dateTime)
    {
        TimeSpan span = DateTime.UtcNow - dateTime;

        if (span.TotalMinutes < 1)
        {
            return "just now";
        }
        else if (span.TotalMinutes < 60)
        {
            int minutes = (int)span.TotalMinutes;
            return $"{minutes} minute{(minutes != 1 ? "s" : "")} ago";
        }
        else if (span.TotalHours < 24)
        {
            int hours = (int)span.TotalHours;
            return $"{hours} hour{(hours != 1 ? "s" : "")} ago";
        }
        else if (span.TotalDays < 30)
        {
            int days = (int)span.TotalDays;
            return $"{days} day{(days != 1 ? "s" : "")} ago";
        }
        else if (span.TotalDays < 365)
        {
            int months = (int)(span.TotalDays / 30);
            return $"{months} month{(months != 1 ? "s" : "")} ago";
        }
        else
        {
            int years = (int)(span.TotalDays / 365);
            return $"{years} year{(years != 1 ? "s" : "")} ago";
        }
    }
}
