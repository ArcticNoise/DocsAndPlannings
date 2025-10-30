using System.ComponentModel.DataAnnotations;

namespace DocsAndPlannings.Core.Models;

public class Document
{
    public int Id { get; set; }

    [MaxLength(200)]
    public required string Title { get; set; }

    public required string Content { get; set; }

    public int? ParentDocumentId { get; set; }
    public Document? ParentDocument { get; set; }

    public int AuthorId { get; set; }
    public User Author { get; set; } = null!;

    public int CurrentVersion { get; set; } = 1;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsPublished { get; set; } = false;
    public bool IsDeleted { get; set; } = false;

    public ICollection<Document> ChildDocuments { get; set; } = new List<Document>();
    public ICollection<DocumentVersion> Versions { get; set; } = new List<DocumentVersion>();
    public ICollection<DocumentTagMap> Tags { get; set; } = new List<DocumentTagMap>();
}
