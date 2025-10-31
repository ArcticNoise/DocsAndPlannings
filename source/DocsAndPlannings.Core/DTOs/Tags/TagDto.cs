namespace DocsAndPlannings.Core.DTOs.Tags;

/// <summary>
/// Data transfer object for a tag
/// </summary>
public sealed record TagDto
{
    /// <summary>
    /// Gets the unique identifier of the tag
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets the name of the tag
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the optional color code for the tag (e.g., #FF5733)
    /// </summary>
    public string? Color { get; init; }

    /// <summary>
    /// Gets the timestamp when the tag was created
    /// </summary>
    public required DateTime CreatedAt { get; init; }
}
