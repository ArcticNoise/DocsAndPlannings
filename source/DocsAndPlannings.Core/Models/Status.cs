using System.ComponentModel.DataAnnotations;

namespace DocsAndPlannings.Core.Models;

public class Status
{
    public int Id { get; set; }

    [MaxLength(50)]
    public required string Name { get; set; }

    [MaxLength(20)]
    public string? Color { get; set; }

    public int OrderIndex { get; set; }

    public bool IsDefaultForNew { get; set; } = false;
    public bool IsCompletedStatus { get; set; } = false;
    public bool IsCancelledStatus { get; set; } = false;

    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<StatusTransition> TransitionsFrom { get; set; } = new List<StatusTransition>();
    public ICollection<StatusTransition> TransitionsTo { get; set; } = new List<StatusTransition>();
    public ICollection<Epic> Epics { get; set; } = new List<Epic>();
    public ICollection<WorkItem> WorkItems { get; set; } = new List<WorkItem>();
}
