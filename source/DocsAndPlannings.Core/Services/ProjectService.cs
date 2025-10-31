using DocsAndPlannings.Core.Data;
using DocsAndPlannings.Core.DTOs.Projects;
using DocsAndPlannings.Core.Exceptions;
using DocsAndPlannings.Core.Exceptions.Planning;
using DocsAndPlannings.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DocsAndPlannings.Core.Services;

public sealed class ProjectService : IProjectService
{
    private readonly ApplicationDbContext m_Context;

    public ProjectService(ApplicationDbContext context)
    {
        m_Context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<ProjectDto> CreateProjectAsync(CreateProjectRequest request, int ownerId)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        // Validate unique key
        var exists = await m_Context.Projects
            .AnyAsync(p => p.Key == request.Key);

        if (exists)
        {
            throw new DuplicateKeyException($"Project with key '{request.Key}' already exists");
        }

        var project = new Project
        {
            Key = request.Key,
            Name = request.Name,
            Description = request.Description,
            OwnerId = ownerId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            IsArchived = false
        };

        m_Context.Projects.Add(project);
        await m_Context.SaveChangesAsync();

        return await MapToDtoAsync(project);
    }

    public async Task<ProjectDto?> GetProjectByIdAsync(int id)
    {
        var project = await m_Context.Projects
            .Include(p => p.Owner)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        return project != null ? await MapToDtoAsync(project) : null;
    }

    public async Task<IReadOnlyList<ProjectListItemDto>> GetAllProjectsAsync(
        int? ownerId = null,
        bool? isActive = null,
        bool? isArchived = null,
        int page = 1,
        int pageSize = 50)
    {
        var query = m_Context.Projects
            .Include(p => p.Owner)
            .AsNoTracking()
            .AsQueryable();

        if (ownerId.HasValue)
        {
            query = query.Where(p => p.OwnerId == ownerId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(p => p.IsActive == isActive.Value);
        }

        if (isArchived.HasValue)
        {
            query = query.Where(p => p.IsArchived == isArchived.Value);
        }

        var projects = await query
            .OrderByDescending(p => p.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Single query to get all counts
        var projectIds = projects.Select(p => p.Id).ToList();

        var epicCounts = await m_Context.Epics
            .Where(e => projectIds.Contains(e.ProjectId))
            .GroupBy(e => e.ProjectId)
            .Select(g => new { ProjectId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ProjectId, x => x.Count);

        var workItemCounts = await m_Context.WorkItems
            .Where(w => projectIds.Contains(w.ProjectId))
            .GroupBy(w => w.ProjectId)
            .Select(g => new { ProjectId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ProjectId, x => x.Count);

        return projects.Select(project => new ProjectListItemDto
        {
            Id = project.Id,
            Key = project.Key,
            Name = project.Name,
            OwnerName = $"{project.Owner.FirstName} {project.Owner.LastName}",
            UpdatedAt = project.UpdatedAt,
            IsActive = project.IsActive,
            IsArchived = project.IsArchived,
            EpicCount = epicCounts.GetValueOrDefault(project.Id, 0),
            WorkItemCount = workItemCounts.GetValueOrDefault(project.Id, 0)
        }).ToList();
    }

    public async Task<ProjectDto> UpdateProjectAsync(int id, UpdateProjectRequest request, int userId)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var project = await m_Context.Projects
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project is null)
        {
            throw new NotFoundException($"Project with ID {id} not found");
        }

        // Check ownership
        if (project.OwnerId != userId)
        {
            throw new ForbiddenException("Only the project owner can update the project");
        }

        project.Name = request.Name;
        project.Description = request.Description;
        project.IsActive = request.IsActive;
        project.UpdatedAt = DateTime.UtcNow;

        await m_Context.SaveChangesAsync();

        return await MapToDtoAsync(project);
    }

    public async Task DeleteProjectAsync(int id, int userId)
    {
        var project = await m_Context.Projects
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project is null)
        {
            throw new NotFoundException($"Project with ID {id} not found");
        }

        // Check ownership
        if (project.OwnerId != userId)
        {
            throw new ForbiddenException("Only the project owner can delete the project");
        }

        // Check for dependencies
        var epicCount = await m_Context.Epics.CountAsync(e => e.ProjectId == id);
        var workItemCount = await m_Context.WorkItems.CountAsync(w => w.ProjectId == id);

        if (epicCount > 0 || workItemCount > 0)
        {
            throw new BadRequestException(
                $"Cannot delete project '{project.Name}' because it contains {epicCount} epics and {workItemCount} work items. " +
                "Please delete or reassign them first, or archive the project instead.");
        }

        m_Context.Projects.Remove(project);
        await m_Context.SaveChangesAsync();
    }

    public async Task<ProjectDto> ArchiveProjectAsync(int id, int userId)
    {
        var project = await m_Context.Projects
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project is null)
        {
            throw new NotFoundException($"Project with ID {id} not found");
        }

        if (project.OwnerId != userId)
        {
            throw new ForbiddenException("Only the project owner can archive the project");
        }

        project.IsArchived = true;
        project.UpdatedAt = DateTime.UtcNow;

        await m_Context.SaveChangesAsync();

        return await MapToDtoAsync(project);
    }

    public async Task<ProjectDto> UnarchiveProjectAsync(int id, int userId)
    {
        var project = await m_Context.Projects
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project is null)
        {
            throw new NotFoundException($"Project with ID {id} not found");
        }

        if (project.OwnerId != userId)
        {
            throw new ForbiddenException("Only the project owner can unarchive the project");
        }

        project.IsArchived = false;
        project.UpdatedAt = DateTime.UtcNow;

        await m_Context.SaveChangesAsync();

        return await MapToDtoAsync(project);
    }

    private async Task<ProjectDto> MapToDtoAsync(Project project)
    {
        var epicCount = await m_Context.Epics.CountAsync(e => e.ProjectId == project.Id);
        var workItemCount = await m_Context.WorkItems.CountAsync(w => w.ProjectId == project.Id);

        return new ProjectDto
        {
            Id = project.Id,
            Key = project.Key,
            Name = project.Name,
            Description = project.Description,
            OwnerId = project.OwnerId,
            OwnerName = $"{project.Owner.FirstName} {project.Owner.LastName}",
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt,
            IsActive = project.IsActive,
            IsArchived = project.IsArchived,
            EpicCount = epicCount,
            WorkItemCount = workItemCount
        };
    }
}
