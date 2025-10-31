using DocsAndPlannings.Core.DTOs.Comments;

namespace DocsAndPlannings.Core.Services;

public interface ICommentService
{
    Task<CommentDto> CreateCommentAsync(int workItemId, CreateCommentRequest request, int authorId);
    Task<IReadOnlyList<CommentDto>> GetCommentsByWorkItemIdAsync(int workItemId);
    Task<CommentDto?> GetCommentByIdAsync(int id);
    Task<CommentDto> UpdateCommentAsync(int id, UpdateCommentRequest request, int userId);
    Task DeleteCommentAsync(int id, int userId);
}
