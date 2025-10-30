namespace DocsAndPlannings.Core.Models;

public class StatusTransition
{
    public int Id { get; set; }

    public int FromStatusId { get; set; }
    public Status FromStatus { get; set; } = null!;

    public int ToStatusId { get; set; }
    public Status ToStatus { get; set; } = null!;

    public bool IsAllowed { get; set; } = true;

    public DateTime CreatedAt { get; set; }
}
