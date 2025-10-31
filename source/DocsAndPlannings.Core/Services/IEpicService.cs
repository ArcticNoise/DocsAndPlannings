using DocsAndPlannings.Core.DTOs.Epics;

namespace DocsAndPlannings.Core.Services;

public interface IEpicService
{
    Task<EpicDto> CreateEpicAsync(CreateEpicRequest request, int creatorId);
    Task<EpicDto?> GetEpicByIdAsync(int id);
    Task<EpicDto?> GetEpicByKeyAsync(string key);
    Task<IReadOnlyList<EpicListItemDto>> GetAllEpicsAsync(
        int? projectId = null,
        int? statusId = null,
        int? assigneeId = null,
        bool? isActive = null,
        int page = 1,
        int pageSize = 50);
    Task<EpicDto> UpdateEpicAsync(int id, UpdateEpicRequest request, int userId);
    Task DeleteEpicAsync(int id, int userId);
    Task<EpicDto> AssignEpicAsync(int id, int? assigneeId, int userId);
    Task<EpicDto> UpdateEpicStatusAsync(int id, int statusId, int userId);
}
