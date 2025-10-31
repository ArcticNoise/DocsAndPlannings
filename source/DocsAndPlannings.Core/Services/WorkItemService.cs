using DocsAndPlannings.Core.Data;
using DocsAndPlannings.Core.DTOs.WorkItems;
using DocsAndPlannings.Core.Exceptions;
using DocsAndPlannings.Core.Exceptions.Planning;
using DocsAndPlannings.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DocsAndPlannings.Core.Services;

public sealed class WorkItemService : IWorkItemService
{
    private readonly ApplicationDbContext m_Context;
    private readonly IKeyGenerationService m_KeyGenerationService;
    private readonly IStatusService m_StatusService;

    public WorkItemService(
        ApplicationDbContext context,
        IKeyGenerationService keyGenerationService,
        IStatusService statusService)
    {
        m_Context = context ?? throw new ArgumentNullException(nameof(context));
        m_KeyGenerationService = keyGenerationService ?? throw new ArgumentNullException(nameof(keyGenerationService));
        m_StatusService = statusService ?? throw new ArgumentNullException(nameof(statusService));
    }

    public async Task<WorkItemDto> CreateWorkItemAsync(CreateWorkItemRequest request, int creatorId)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        // Validate project exists
        var project = await m_Context.Projects.FindAsync(request.ProjectId);
        if (project is null)
        {
            throw new NotFoundException($"Project with ID {request.ProjectId} not found");
        }

        // Validate epic if provided
        if (request.EpicId.HasValue)
        {
            var epic = await m_Context.Epics.FindAsync(request.EpicId.Value);
            if (epic is null)
            {
                throw new NotFoundException($"Epic with ID {request.EpicId.Value} not found");
            }

            if (epic.ProjectId != request.ProjectId)
            {
                throw new BadRequestException("Epic does not belong to the specified project");
            }
        }

        // Get default status for new items
        var defaultStatus = await m_Context.Statuses
            .FirstOrDefaultAsync(s => s.IsDefaultForNew && s.IsActive);
        if (defaultStatus is null)
        {
            throw new BadRequestException("No default status found. Please ensure at least one status is marked as default.");
        }

        // Validate assignee if provided
        if (request.AssigneeId.HasValue)
        {
            var assignee = await m_Context.Users.FindAsync(request.AssigneeId.Value);
            if (assignee is null)
            {
                throw new NotFoundException($"User with ID {request.AssigneeId.Value} not found");
            }
        }

        // Validate hierarchy
        if (request.ParentWorkItemId.HasValue)
        {
            await ValidateHierarchyAsync(request.Type, request.ParentWorkItemId.Value, request.ProjectId);
        }

        // Generate work item key
        var workItemKey = await m_KeyGenerationService.GenerateWorkItemKeyAsync(request.ProjectId);

        var workItem = new WorkItem
        {
            ProjectId = request.ProjectId,
            EpicId = request.EpicId,
            ParentWorkItemId = request.ParentWorkItemId,
            Key = workItemKey,
            Type = request.Type,
            Summary = request.Summary,
            Description = request.Description,
            StatusId = defaultStatus.Id,
            AssigneeId = request.AssigneeId,
            ReporterId = request.ReporterId ?? creatorId,
            Priority = request.Priority,
            DueDate = request.DueDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        m_Context.WorkItems.Add(workItem);
        await m_Context.SaveChangesAsync();

        return await MapToDtoAsync(workItem);
    }

    public async Task<WorkItemDto?> GetWorkItemByIdAsync(int id)
    {
        var workItem = await m_Context.WorkItems
            .Include(w => w.Project)
            .Include(w => w.Epic)
            .Include(w => w.ParentWorkItem)
            .Include(w => w.Status)
            .Include(w => w.Assignee)
            .Include(w => w.Reporter)
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == id);

