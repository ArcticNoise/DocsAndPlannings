using System.ComponentModel.DataAnnotations;

namespace DocsAndPlannings.Core.Models;

public class WorkItem
{
    public int Id { get; set; }

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public int? EpicId { get; set; }
    public Epic? Epic { get; set; }

    public int? ParentWorkItemId { get; set; }
    public WorkItem? ParentWorkItem { get; set; }

    [MaxLength(50)]
    public required string Key { get; set; }

    public WorkItemType Type { get; set; }

    [MaxLength(200)]
    public required string Summary { get; set; }

    public string? Description { get; set; }

    public int? AssigneeId { get; set; }
    public User? Assignee { get; set; }

    public int? ReporterId { get; set; }
    public User? Reporter { get; set; }

    public int StatusId { get; set; }
    public Status Status { get; set; } = null!;

    public int Priority { get; set; } = 3;

    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<WorkItem> ChildWorkItems { get; set; } = new List<WorkItem>();
    public ICollection<WorkItemComment> Comments { get; set; } = new List<WorkItemComment>();
}
