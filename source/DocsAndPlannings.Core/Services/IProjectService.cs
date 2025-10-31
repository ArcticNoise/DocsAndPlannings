using DocsAndPlannings.Core.DTOs.Projects;

namespace DocsAndPlannings.Core.Services;

/// <summary>
/// Service for managing projects in the planning/tracking system
/// </summary>
public interface IProjectService
{
    /// <summary>
    /// Creates a new project with the specified details
    /// </summary>
    /// <param name="request">The project creation request containing project details</param>
    /// <param name="ownerId">The ID of the user who will own the project</param>
    /// <returns>The created project with generated ID and metadata</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null</exception>
    /// <exception cref="DuplicateKeyException">Thrown when a project with the same key already exists</exception>
    Task<ProjectDto> CreateProjectAsync(CreateProjectRequest request, int ownerId);

    /// <summary>
    /// Retrieves a project by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the project</param>
    /// <returns>The project if found, otherwise null</returns>
    Task<ProjectDto?> GetProjectByIdAsync(int id);

    /// <summary>
    /// Retrieves a paginated list of projects with optional filtering
    /// </summary>
    /// <param name="ownerId">Optional filter by project owner ID</param>
    /// <param name="isActive">Optional filter by active status</param>
    /// <param name="isArchived">Optional filter by archived status</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page (max 50)</param>
    /// <returns>A list of projects matching the specified criteria</returns>
    Task<IReadOnlyList<ProjectListItemDto>> GetAllProjectsAsync(
        int? ownerId = null,
        bool? isActive = null,
        bool? isArchived = null,
        int page = 1,
        int pageSize = 50);

    /// <summary>
    /// Updates an existing project with new details
    /// </summary>
    /// <param name="id">The unique identifier of the project to update</param>
    /// <param name="request">The update request containing new project details</param>
    /// <param name="userId">The ID of the user performing the update</param>
    /// <returns>The updated project</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null</exception>
    /// <exception cref="NotFoundException">Thrown when project is not found</exception>
    /// <exception cref="ForbiddenException">Thrown when user is not the project owner</exception>
    Task<ProjectDto> UpdateProjectAsync(int id, UpdateProjectRequest request, int userId);

    /// <summary>
    /// Deletes a project if it has no dependent epics or work items
    /// </summary>
    /// <param name="id">The unique identifier of the project to delete</param>
    /// <param name="userId">The ID of the user performing the deletion</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="NotFoundException">Thrown when project is not found</exception>
    /// <exception cref="ForbiddenException">Thrown when user is not the project owner</exception>
    /// <exception cref="BadRequestException">Thrown when project contains epics or work items</exception>
    Task DeleteProjectAsync(int id, int userId);

    /// <summary>
    /// Archives a project, preventing further modifications
    /// </summary>
    /// <param name="id">The unique identifier of the project to archive</param>
    /// <param name="userId">The ID of the user performing the archive operation</param>
    /// <returns>The archived project</returns>
    /// <exception cref="NotFoundException">Thrown when project is not found</exception>
    /// <exception cref="ForbiddenException">Thrown when user is not the project owner</exception>
    Task<ProjectDto> ArchiveProjectAsync(int id, int userId);

    /// <summary>
    /// Unarchives a project, allowing modifications again
    /// </summary>
    /// <param name="id">The unique identifier of the project to unarchive</param>
    /// <param name="userId">The ID of the user performing the unarchive operation</param>
    /// <returns>The unarchived project</returns>
    /// <exception cref="NotFoundException">Thrown when project is not found</exception>
    /// <exception cref="ForbiddenException">Thrown when user is not the project owner</exception>
    Task<ProjectDto> UnarchiveProjectAsync(int id, int userId);
}
