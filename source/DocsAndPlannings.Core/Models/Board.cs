using System.ComponentModel.DataAnnotations;

namespace DocsAndPlannings.Core.Models;

/// <summary>
/// Represents a Kanban board for a project
/// </summary>
public class Board
{
    public int Id { get; set; }

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    [MaxLength(200)]
    public required string Name { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<BoardColumn> BoardColumns { get; set; } = new List<BoardColumn>();
}
