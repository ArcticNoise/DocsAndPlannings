using DocsAndPlannings.Core.Data;
using DocsAndPlannings.Core.DTOs.Statuses;
using DocsAndPlannings.Core.Exceptions;
using DocsAndPlannings.Core.Exceptions.Planning;
using DocsAndPlannings.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DocsAndPlannings.Core.Services;

/// <summary>
/// Service for managing statuses and status transitions
/// </summary>
public sealed class StatusService : IStatusService
{
    private readonly ApplicationDbContext m_Context;

    public StatusService(ApplicationDbContext context)
    {
        m_Context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IReadOnlyList<StatusDto>> GetAllStatusesAsync(CancellationToken cancellationToken = default)
    {
        var statuses = await m_Context.Statuses
            .AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.OrderIndex)
            .ToListAsync(cancellationToken);

        return statuses.Select(MapToDto).ToList();
    }

    public async Task<StatusDto?> GetStatusByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var status = await m_Context.Statuses
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        return status != null ? MapToDto(status) : null;
    }

    public async Task<StatusDto> CreateStatusAsync(CreateStatusRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        // Check for duplicate name
        var exists = await m_Context.Statuses
            .AnyAsync(s => s.Name == request.Name, cancellationToken);

        if (exists)
        {
            throw new BadRequestException($"Status with name '{request.Name}' already exists");
        }

        var status = new Status
        {
            Name = request.Name,
            Color = request.Color,
            OrderIndex = request.OrderIndex,
            IsDefaultForNew = request.IsDefaultForNew,
            IsCompletedStatus = request.IsCompletedStatus,
            IsCancelledStatus = request.IsCancelledStatus,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        m_Context.Statuses.Add(status);
        await m_Context.SaveChangesAsync(cancellationToken);

        return MapToDto(status);
    }

    public async Task<StatusDto> UpdateStatusAsync(int id, UpdateStatusRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var status = await m_Context.Statuses
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (status is null)
        {
            throw new NotFoundException($"Status with ID {id} not found");
        }

        // Check for duplicate name (excluding current status)
        var exists = await m_Context.Statuses
            .AnyAsync(s => s.Name == request.Name && s.Id != id, cancellationToken);

        if (exists)
        {
            throw new BadRequestException($"Status with name '{request.Name}' already exists");
        }

        status.Name = request.Name;
        status.Color = request.Color;
        status.OrderIndex = request.OrderIndex;
        status.IsDefaultForNew = request.IsDefaultForNew;
        status.IsCompletedStatus = request.IsCompletedStatus;
        status.IsCancelledStatus = request.IsCancelledStatus;
        status.IsActive = request.IsActive;

        await m_Context.SaveChangesAsync(cancellationToken);

        return MapToDto(status);
    }

    public async Task DeleteStatusAsync(int id, CancellationToken cancellationToken = default)
    {
        var status = await m_Context.Statuses
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (status is null)
        {
            throw new NotFoundException($"Status with ID {id} not found");
        }

        // Check if status is in use
        var epicCount = await m_Context.Epics.CountAsync(e => e.StatusId == id, cancellationToken);
        var workItemCount = await m_Context.WorkItems.CountAsync(w => w.StatusId == id, cancellationToken);

        if (epicCount > 0 || workItemCount > 0)
        {
            throw new BadRequestException(
                $"Cannot delete status '{status.Name}' because it is in use by {epicCount} epics and {workItemCount} work items");
        }

        m_Context.Statuses.Remove(status);
        await m_Context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StatusDto>> GetAllowedTransitionsAsync(int fromStatusId, CancellationToken cancellationToken = default)
    {
        var transitions = await m_Context.StatusTransitions
            .AsNoTracking()
            .Include(t => t.ToStatus)
            .Where(t => t.FromStatusId == fromStatusId && t.IsAllowed)
            .Select(t => t.ToStatus)
            .ToListAsync(cancellationToken);

        return transitions.Select(MapToDto).ToList();
    }

    public async Task<bool> ValidateTransitionAsync(int fromStatusId, int toStatusId, CancellationToken cancellationToken = default)
    {
        // Same status is always allowed
        if (fromStatusId == toStatusId)
        {
            return true;
        }

        // Check if there's an explicit transition rule
        var transition = await m_Context.StatusTransitions
            .AsNoTracking()
            .FirstOrDefaultAsync(t =>
                t.FromStatusId == fromStatusId &&
                t.ToStatusId == toStatusId, cancellationToken);

        // If no explicit rule exists, allow transition (permissive by default)
        // If rule exists, respect the IsAllowed flag
        return transition?.IsAllowed ?? true;
    }

    public async Task<StatusTransitionDto> CreateStatusTransitionAsync(CreateStatusTransitionRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        // Validate source and target statuses exist
        var fromStatus = await m_Context.Statuses.FindAsync(new object[] { request.FromStatusId }, cancellationToken);
        var toStatus = await m_Context.Statuses.FindAsync(new object[] { request.ToStatusId }, cancellationToken);

        if (fromStatus is null)
        {
            throw new NotFoundException($"Source status with ID {request.FromStatusId} not found");
        }

        if (toStatus is null)
        {
            throw new NotFoundException($"Target status with ID {request.ToStatusId} not found");
        }

        // Check for duplicate transition
        var exists = await m_Context.StatusTransitions
            .AnyAsync(t => t.FromStatusId == request.FromStatusId && t.ToStatusId == request.ToStatusId, cancellationToken);

        if (exists)
        {
            throw new BadRequestException(
                $"Transition from '{fromStatus.Name}' to '{toStatus.Name}' already exists");
        }

        var transition = new StatusTransition
        {
            FromStatusId = request.FromStatusId,
            ToStatusId = request.ToStatusId,
            IsAllowed = request.IsAllowed,
            CreatedAt = DateTime.UtcNow
        };

        m_Context.StatusTransitions.Add(transition);
        await m_Context.SaveChangesAsync(cancellationToken);

        return new StatusTransitionDto
        {
            Id = transition.Id,
            FromStatusId = transition.FromStatusId,
            FromStatusName = fromStatus.Name,
            ToStatusId = transition.ToStatusId,
            ToStatusName = toStatus.Name,
            IsAllowed = transition.IsAllowed,
            CreatedAt = transition.CreatedAt
        };
    }

    public async Task CreateDefaultStatusesAsync(CancellationToken cancellationToken = default)
    {
        // Check if statuses already exist
        var hasStatuses = await m_Context.Statuses.AnyAsync(cancellationToken);
        if (hasStatuses)
        {
            return; // Already initialized
        }

        var defaultStatuses = new[]
        {
            new Status
            {
                Name = "TODO",
                Color = "#95a5a6",
                OrderIndex = 1,
                IsDefaultForNew = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new Status
            {
                Name = "IN PROGRESS",
                Color = "#3498db",
                OrderIndex = 2,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new Status
            {
                Name = "DONE",
                Color = "#2ecc71",
                OrderIndex = 3,
                IsCompletedStatus = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new Status
            {
                Name = "CANCELLED",
                Color = "#e74c3c",
                OrderIndex = 4,
                IsCancelledStatus = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new Status
            {
                Name = "BACKLOG",
                Color = "#34495e",
                OrderIndex = 0,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            }
        };

        m_Context.Statuses.AddRange(defaultStatuses);
        await m_Context.SaveChangesAsync(cancellationToken);
    }

    private static StatusDto MapToDto(Status status)
    {
        return new StatusDto
        {
            Id = status.Id,
            Name = status.Name,
            Color = status.Color,
            OrderIndex = status.OrderIndex,
            IsDefaultForNew = status.IsDefaultForNew,
            IsCompletedStatus = status.IsCompletedStatus,
            IsCancelledStatus = status.IsCancelledStatus,
            CreatedAt = status.CreatedAt,
            IsActive = status.IsActive
        };
    }
}
