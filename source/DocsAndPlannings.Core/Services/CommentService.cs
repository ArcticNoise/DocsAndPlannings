using DocsAndPlannings.Core.Data;
using DocsAndPlannings.Core.DTOs.Comments;
using DocsAndPlannings.Core.Exceptions;
using DocsAndPlannings.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DocsAndPlannings.Core.Services;

public sealed class CommentService : ICommentService
{
    private readonly ApplicationDbContext m_Context;

    public CommentService(ApplicationDbContext context)
    {
        m_Context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<CommentDto> CreateCommentAsync(int workItemId, CreateCommentRequest request, int authorId, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        // Validate work item exists
        var workItem = await m_Context.WorkItems.FindAsync(new object[] { workItemId }, cancellationToken);
        if (workItem is null)
        {
            throw new NotFoundException($"Work item with ID {workItemId} not found");
        }

        // Validate author exists
        var author = await m_Context.Users.FindAsync(new object[] { authorId }, cancellationToken);
        if (author is null)
        {
            throw new NotFoundException($"User with ID {authorId} not found");
        }

        var comment = new WorkItemComment
        {
            WorkItemId = workItemId,
            AuthorId = authorId,
            Content = request.Content,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsEdited = false
        };

        m_Context.WorkItemComments.Add(comment);
        await m_Context.SaveChangesAsync(cancellationToken);

        return MapToDto(comment, author);
    }

    public async Task<IReadOnlyList<CommentDto>> GetCommentsByWorkItemIdAsync(int workItemId, CancellationToken cancellationToken = default)
    {
        var comments = await m_Context.WorkItemComments
            .Include(c => c.Author)
            .AsNoTracking()
            .Where(c => c.WorkItemId == workItemId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        return comments.Select(c => MapToDto(c, c.Author)).ToList();
    }

    public async Task<CommentDto?> GetCommentByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var comment = await m_Context.WorkItemComments
            .Include(c => c.Author)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        return comment != null ? MapToDto(comment, comment.Author) : null;
    }

    public async Task<CommentDto> UpdateCommentAsync(int id, UpdateCommentRequest request, int userId, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var comment = await m_Context.WorkItemComments
            .Include(c => c.Author)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (comment is null)
        {
            throw new NotFoundException($"Comment with ID {id} not found");
        }

        // Check authorization - only the author can update the comment
        if (comment.AuthorId != userId)
        {
            throw new ForbiddenException("Only the comment author can update the comment");
        }

        comment.Content = request.Content;
        comment.UpdatedAt = DateTime.UtcNow;
        comment.IsEdited = true;

        await m_Context.SaveChangesAsync(cancellationToken);

        return MapToDto(comment, comment.Author);
    }

    public async Task DeleteCommentAsync(int id, int userId, CancellationToken cancellationToken = default)
    {
        var comment = await m_Context.WorkItemComments
            .Include(c => c.Author)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (comment is null)
        {
            throw new NotFoundException($"Comment with ID {id} not found");
        }

        // Check authorization - only the author can delete the comment
        // Note: In a real application, you might also allow admins to delete comments
        if (comment.AuthorId != userId)
        {
            throw new ForbiddenException("Only the comment author can delete the comment");
        }

        m_Context.WorkItemComments.Remove(comment);
        await m_Context.SaveChangesAsync(cancellationToken);
    }

    private static CommentDto MapToDto(WorkItemComment comment, User author)
    {
        return new CommentDto
        {
            Id = comment.Id,
            WorkItemId = comment.WorkItemId,
            AuthorId = comment.AuthorId,
            AuthorName = $"{author.FirstName} {author.LastName}",
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt,
            IsEdited = comment.IsEdited
        };
    }
}
