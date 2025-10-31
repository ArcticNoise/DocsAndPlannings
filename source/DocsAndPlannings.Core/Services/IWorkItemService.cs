using DocsAndPlannings.Core.DTOs.WorkItems;

namespace DocsAndPlannings.Core.Services;

/// <summary>
/// Service for managing work items (tasks, subtasks, bugs) in the planning/tracking system
/// </summary>
public interface IWorkItemService
{
    /// <summary>
    /// Creates a new work item with hierarchy validation
    /// </summary>
    /// <param name="request">The work item creation request containing work item details</param>
    /// <param name="creatorId">The ID of the user creating the work item</param>
    /// <returns>The created work item with generated key and metadata</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null</exception>
    /// <exception cref="NotFoundException">Thrown when project, epic, parent, assignee, or reporter is not found</exception>
    /// <exception cref="BadRequestException">Thrown when no default status is found</exception>
    /// <exception cref="InvalidHierarchyException">Thrown when hierarchy rules are violated (e.g., task with parent)</exception>
    Task<WorkItemDto> CreateWorkItemAsync(CreateWorkItemRequest request, int creatorId);

    /// <summary>
    /// Retrieves a work item by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the work item</param>
    /// <returns>The work item if found, otherwise null</returns>
    Task<WorkItemDto?> GetWorkItemByIdAsync(int id);

    /// <summary>
    /// Retrieves a work item by its unique key (e.g., "PROJ-123")
    /// </summary>
    /// <param name="key">The unique key of the work item</param>
    /// <returns>The work item if found, otherwise null</returns>
    /// <exception cref="ArgumentException">Thrown when key is null or empty</exception>
    Task<WorkItemDto?> GetWorkItemByKeyAsync(string key);

    /// <summary>
    /// Searches work items with advanced filtering and pagination
    /// </summary>
    /// <param name="request">The search request containing filter criteria and pagination</param>
    /// <returns>A list of work items matching the search criteria</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null</exception>
    Task<IReadOnlyList<WorkItemListItemDto>> SearchWorkItemsAsync(WorkItemSearchRequest request);

    /// <summary>
    /// Updates an existing work item with new details
    /// </summary>
    /// <param name="id">The unique identifier of the work item to update</param>
    /// <param name="request">The update request containing new work item details</param>
    /// <param name="userId">The ID of the user performing the update</param>
    /// <returns>The updated work item</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null</exception>
    /// <exception cref="NotFoundException">Thrown when work item, status, epic, assignee, or reporter is not found</exception>
    Task<WorkItemDto> UpdateWorkItemAsync(int id, UpdateWorkItemRequest request, int userId);

    /// <summary>
    /// Deletes a work item if it has no dependent child work items
    /// </summary>
    /// <param name="id">The unique identifier of the work item to delete</param>
    /// <param name="userId">The ID of the user performing the deletion</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="NotFoundException">Thrown when work item is not found</exception>
    /// <exception cref="BadRequestException">Thrown when work item has child work items</exception>
    Task DeleteWorkItemAsync(int id, int userId);

    /// <summary>
    /// Assigns or unassigns a work item to/from a user
    /// </summary>
    /// <param name="id">The unique identifier of the work item</param>
    /// <param name="assigneeId">The ID of the user to assign, or null to unassign</param>
    /// <param name="userId">The ID of the user performing the assignment</param>
    /// <returns>The updated work item</returns>
    /// <exception cref="NotFoundException">Thrown when work item or assignee is not found</exception>
    Task<WorkItemDto> AssignWorkItemAsync(int id, int? assigneeId, int userId);

    /// <summary>
    /// Updates the status of a work item with transition validation
    /// </summary>
    /// <param name="id">The unique identifier of the work item</param>
    /// <param name="statusId">The ID of the new status</param>
    /// <param name="userId">The ID of the user performing the status update</param>
    /// <returns>The updated work item</returns>
    /// <exception cref="NotFoundException">Thrown when work item or status is not found</exception>
    /// <exception cref="InvalidStatusTransitionException">Thrown when status transition is not allowed</exception>
    Task<WorkItemDto> UpdateWorkItemStatusAsync(int id, int statusId, int userId);

    /// <summary>
    /// Updates the parent-child relationship of a work item with circular reference detection
    /// </summary>
    /// <param name="id">The unique identifier of the work item</param>
    /// <param name="parentWorkItemId">The ID of the new parent work item, or null to remove parent</param>
    /// <param name="userId">The ID of the user performing the update</param>
    /// <returns>The updated work item</returns>
    /// <exception cref="NotFoundException">Thrown when work item or parent work item is not found</exception>
    /// <exception cref="InvalidHierarchyException">Thrown when hierarchy rules are violated</exception>
    /// <exception cref="CircularHierarchyException">Thrown when circular reference is detected</exception>
    Task<WorkItemDto> UpdateWorkItemParentAsync(int id, int? parentWorkItemId, int userId);
}
