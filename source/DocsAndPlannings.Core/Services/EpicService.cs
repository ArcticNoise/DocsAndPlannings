using DocsAndPlannings.Core.Data;
using DocsAndPlannings.Core.DTOs.Epics;
using DocsAndPlannings.Core.Exceptions;
using DocsAndPlannings.Core.Exceptions.Planning;
using DocsAndPlannings.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DocsAndPlannings.Core.Services;

public sealed class EpicService : IEpicService
{
    private readonly ApplicationDbContext m_Context;
    private readonly IKeyGenerationService m_KeyGenerationService;
    private readonly IStatusService m_StatusService;

    public EpicService(
        ApplicationDbContext context,
        IKeyGenerationService keyGenerationService,
        IStatusService statusService)
    {
        m_Context = context ?? throw new ArgumentNullException(nameof(context));
        m_KeyGenerationService = keyGenerationService ?? throw new ArgumentNullException(nameof(keyGenerationService));
        m_StatusService = statusService ?? throw new ArgumentNullException(nameof(statusService));
    }

    public async Task<EpicDto> CreateEpicAsync(CreateEpicRequest request, int creatorId)
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

        // Generate epic key
        var epicKey = await m_KeyGenerationService.GenerateEpicKeyAsync(request.ProjectId);

        var epic = new Epic
        {
            ProjectId = request.ProjectId,
            Key = epicKey,
            Summary = request.Summary,
            Description = request.Description,
            StatusId = defaultStatus.Id,
            AssigneeId = request.AssigneeId,
            Priority = request.Priority,
            StartDate = request.StartDate,
            DueDate = request.DueDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        m_Context.Epics.Add(epic);
        await m_Context.SaveChangesAsync();

        return await MapToDtoAsync(epic);
    }

    public async Task<EpicDto?> GetEpicByIdAsync(int id)
    {
        var epic = await m_Context.Epics
            .Include(e => e.Project)
            .Include(e => e.Status)
            .Include(e => e.Assignee)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);

