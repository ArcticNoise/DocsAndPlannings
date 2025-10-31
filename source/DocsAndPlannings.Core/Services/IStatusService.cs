using DocsAndPlannings.Core.DTOs.Statuses;

namespace DocsAndPlannings.Core.Services;

/// <summary>
/// Service for managing statuses and status transitions
/// </summary>
public interface IStatusService
{
    /// <summary>
    /// Gets all statuses ordered by OrderIndex
    /// </summary>
    /// <returns>A list of all statuses</returns>
    Task<IReadOnlyList<StatusDto>> GetAllStatusesAsync();

    /// <summary>
    /// Gets a status by ID
    /// </summary>
    /// <param name="id">The status ID</param>
    /// <returns>The status if found</returns>
    Task<StatusDto?> GetStatusByIdAsync(int id);

    /// <summary>
    /// Creates a new status
    /// </summary>
    /// <param name="request">The create request</param>
    /// <returns>The created status</returns>
    Task<StatusDto> CreateStatusAsync(CreateStatusRequest request);

    /// <summary>
    /// Updates an existing status
    /// </summary>
    /// <param name="id">The status ID</param>
    /// <param name="request">The update request</param>
    /// <returns>The updated status</returns>
    Task<StatusDto> UpdateStatusAsync(int id, UpdateStatusRequest request);

    /// <summary>
    /// Deletes a status
    /// </summary>
    /// <param name="id">The status ID</param>
    /// <returns>A task representing the operation</returns>
    Task DeleteStatusAsync(int id);

    /// <summary>
    /// Gets all allowed transitions from a status
    /// </summary>
    /// <param name="fromStatusId">The source status ID</param>
    /// <returns>A list of allowed target statuses</returns>
    Task<IReadOnlyList<StatusDto>> GetAllowedTransitionsAsync(int fromStatusId);

    /// <summary>
    /// Validates if a status transition is allowed
    /// </summary>
    /// <param name="fromStatusId">The source status ID</param>
    /// <param name="toStatusId">The target status ID</param>
    /// <returns>True if the transition is allowed</returns>
    Task<bool> ValidateTransitionAsync(int fromStatusId, int toStatusId);

    /// <summary>
    /// Creates a status transition rule
    /// </summary>
    /// <param name="request">The transition request</param>
    /// <returns>The created transition</returns>
    Task<StatusTransitionDto> CreateStatusTransitionAsync(CreateStatusTransitionRequest request);

    /// <summary>
    /// Creates default statuses for a new installation
    /// </summary>
    /// <returns>A task representing the operation</returns>
    Task CreateDefaultStatusesAsync();
}
