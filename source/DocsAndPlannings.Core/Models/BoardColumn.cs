using System.ComponentModel.DataAnnotations;

namespace DocsAndPlannings.Core.Models;

/// <summary>
/// Represents a column configuration in a Kanban board
/// </summary>
public class BoardColumn
{
    public int Id { get; set; }

    public int BoardId { get; set; }
    public Board Board { get; set; } = null!;

    public int StatusId { get; set; }
    public Status Status { get; set; } = null!;

    public int OrderIndex { get; set; }

    public int? WIPLimit { get; set; }

    public bool IsCollapsed { get; set; } = false;

    [Timestamp]
    public byte[] RowVersion { get; set; } = null!;
}
