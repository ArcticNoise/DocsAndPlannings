using System.Diagnostics;
using DocsAndPlannings.Core.Data;
using DocsAndPlannings.Core.DTOs.Tags;
using DocsAndPlannings.Core.Exceptions;
using DocsAndPlannings.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DocsAndPlannings.Core.Services;

/// <summary>
/// Service for managing document tags
/// </summary>
public sealed class TagService : ITagService
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the TagService class
    /// </summary>
    /// <param name="context">The database context</param>
    public TagService(ApplicationDbContext context)
    {
        Debug.Assert(context != null, "Database context cannot be null");
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<TagDto> CreateTagAsync(CreateTagRequest request)
    {
        Debug.Assert(request != null, "Create tag request cannot be null");

        // Check for duplicate tag name (case-insensitive)
        bool nameExists = await _context.DocumentTags
            .AnyAsync(t => string.Equals(t.Name, request.Name, StringComparison.OrdinalIgnoreCase));

        if (nameExists)
        {
            throw new BadRequestException($"Tag with name '{request.Name}' already exists");
        }

        DocumentTag tag = new DocumentTag
        {
            Name = request.Name,
            Color = request.Color,
            CreatedAt = DateTime.UtcNow,
            Documents = []
        };

        _context.DocumentTags.Add(tag);
        await _context.SaveChangesAsync();

        return MapToTagDto(tag);
    }

    /// <inheritdoc/>
    public async Task<TagDto> GetTagByIdAsync(int id)
    {
        Debug.Assert(id > 0, "Tag ID must be positive");

        DocumentTag? tag = await _context.DocumentTags.FindAsync(id);

        if (tag == null)
        {
            throw new NotFoundException($"Tag with ID {id} not found");
        }

        return MapToTagDto(tag);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<TagDto>> GetAllTagsAsync()
    {
        List<DocumentTag> tags = await _context.DocumentTags
            .OrderBy(t => t.Name)
            .ToListAsync();

        return tags.Select(MapToTagDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<TagDto> UpdateTagAsync(int id, UpdateTagRequest request)
    {
        Debug.Assert(id > 0, "Tag ID must be positive");
        Debug.Assert(request != null, "Update tag request cannot be null");

        DocumentTag? tag = await _context.DocumentTags.FindAsync(id);

        if (tag == null)
        {
            throw new NotFoundException($"Tag with ID {id} not found");
        }

        // Update name if provided
        if (!string.IsNullOrWhiteSpace(request.Name) && !string.Equals(tag.Name, request.Name, StringComparison.Ordinal))
        {
            // Check for duplicate tag name (case-insensitive)
            bool nameExists = await _context.DocumentTags
                .AnyAsync(t => t.Id != id && string.Equals(t.Name, request.Name, StringComparison.OrdinalIgnoreCase));

            if (nameExists)
            {
                throw new BadRequestException($"Tag with name '{request.Name}' already exists");
            }

            tag.Name = request.Name;
        }

        // Update color if provided
        if (request.Color != null)
        {
            tag.Color = request.Color;
        }

        await _context.SaveChangesAsync();

        return MapToTagDto(tag);
    }

    /// <inheritdoc/>
    public async Task DeleteTagAsync(int id)
    {
        Debug.Assert(id > 0, "Tag ID must be positive");

        DocumentTag? tag = await _context.DocumentTags.FindAsync(id);

        if (tag == null)
        {
            throw new NotFoundException($"Tag with ID {id} not found");
        }

        _context.DocumentTags.Remove(tag);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Maps a DocumentTag entity to TagDto
    /// </summary>
    private static TagDto MapToTagDto(DocumentTag tag)
    {
        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Color = tag.Color,
            CreatedAt = tag.CreatedAt
        };
    }
}
