namespace DocsAndPlannings.Core.Models;

public class DocumentTagMap
{
    public int DocumentId { get; set; }
    public Document Document { get; set; } = null!;

    public int TagId { get; set; }
    public DocumentTag Tag { get; set; } = null!;

    public DateTime AssignedAt { get; set; }
}
