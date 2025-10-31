namespace DocsAndPlannings.Core.DTOs.Projects;

/// <summary>
/// Data transfer object for a project in list view (summary)
/// </summary>
public sealed record ProjectListItemDto
{
    /// <summary>
    /// Gets the unique identifier of the project
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets the unique key of the project
    /// </summary>
    public required string Key { get; init; }

    /// <summary>
    /// Gets the name of the project
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the name of the project owner
    /// </summary>
    public required string OwnerName { get; init; }

    /// <summary>
    /// Gets the timestamp when the project was last updated
    /// </summary>
    public required DateTime UpdatedAt { get; init; }

    /// <summary>
    /// Gets whether the project is active
    /// </summary>
    public required bool IsActive { get; init; }

    /// <summary>
    /// Gets whether the project is archived
    /// </summary>
    public required bool IsArchived { get; init; }

    /// <summary>
    /// Gets the count of epics in this project
    /// </summary>
    public int EpicCount { get; init; }

    /// <summary>
    /// Gets the count of work items in this project
    /// </summary>
    public int WorkItemCount { get; init; }
}