        return workItem != null ? await MapToDtoAsync(workItem) : null;
    }

    public async Task<WorkItemDto?> GetWorkItemByKeyAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Work item key cannot be null or empty", nameof(key));
        }

        var workItem = await m_Context.WorkItems
            .Include(w => w.Project)
            .Include(w => w.Epic)
            .Include(w => w.ParentWorkItem)
            .Include(w => w.Status)
            .Include(w => w.Assignee)
            .Include(w => w.Reporter)
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Key == key);

        return workItem != null ? await MapToDtoAsync(workItem) : null;
    }

    public async Task<IReadOnlyList<WorkItemListItemDto>> SearchWorkItemsAsync(WorkItemSearchRequest request)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var query = m_Context.WorkItems
            .Include(w => w.Project)
            .Include(w => w.Epic)
            .Include(w => w.Status)
            .Include(w => w.Assignee)
            .AsNoTracking()
            .AsQueryable();

        if (request.ProjectId.HasValue)
        {
            query = query.Where(w => w.ProjectId == request.ProjectId.Value);
        }

        if (request.EpicId.HasValue)
        {
            query = query.Where(w => w.EpicId == request.EpicId.Value);
        }

        if (request.StatusId.HasValue)
        {
            query = query.Where(w => w.StatusId == request.StatusId.Value);
        }

        if (request.AssigneeId.HasValue)
        {
            query = query.Where(w => w.AssigneeId == request.AssigneeId.Value);
        }

        if (request.ReporterId.HasValue)
        {
            query = query.Where(w => w.ReporterId == request.ReporterId.Value);
        }

        if (request.Type.HasValue)
        {
            query = query.Where(w => w.Type == request.Type.Value);
        }

        if (request.Priority.HasValue)
        {
            query = query.Where(w => w.Priority == request.Priority.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            query = query.Where(w =>
                w.Key.Contains(request.SearchText) ||
                w.Summary.Contains(request.SearchText) ||
                (w.Description != null && w.Description.Contains(request.SearchText)));
        }

        var workItems = await query
            .OrderByDescending(w => w.UpdatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        // Single query to get all counts
        var workItemIds = workItems.Select(w => w.Id).ToList();

        var childWorkItemCounts = await m_Context.WorkItems
            .Where(w => workItemIds.Contains(w.ParentWorkItemId!.Value))
            .GroupBy(w => w.ParentWorkItemId)
            .Select(g => new { ParentId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ParentId!.Value, x => x.Count);

        var commentCounts = await m_Context.WorkItemComments
            .Where(c => workItemIds.Contains(c.WorkItemId))
            .GroupBy(c => c.WorkItemId)
            .Select(g => new { WorkItemId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.WorkItemId, x => x.Count);

        return workItems.Select(workItem => new WorkItemListItemDto
        {
            Id = workItem.Id,
            Key = workItem.Key,
            Type = workItem.Type,
            Summary = workItem.Summary,
            StatusName = workItem.Status.Name,
            AssigneeName = workItem.Assignee != null ? $"{workItem.Assignee.FirstName} {workItem.Assignee.LastName}" : null,
            Priority = workItem.Priority,
            DueDate = workItem.DueDate,
            UpdatedAt = workItem.UpdatedAt,
            ChildWorkItemCount = childWorkItemCounts.GetValueOrDefault(workItem.Id, 0),
            CommentCount = commentCounts.GetValueOrDefault(workItem.Id, 0)
        }).ToList();
    }

    public async Task<WorkItemDto> UpdateWorkItemAsync(int id, UpdateWorkItemRequest request, int userId)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var workItem = await m_Context.WorkItems
            .Include(w => w.Project)
            .Include(w => w.Epic)
            .Include(w => w.ParentWorkItem)
            .Include(w => w.Status)
            .Include(w => w.Assignee)
            .Include(w => w.Reporter)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (workItem is null)
        {
            throw new NotFoundException($"Work item with ID {id} not found");
        }

        // Validate status exists
        var status = await m_Context.Statuses.FindAsync(request.StatusId);
        if (status is null)
        {
            throw new NotFoundException($"Status with ID {request.StatusId} not found");
        }

        // Validate assignee if provided
        if (request.AssigneeId.HasValue)
        {
            var assignee = await m_Context.Users.FindAsync(request.AssigneeId.Value);
            if (assignee is null)
            {
                throw new NotFoundException($"User with ID {request.AssigneeId.Value} not found");
            }
        }

        workItem.Summary = request.Summary;
        workItem.Description = request.Description;
        workItem.StatusId = request.StatusId;
        workItem.AssigneeId = request.AssigneeId;
        workItem.Priority = request.Priority;
        workItem.DueDate = request.DueDate;
        workItem.UpdatedAt = DateTime.UtcNow;

        await m_Context.SaveChangesAsync();

        return await MapToDtoAsync(workItem);
    }

    public async Task DeleteWorkItemAsync(int id, int userId)
    {
        var workItem = await m_Context.WorkItems.FirstOrDefaultAsync(w => w.Id == id);

        if (workItem is null)
        {
            throw new NotFoundException($"Work item with ID {id} not found");
        }

        // Check for child work items
        var childCount = await m_Context.WorkItems.CountAsync(w => w.ParentWorkItemId == id);

        if (childCount > 0)
        {
            throw new BadRequestException(
                $"Cannot delete work item '{workItem.Key}' because it has {childCount} child work items. " +
                "Please delete or reassign them first.");
        }

        // Check for comments
        var commentCount = await m_Context.WorkItemComments.CountAsync(c => c.WorkItemId == id);

        if (commentCount > 0)
        {
            throw new BadRequestException(
                $"Cannot delete work item '{workItem.Key}' because it has {commentCount} comments. " +
                "Please delete them first.");
        }

        m_Context.WorkItems.Remove(workItem);
        await m_Context.SaveChangesAsync();
    }

    public async Task<WorkItemDto> AssignWorkItemAsync(int id, int? assigneeId, int userId)
    {
        var workItem = await m_Context.WorkItems
            .Include(w => w.Project)
            .Include(w => w.Epic)
            .Include(w => w.ParentWorkItem)
            .Include(w => w.Status)
            .Include(w => w.Assignee)
            .Include(w => w.Reporter)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (workItem is null)
        {
            throw new NotFoundException($"Work item with ID {id} not found");
        }

        // Validate assignee if provided
        if (assigneeId.HasValue)
        {
            var assignee = await m_Context.Users.FindAsync(assigneeId.Value);
            if (assignee is null)
            {
                throw new NotFoundException($"User with ID {assigneeId.Value} not found");
            }
        }

        workItem.AssigneeId = assigneeId;
        workItem.UpdatedAt = DateTime.UtcNow;

        await m_Context.SaveChangesAsync();

        return await MapToDtoAsync(workItem);
    }

    public async Task<WorkItemDto> UpdateWorkItemStatusAsync(int id, int statusId, int userId)
    {
        var workItem = await m_Context.WorkItems
            .Include(w => w.Project)
            .Include(w => w.Epic)
            .Include(w => w.ParentWorkItem)
            .Include(w => w.Status)
            .Include(w => w.Assignee)
            .Include(w => w.Reporter)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (workItem is null)
        {
            throw new NotFoundException($"Work item with ID {id} not found");
        }

        // Validate status exists
        var newStatus = await m_Context.Statuses.FindAsync(statusId);
        if (newStatus is null)
        {
            throw new NotFoundException($"Status with ID {statusId} not found");
        }

        // Validate status transition
        var isValidTransition = await m_StatusService.ValidateTransitionAsync(workItem.StatusId, statusId);
        if (!isValidTransition)
        {
            var currentStatus = await m_Context.Statuses.FindAsync(workItem.StatusId);
            throw new InvalidStatusTransitionException(
                $"Cannot transition from '{currentStatus?.Name ?? "Unknown"}' to '{newStatus.Name}'");
        }

        workItem.StatusId = statusId;
        workItem.UpdatedAt = DateTime.UtcNow;

        await m_Context.SaveChangesAsync();

        return await MapToDtoAsync(workItem);
    }

    public async Task<WorkItemDto> UpdateWorkItemParentAsync(int id, int? parentWorkItemId, int userId)
    {
        var workItem = await m_Context.WorkItems
            .Include(w => w.Project)
            .Include(w => w.Epic)
            .Include(w => w.ParentWorkItem)
            .Include(w => w.Status)
            .Include(w => w.Assignee)
            .Include(w => w.Reporter)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (workItem is null)
        {
            throw new NotFoundException($"Work item with ID {id} not found");
        }

        // Validate hierarchy if parent is specified
        if (parentWorkItemId.HasValue)
        {
            await ValidateHierarchyAsync(workItem.Type, parentWorkItemId.Value, workItem.ProjectId);

            // Check for circular reference
            var wouldCreateCircular = await WouldCreateCircularReferenceAsync(id, parentWorkItemId);
            if (wouldCreateCircular)
            {
                throw new CircularHierarchyException("Setting this parent would create a circular reference");
            }
        }

        workItem.ParentWorkItemId = parentWorkItemId;
        workItem.UpdatedAt = DateTime.UtcNow;

        await m_Context.SaveChangesAsync();

        return await MapToDtoAsync(workItem);
    }

    private async Task ValidateHierarchyAsync(WorkItemType childType, int parentWorkItemId, int projectId)
    {
        var parent = await m_Context.WorkItems.FindAsync(parentWorkItemId);

        if (parent is null)
        {
            throw new NotFoundException($"Parent work item with ID {parentWorkItemId} not found");
        }

        if (parent.ProjectId != projectId)
        {
            throw new BadRequestException("Parent work item does not belong to the same project");
        }

        // Subtasks can only have Task parents
        if (childType == WorkItemType.Subtask && parent.Type != WorkItemType.Task)
        {
            throw new InvalidHierarchyException(
                $"Subtasks can only be children of Tasks. Cannot assign {childType} to {parent.Type}.");
        }

        // Tasks cannot have parents (only Subtasks can have parents)
        if (childType == WorkItemType.Task)
        {
            throw new InvalidHierarchyException(
                "Tasks cannot have parent work items. Only Subtasks can have parents.");
        }

        // Bugs cannot have parents
        if (childType == WorkItemType.Bug)
        {
            throw new InvalidHierarchyException(
                "Bugs cannot have parent work items.");
        }

        // Check max nesting level (Task → Subtask, not Subtask → Sub-subtask)
        if (parent.ParentWorkItemId.HasValue)
        {
            throw new InvalidHierarchyException(
                "Maximum nesting level exceeded. Only one level of nesting is allowed (Task → Subtask).");
        }
    }

    private async Task<bool> WouldCreateCircularReferenceAsync(int workItemId, int? parentWorkItemId)
    {
        if (!parentWorkItemId.HasValue)
        {
            return false;
        }

        var visited = new HashSet<int> { workItemId };
        var currentId = parentWorkItemId.Value;

        while (currentId != 0)
        {
            if (visited.Contains(currentId))
            {
                return true; // Circular reference detected
            }

            visited.Add(currentId);

            var parent = await m_Context.WorkItems
                .AsNoTracking()
                .Where(w => w.Id == currentId)
                .Select(w => new { w.ParentWorkItemId })
                .FirstOrDefaultAsync();

            if (parent is null)
            {
                break;
            }

            currentId = parent.ParentWorkItemId ?? 0;
        }

        return false;
    }

    private async Task<WorkItemDto> MapToDtoAsync(WorkItem workItem)
    {
        var childWorkItemCount = await m_Context.WorkItems.CountAsync(w => w.ParentWorkItemId == workItem.Id);
        var commentCount = await m_Context.WorkItemComments.CountAsync(c => c.WorkItemId == workItem.Id);

        return new WorkItemDto
        {
            Id = workItem.Id,
            ProjectId = workItem.ProjectId,
            ProjectKey = workItem.Project.Key,
            EpicId = workItem.EpicId,
            EpicKey = workItem.Epic?.Key,
            ParentWorkItemId = workItem.ParentWorkItemId,
            ParentWorkItemKey = workItem.ParentWorkItem?.Key,
            Key = workItem.Key,
            Type = workItem.Type,
            Summary = workItem.Summary,
            Description = workItem.Description,
            StatusId = workItem.StatusId,
            StatusName = workItem.Status.Name,
            AssigneeId = workItem.AssigneeId,
            AssigneeName = workItem.Assignee != null ? $"{workItem.Assignee.FirstName} {workItem.Assignee.LastName}" : null,
            ReporterId = workItem.ReporterId,
            ReporterName = workItem.Reporter != null ? $"{workItem.Reporter.FirstName} {workItem.Reporter.LastName}" : null,
            Priority = workItem.Priority,
            DueDate = workItem.DueDate,
            CreatedAt = workItem.CreatedAt,
            UpdatedAt = workItem.UpdatedAt,
            ChildWorkItemCount = childWorkItemCount,
            CommentCount = commentCount
        };
    }
}
