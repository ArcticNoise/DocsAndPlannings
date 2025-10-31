using DocsAndPlannings.Core.DTOs.WorkItems;

namespace DocsAndPlannings.Core.Services;

public interface IWorkItemService
{
    Task<WorkItemDto> CreateWorkItemAsync(CreateWorkItemRequest request, int creatorId);
    Task<WorkItemDto?> GetWorkItemByIdAsync(int id);
    Task<WorkItemDto?> GetWorkItemByKeyAsync(string key);
    Task<IReadOnlyList<WorkItemListItemDto>> SearchWorkItemsAsync(WorkItemSearchRequest request);
    Task<WorkItemDto> UpdateWorkItemAsync(int id, UpdateWorkItemRequest request, int userId);
    Task DeleteWorkItemAsync(int id, int userId);
    Task<WorkItemDto> AssignWorkItemAsync(int id, int? assigneeId, int userId);
    Task<WorkItemDto> UpdateWorkItemStatusAsync(int id, int statusId, int userId);
    Task<WorkItemDto> UpdateWorkItemParentAsync(int id, int? parentWorkItemId, int userId);
}
