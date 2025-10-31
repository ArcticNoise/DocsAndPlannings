namespace DocsAndPlannings.Core.DTOs.Documents;

/// <summary>
/// Data transfer object for document tag
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
}
