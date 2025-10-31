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
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>A list of all statuses</returns>
    Task<IReadOnlyList<StatusDto>> GetAllStatusesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a status by ID
    /// </summary>
    /// <param name="id">The status ID</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>The status if found</returns>
    Task<StatusDto?> GetStatusByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new status
    /// </summary>
    /// <param name="request">The create request</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>The created status</returns>
    Task<StatusDto> CreateStatusAsync(CreateStatusRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing status
    /// </summary>
    /// <param name="id">The status ID</param>
    /// <param name="request">The update request</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>The updated status</returns>
    Task<StatusDto> UpdateStatusAsync(int id, UpdateStatusRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a status
    /// </summary>
    /// <param name="id">The status ID</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>A task representing the operation</returns>
    Task DeleteStatusAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all allowed transitions from a status
    /// </summary>
    /// <param name="fromStatusId">The source status ID</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>A list of allowed target statuses</returns>
    Task<IReadOnlyList<StatusDto>> GetAllowedTransitionsAsync(int fromStatusId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if a status transition is allowed
    /// </summary>
    /// <param name="fromStatusId">The source status ID</param>
    /// <param name="toStatusId">The target status ID</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>True if the transition is allowed</returns>
    Task<bool> ValidateTransitionAsync(int fromStatusId, int toStatusId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a status transition rule
    /// </summary>
    /// <param name="request">The transition request</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>The created transition</returns>
    Task<StatusTransitionDto> CreateStatusTransitionAsync(CreateStatusTransitionRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates default statuses for a new installation
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>A task representing the operation</returns>
    Task CreateDefaultStatusesAsync(CancellationToken cancellationToken = default);
}
