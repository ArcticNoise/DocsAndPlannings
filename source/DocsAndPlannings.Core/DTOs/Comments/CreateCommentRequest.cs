using System.ComponentModel.DataAnnotations;

namespace DocsAndPlannings.Core.DTOs.Comments;

/// <summary>
/// Request model for creating a new comment on a work item
/// </summary>
public sealed record CreateCommentRequest
{
    /// <summary>
    /// Gets the content of the comment
    /// </summary>
    [Required]
    [MaxLength(5000)]
    public required string Content { get; init; }
}
