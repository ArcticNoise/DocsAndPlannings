using DocsAndPlannings.Core.DTOs.Projects;

namespace DocsAndPlannings.Core.Services;

public interface IProjectService
{
    Task<ProjectDto> CreateProjectAsync(CreateProjectRequest request, int ownerId);
    Task<ProjectDto?> GetProjectByIdAsync(int id);
    Task<IReadOnlyList<ProjectListItemDto>> GetAllProjectsAsync(
        int? ownerId = null,
        bool? isActive = null,
        bool? isArchived = null,
        int page = 1,
        int pageSize = 50);
    Task<ProjectDto> UpdateProjectAsync(int id, UpdateProjectRequest request, int userId);
    Task DeleteProjectAsync(int id, int userId);
    Task<ProjectDto> ArchiveProjectAsync(int id, int userId);
    Task<ProjectDto> UnarchiveProjectAsync(int id, int userId);
}
