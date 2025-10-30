using System.ComponentModel.DataAnnotations;

namespace DocsAndPlannings.Core.Models;

public class Epic
{
    public int Id { get; set; }

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    [MaxLength(50)]
    public required string Key { get; set; }

    [MaxLength(200)]
    public required string Summary { get; set; }

    public string? Description { get; set; }

    public int? AssigneeId { get; set; }
    public User? Assignee { get; set; }

    public int StatusId { get; set; }
    public Status Status { get; set; } = null!;

    public int Priority { get; set; } = 3;

    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<WorkItem> WorkItems { get; set; } = new List<WorkItem>();
}
