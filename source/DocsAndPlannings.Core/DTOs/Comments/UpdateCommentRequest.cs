using System.ComponentModel.DataAnnotations;

namespace DocsAndPlannings.Core.DTOs.Comments;

/// <summary>
/// Request model for updating an existing comment
/// </summary>
public sealed record UpdateCommentRequest
{
    /// <summary>
    /// Gets the content of the comment
    /// </summary>
    [Required]
    [MaxLength(5000)]
    public required string Content { get; init; }
}
