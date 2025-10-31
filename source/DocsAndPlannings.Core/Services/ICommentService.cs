using DocsAndPlannings.Core.DTOs.Comments;

namespace DocsAndPlannings.Core.Services;

/// <summary>
/// Service for managing comments on work items in the planning/tracking system
/// </summary>
public interface ICommentService
{
    /// <summary>
    /// Creates a new comment on a work item
    /// </summary>
    /// <param name="workItemId">The ID of the work item to comment on</param>
    /// <param name="request">The comment creation request containing comment details</param>
    /// <param name="authorId">The ID of the user creating the comment</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>The created comment with generated metadata</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null</exception>
    /// <exception cref="NotFoundException">Thrown when work item is not found</exception>
    Task<CommentDto> CreateCommentAsync(int workItemId, CreateCommentRequest request, int authorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all comments for a specific work item, ordered by creation date
    /// </summary>
    /// <param name="workItemId">The ID of the work item</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>A list of comments for the work item</returns>
    Task<IReadOnlyList<CommentDto>> GetCommentsByWorkItemIdAsync(int workItemId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a comment by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the comment</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>The comment if found, otherwise null</returns>
    Task<CommentDto?> GetCommentByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing comment (only the author can edit)
    /// </summary>
    /// <param name="id">The unique identifier of the comment to update</param>
    /// <param name="request">The update request containing new comment text</param>
    /// <param name="userId">The ID of the user performing the update</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>The updated comment</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null</exception>
    /// <exception cref="NotFoundException">Thrown when comment is not found</exception>
    /// <exception cref="ForbiddenException">Thrown when user is not the comment author</exception>
    Task<CommentDto> UpdateCommentAsync(int id, UpdateCommentRequest request, int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a comment (only the author can delete)
    /// </summary>
    /// <param name="id">The unique identifier of the comment to delete</param>
    /// <param name="userId">The ID of the user performing the deletion</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="NotFoundException">Thrown when comment is not found</exception>
    /// <exception cref="ForbiddenException">Thrown when user is not the comment author</exception>
    Task DeleteCommentAsync(int id, int userId, CancellationToken cancellationToken = default);
}
