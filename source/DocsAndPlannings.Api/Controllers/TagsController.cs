using DocsAndPlannings.Core.DTOs.Tags;
using DocsAndPlannings.Core.Exceptions;
using DocsAndPlannings.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocsAndPlannings.Api.Controllers;

/// <summary>
/// Controller for managing document tags
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class TagsController : ControllerBase
{
    private readonly ITagService _tagService;
    private readonly ILogger<TagsController> _logger;

    /// <summary>
    /// Initializes a new instance of the TagsController class
    /// </summary>
    public TagsController(
        ITagService tagService,
        ILogger<TagsController> logger)
    {
        _tagService = tagService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new tag (admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TagDto>> CreateTag([FromBody] CreateTagRequest request)
    {
        try
        {
            TagDto tag = await _tagService.CreateTagAsync(request);
            _logger.LogInformation("Tag created: {TagId} - {TagName}", tag.Id, tag.Name);
            return CreatedAtAction(nameof(GetTagById), new { id = tag.Id }, tag);
        }
        catch (BadRequestException ex)
        {
            _logger.LogWarning("Tag creation failed: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during tag creation");
            return StatusCode(500, new { error = "An error occurred while creating the tag" });
        }
    }

    /// <summary>
    /// Gets a tag by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<TagDto>> GetTagById(int id)
    {
        try
        {
            TagDto tag = await _tagService.GetTagByIdAsync(id);
            return Ok(tag);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving tag {TagId}", id);
            return StatusCode(500, new { error = "An error occurred while retrieving the tag" });
        }
    }

    /// <summary>
    /// Gets all tags
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TagDto>>> GetAllTags()
    {
        try
        {
            IReadOnlyList<TagDto> tags = await _tagService.GetAllTagsAsync();
            return Ok(tags);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving all tags");
            return StatusCode(500, new { error = "An error occurred while retrieving tags" });
        }
    }

    /// <summary>
    /// Updates an existing tag (admin only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TagDto>> UpdateTag(int id, [FromBody] UpdateTagRequest request)
    {
        try
        {
            TagDto tag = await _tagService.UpdateTagAsync(id, request);
            _logger.LogInformation("Tag updated: {TagId} - {TagName}", tag.Id, tag.Name);
            return Ok(tag);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Tag update failed: {Message}", ex.Message);
            return NotFound(new { error = ex.Message });
        }
        catch (BadRequestException ex)
        {
            _logger.LogWarning("Tag update failed: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating tag {TagId}", id);
            return StatusCode(500, new { error = "An error occurred while updating the tag" });
        }
    }

    /// <summary>
    /// Deletes a tag (admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteTag(int id)
    {
        try
        {
            await _tagService.DeleteTagAsync(id);
            _logger.LogInformation("Tag deleted: {TagId}", id);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Tag deletion failed: {Message}", ex.Message);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting tag {TagId}", id);
            return StatusCode(500, new { error = "An error occurred while deleting the tag" });
        }
    }
}
