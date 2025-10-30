namespace DocsAndPlannings.Core.Models;

public class WorkItemComment
{
    public int Id { get; set; }

    public int WorkItemId { get; set; }
    public WorkItem WorkItem { get; set; } = null!;

    public int AuthorId { get; set; }
    public User Author { get; set; } = null!;

    public required string Content { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsEdited { get; set; } = false;
}
