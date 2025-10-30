using System.ComponentModel.DataAnnotations;

namespace DocsAndPlannings.Core.Models;

public class DocumentVersion
{
    public int Id { get; set; }

    public int DocumentId { get; set; }
    public Document Document { get; set; } = null!;

    public int VersionNumber { get; set; }

    [MaxLength(200)]
    public required string Title { get; set; }

    public required string Content { get; set; }

    public int ModifiedById { get; set; }
    public User ModifiedBy { get; set; } = null!;

    [MaxLength(500)]
    public string? ChangeDescription { get; set; }

    public DateTime CreatedAt { get; set; }
}
