using DocsAndPlannings.Core.DTOs.Tags;

namespace DocsAndPlannings.Core.Services;

/// <summary>
/// Service interface for managing document tags
/// </summary>
public interface ITagService
{
    /// <summary>
    /// Creates a new tag (admin only)
    /// </summary>
    /// <param name="request">The tag creation request</param>
    /// <returns>The created tag</returns>
    /// <exception cref="BadRequestException">Thrown when tag name already exists</exception>
    Task<TagDto> CreateTagAsync(CreateTagRequest request);

    /// <summary>
    /// Gets a tag by ID
    /// </summary>
    /// <param name="id">The tag ID</param>
    /// <returns>The tag if found</returns>
    /// <exception cref="NotFoundException">Thrown when tag is not found</exception>
    Task<TagDto> GetTagByIdAsync(int id);

    /// <summary>
    /// Gets all tags
    /// </summary>
    /// <returns>List of all tags</returns>
    Task<IReadOnlyList<TagDto>> GetAllTagsAsync();

    /// <summary>
    /// Updates an existing tag (admin only)
    /// </summary>
    /// <param name="id">The tag ID</param>
    /// <param name="request">The update request</param>
    /// <returns>The updated tag</returns>
    /// <exception cref="NotFoundException">Thrown when tag is not found</exception>
    /// <exception cref="BadRequestException">Thrown when updated name already exists</exception>
    Task<TagDto> UpdateTagAsync(int id, UpdateTagRequest request);

    /// <summary>
    /// Deletes a tag (admin only)
    /// </summary>
    /// <param name="id">The tag ID</param>
    /// <exception cref="NotFoundException">Thrown when tag is not found</exception>
    Task DeleteTagAsync(int id);
}