        return epic != null ? await MapToDtoAsync(epic) : null;
    }

    public async Task<EpicDto?> GetEpicByKeyAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Epic key cannot be null or empty", nameof(key));
        }

        var epic = await m_Context.Epics
            .Include(e => e.Project)
            .Include(e => e.Status)
            .Include(e => e.Assignee)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Key == key);

        return epic != null ? await MapToDtoAsync(epic) : null;
    }

    public async Task<IReadOnlyList<EpicListItemDto>> GetAllEpicsAsync(
        int? projectId = null,
        int? statusId = null,
        int? assigneeId = null,
        bool? isActive = null,
        int page = 1,
        int pageSize = 50)
    {
        var query = m_Context.Epics
            .Include(e => e.Project)
            .Include(e => e.Status)
            .Include(e => e.Assignee)
            .AsNoTracking()
            .AsQueryable();

        if (projectId.HasValue)
        {
            query = query.Where(e => e.ProjectId == projectId.Value);
        }

        if (statusId.HasValue)
        {
            query = query.Where(e => e.StatusId == statusId.Value);
        }

        if (assigneeId.HasValue)
        {
            query = query.Where(e => e.AssigneeId == assigneeId.Value);
        }

        // isActive parameter kept for API compatibility but not used since Epic doesn't have IsActive

        var epics = await query
            .OrderByDescending(e => e.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Single query to get all work item counts
        var epicIds = epics.Select(e => e.Id).ToList();

        var workItemCounts = await m_Context.WorkItems
            .Where(w => epicIds.Contains(w.EpicId!.Value))
            .GroupBy(w => w.EpicId)
            .Select(g => new { EpicId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.EpicId!.Value, x => x.Count);

        var completedWorkItemCounts = await m_Context.WorkItems
            .Include(w => w.Status)
            .Where(w => epicIds.Contains(w.EpicId!.Value) && w.Status.IsCompletedStatus)
            .GroupBy(w => w.EpicId)
            .Select(g => new { EpicId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.EpicId!.Value, x => x.Count);

        return epics.Select(epic => new EpicListItemDto
        {
            Id = epic.Id,
            Key = epic.Key,
            Summary = epic.Summary,
            StatusName = epic.Status.Name,
            AssigneeName = epic.Assignee != null ? $"{epic.Assignee.FirstName} {epic.Assignee.LastName}" : null,
            Priority = epic.Priority,
            DueDate = epic.DueDate,
            UpdatedAt = epic.UpdatedAt,
            WorkItemCount = workItemCounts.GetValueOrDefault(epic.Id, 0),
            CompletedWorkItemCount = completedWorkItemCounts.GetValueOrDefault(epic.Id, 0)
        }).ToList();
    }

    public async Task<EpicDto> UpdateEpicAsync(int id, UpdateEpicRequest request, int userId)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var epic = await m_Context.Epics
            .Include(e => e.Project)
            .Include(e => e.Status)
            .Include(e => e.Assignee)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (epic is null)
        {
            throw new NotFoundException($"Epic with ID {id} not found");
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

        epic.Summary = request.Summary;
        epic.Description = request.Description;
        epic.StatusId = request.StatusId;
        epic.AssigneeId = request.AssigneeId;
        epic.Priority = request.Priority;
        epic.StartDate = request.StartDate;
        epic.DueDate = request.DueDate;
        epic.UpdatedAt = DateTime.UtcNow;

        await m_Context.SaveChangesAsync();

        return await MapToDtoAsync(epic);
    }

    public async Task DeleteEpicAsync(int id, int userId)
    {
        var epic = await m_Context.Epics.FirstOrDefaultAsync(e => e.Id == id);

        if (epic is null)
        {
            throw new NotFoundException($"Epic with ID {id} not found");
        }

        // Check for associated work items
        var workItemCount = await m_Context.WorkItems.CountAsync(w => w.EpicId == id);

        if (workItemCount > 0)
        {
            throw new BadRequestException(
                $"Cannot delete epic '{epic.Key}' because it contains {workItemCount} work items. " +
                "Please delete or reassign them first.");
        }

        m_Context.Epics.Remove(epic);
        await m_Context.SaveChangesAsync();
    }

    public async Task<EpicDto> AssignEpicAsync(int id, int? assigneeId, int userId)
    {
        var epic = await m_Context.Epics
            .Include(e => e.Project)
            .Include(e => e.Status)
            .Include(e => e.Assignee)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (epic is null)
        {
            throw new NotFoundException($"Epic with ID {id} not found");
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

        epic.AssigneeId = assigneeId;
        epic.UpdatedAt = DateTime.UtcNow;

        await m_Context.SaveChangesAsync();

        return await MapToDtoAsync(epic);
    }

    public async Task<EpicDto> UpdateEpicStatusAsync(int id, int statusId, int userId)
    {
        var epic = await m_Context.Epics
            .Include(e => e.Project)
            .Include(e => e.Status)
            .Include(e => e.Assignee)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (epic is null)
        {
            throw new NotFoundException($"Epic with ID {id} not found");
        }

        // Validate status exists
        var newStatus = await m_Context.Statuses.FindAsync(statusId);
        if (newStatus is null)
        {
            throw new NotFoundException($"Status with ID {statusId} not found");
        }

        // Validate status transition
        var isValidTransition = await m_StatusService.ValidateTransitionAsync(epic.StatusId, statusId);
        if (!isValidTransition)
        {
            var currentStatus = await m_Context.Statuses.FindAsync(epic.StatusId);
            throw new InvalidStatusTransitionException(
                $"Cannot transition from '{currentStatus?.Name ?? "Unknown"}' to '{newStatus.Name}'");
        }

        epic.StatusId = statusId;
        epic.UpdatedAt = DateTime.UtcNow;

        await m_Context.SaveChangesAsync();

        return await MapToDtoAsync(epic);
    }

    private async Task<EpicDto> MapToDtoAsync(Epic epic)
    {
        var workItemCount = await m_Context.WorkItems.CountAsync(w => w.EpicId == epic.Id);
        var completedWorkItemCount = await m_Context.WorkItems
            .Include(w => w.Status)
            .CountAsync(w => w.EpicId == epic.Id && w.Status.IsCompletedStatus);

        return new EpicDto
        {
            Id = epic.Id,
            ProjectId = epic.ProjectId,
            ProjectKey = epic.Project.Key,
            Key = epic.Key,
            Summary = epic.Summary,
            Description = epic.Description,
            StatusId = epic.StatusId,
            StatusName = epic.Status.Name,
            AssigneeId = epic.AssigneeId,
            AssigneeName = epic.Assignee != null ? $"{epic.Assignee.FirstName} {epic.Assignee.LastName}" : null,
            Priority = epic.Priority,
            StartDate = epic.StartDate,
            DueDate = epic.DueDate,
            CreatedAt = epic.CreatedAt,
            UpdatedAt = epic.UpdatedAt,
            WorkItemCount = workItemCount,
            CompletedWorkItemCount = completedWorkItemCount
        };
    }
}
