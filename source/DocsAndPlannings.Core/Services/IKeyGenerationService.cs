namespace DocsAndPlannings.Core.Services;

/// <summary>
/// Service for generating unique keys for projects, epics, and work items
/// </summary>
public interface IKeyGenerationService
{
    /// <summary>
    /// Generates a unique key for an epic in the format: {PROJECT_KEY}-EPIC-{number}
    /// </summary>
    /// <param name="projectId">The ID of the project</param>
    /// <returns>A unique epic key</returns>
    Task<string> GenerateEpicKeyAsync(int projectId);

    /// <summary>
    /// Generates a unique key for a work item in the format: {PROJECT_KEY}-{number}
    /// </summary>
    /// <param name="projectId">The ID of the project</param>
    /// <returns>A unique work item key</returns>
    Task<string> GenerateWorkItemKeyAsync(int projectId);
}
