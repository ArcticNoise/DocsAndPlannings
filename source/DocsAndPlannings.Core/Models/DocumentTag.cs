using System.ComponentModel.DataAnnotations;

namespace DocsAndPlannings.Core.Models;

public class DocumentTag
{
    public int Id { get; set; }

    [MaxLength(50)]
    public required string Name { get; set; }

    [MaxLength(20)]
    public string? Color { get; set; }

    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<DocumentTagMap> Documents { get; set; } = new List<DocumentTagMap>();
}
