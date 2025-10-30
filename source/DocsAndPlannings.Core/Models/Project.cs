using System.ComponentModel.DataAnnotations;

namespace DocsAndPlannings.Core.Models;

public class Project
{
    public int Id { get; set; }

    [MaxLength(10)]
    public required string Key { get; set; }

    [MaxLength(200)]
    public required string Name { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public int OwnerId { get; set; }
    public User Owner { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsArchived { get; set; } = false;

    public ICollection<Epic> Epics { get; set; } = new List<Epic>();
    public ICollection<WorkItem> WorkItems { get; set; } = new List<WorkItem>();
}
