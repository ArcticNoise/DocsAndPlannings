using DocsAndPlannings.Core.DTOs.Epics;

namespace DocsAndPlannings.Core.Services;

/// <summary>
/// Service for managing epics in the planning/tracking system
/// </summary>
public interface IEpicService
{
    /// <summary>
    /// Creates a new epic with the specified details
    /// </summary>
    /// <param name="request">The epic creation request containing epic details</param>
    /// <param name="creatorId">The ID of the user creating the epic</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>The created epic with generated key and metadata</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null</exception>
    /// <exception cref="NotFoundException">Thrown when project or assignee is not found</exception>
    /// <exception cref="BadRequestException">Thrown when no default status is found</exception>
    Task<EpicDto> CreateEpicAsync(CreateEpicRequest request, int creatorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an epic by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the epic</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>The epic if found, otherwise null</returns>
    Task<EpicDto?> GetEpicByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an epic by its unique key (e.g., "PROJ-E-1")
    /// </summary>
    /// <param name="key">The unique key of the epic</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>The epic if found, otherwise null</returns>
    /// <exception cref="ArgumentException">Thrown when key is null or empty</exception>
    Task<EpicDto?> GetEpicByKeyAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated list of epics with optional filtering
    /// </summary>
    /// <param name="projectId">Optional filter by project ID</param>
    /// <param name="statusId">Optional filter by status ID</param>
    /// <param name="assigneeId">Optional filter by assignee ID</param>
    /// <param name="isActive">Optional filter by active status (not currently used)</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page (max 50)</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>A list of epics matching the specified criteria</returns>
    Task<IReadOnlyList<EpicListItemDto>> GetAllEpicsAsync(
        int? projectId = null,
        int? statusId = null,
        int? assigneeId = null,
        bool? isActive = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing epic with new details
    /// </summary>
    /// <param name="id">The unique identifier of the epic to update</param>
    /// <param name="request">The update request containing new epic details</param>
    /// <param name="userId">The ID of the user performing the update</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>The updated epic</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null</exception>
    /// <exception cref="NotFoundException">Thrown when epic, status, or assignee is not found</exception>
    Task<EpicDto> UpdateEpicAsync(int id, UpdateEpicRequest request, int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an epic if it has no dependent work items
    /// </summary>
    /// <param name="id">The unique identifier of the epic to delete</param>
    /// <param name="userId">The ID of the user performing the deletion</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="NotFoundException">Thrown when epic is not found</exception>
    /// <exception cref="BadRequestException">Thrown when epic contains work items</exception>
    Task DeleteEpicAsync(int id, int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns or unassigns an epic to/from a user
    /// </summary>
    /// <param name="id">The unique identifier of the epic</param>
    /// <param name="assigneeId">The ID of the user to assign, or null to unassign</param>
    /// <param name="userId">The ID of the user performing the assignment</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>The updated epic</returns>
    /// <exception cref="NotFoundException">Thrown when epic or assignee is not found</exception>
    Task<EpicDto> AssignEpicAsync(int id, int? assigneeId, int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the status of an epic with transition validation
    /// </summary>
    /// <param name="id">The unique identifier of the epic</param>
    /// <param name="statusId">The ID of the new status</param>
    /// <param name="userId">The ID of the user performing the status update</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>The updated epic</returns>
    /// <exception cref="NotFoundException">Thrown when epic or status is not found</exception>
    /// <exception cref="InvalidStatusTransitionException">Thrown when status transition is not allowed</exception>
    Task<EpicDto> UpdateEpicStatusAsync(int id, int statusId, int userId, CancellationToken cancellationToken = default);
}
