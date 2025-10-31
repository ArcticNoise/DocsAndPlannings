using Microsoft.EntityFrameworkCore;
using DocsAndPlannings.Core.Data;

namespace DocsAndPlannings.Core.Services;

/// <summary>
/// Service for generating unique keys for projects, epics, and work items
/// </summary>
public class KeyGenerationService : IKeyGenerationService
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the KeyGenerationService class
    /// </summary>
    /// <param name="context">The database context</param>
    public KeyGenerationService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Generates a unique key for an epic in the format: {PROJECT_KEY}-EPIC-{number}
    /// </summary>
    /// <param name="projectId">The ID of the project</param>
    /// <returns>A unique epic key</returns>
    public async Task<string> GenerateEpicKeyAsync(int projectId)
    {
        var project = await _context.Projects
            .AsNoTracking()
            .Where(p => p.Id == projectId)
            .Select(p => p.Key)
            .FirstOrDefaultAsync();

        if (project == null)
        {
            throw new InvalidOperationException($"Project with ID {projectId} not found");
        }

        // Find the highest epic number for this project
        var prefix = $"{project}-EPIC-";
        var maxNumber = await _context.Epics
            .AsNoTracking()
            .Where(e => e.ProjectId == projectId && e.Key.StartsWith(prefix))
            .Select(e => e.Key)
            .ToListAsync()
            .ContinueWith(task =>
            {
                var keys = task.Result;
                if (!keys.Any())
                {
                    return 0;
                }

                return keys
                    .Select(k => k.Substring(prefix.Length))
                    .Where(s => int.TryParse(s, out _))
                    .Select(int.Parse)
                    .DefaultIfEmpty(0)
                    .Max();
            });

        var nextNumber = maxNumber + 1;
        return $"{prefix}{nextNumber}";
    }

    /// <summary>
    /// Generates a unique key for a work item in the format: {PROJECT_KEY}-{number}
    /// </summary>
    /// <param name="projectId">The ID of the project</param>
    /// <returns>A unique work item key</returns>
    public async Task<string> GenerateWorkItemKeyAsync(int projectId)
    {
        var project = await _context.Projects
            .AsNoTracking()
            .Where(p => p.Id == projectId)
            .Select(p => p.Key)
            .FirstOrDefaultAsync();

        if (project == null)
        {
            throw new InvalidOperationException($"Project with ID {projectId} not found");
        }

        // Find the highest work item number for this project
        var prefix = $"{project}-";
        var maxNumber = await _context.WorkItems
            .AsNoTracking()
            .Where(w => w.ProjectId == projectId && w.Key.StartsWith(prefix))
            .Select(w => w.Key)
            .ToListAsync()
            .ContinueWith(task =>
            {
                var keys = task.Result;
                if (!keys.Any())
                {
                    return 0;
                }

                return keys
                    .Select(k => k.Substring(prefix.Length))
                    .Where(s => int.TryParse(s, out _))
                    .Select(int.Parse)
                    .DefaultIfEmpty(0)
                    .Max();
            });

        var nextNumber = maxNumber + 1;
        return $"{prefix}{nextNumber}";
    }
}
