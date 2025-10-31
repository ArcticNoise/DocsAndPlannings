using DocsAndPlannings.Core.Data;
using DocsAndPlannings.Core.DTOs.Tags;
using DocsAndPlannings.Core.Exceptions;
using DocsAndPlannings.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace DocsAndPlannings.Core.Tests.Services;

public sealed class TagServiceTests : IDisposable
{
    private readonly ApplicationDbContext m_Context;
    private readonly ITagService m_TagService;

    public TagServiceTests()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        m_Context = new ApplicationDbContext(options);
        m_TagService = new TagService(m_Context);
    }

    public void Dispose()
    {
        m_Context.Dispose();
    }

    [Fact]
    public async Task CreateTagAsync_CreatesTag_Successfully()
    {
        CreateTagRequest request = new CreateTagRequest
        {
            Name = "Tutorial",
            Color = "#FF0000"
        };

        TagDto result = await m_TagService.CreateTagAsync(request);

        Assert.NotNull(result);
        Assert.Equal("Tutorial", result.Name);
        Assert.Equal("#FF0000", result.Color);
        Assert.True(result.Id > 0);
    }

    [Fact]
    public async Task CreateTagAsync_ThrowsBadRequestException_WhenTagNameAlreadyExists()
    {
        CreateTagRequest request1 = new CreateTagRequest
        {
            Name = "Tutorial",
            Color = "#FF0000"
        };
        await m_TagService.CreateTagAsync(request1);

        CreateTagRequest request2 = new CreateTagRequest
        {
            Name = "Tutorial",
            Color = "#00FF00"
        };

        await Assert.ThrowsAsync<BadRequestException>(
            async () => await m_TagService.CreateTagAsync(request2));
    }

    [Fact]
    public async Task CreateTagAsync_ThrowsBadRequestException_WhenTagNameAlreadyExistsCaseInsensitive()
    {
        CreateTagRequest request1 = new CreateTagRequest
        {
            Name = "Tutorial",
            Color = "#FF0000"
        };
        await m_TagService.CreateTagAsync(request1);

        CreateTagRequest request2 = new CreateTagRequest
        {
            Name = "TUTORIAL",
            Color = "#00FF00"
        };

        await Assert.ThrowsAsync<BadRequestException>(
            async () => await m_TagService.CreateTagAsync(request2));
    }

    [Fact]
    public async Task GetTagByIdAsync_ReturnsTag_Successfully()
    {
        CreateTagRequest request = new CreateTagRequest
        {
            Name = "Tutorial",
            Color = "#FF0000"
        };
        TagDto created = await m_TagService.CreateTagAsync(request);

        TagDto result = await m_TagService.GetTagByIdAsync(created.Id);

        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.Equal("Tutorial", result.Name);
        Assert.Equal("#FF0000", result.Color);
    }

    [Fact]
    public async Task GetTagByIdAsync_ThrowsNotFoundException_WhenTagDoesNotExist()
    {
        await Assert.ThrowsAsync<NotFoundException>(
            async () => await m_TagService.GetTagByIdAsync(999));
    }

    [Fact]
    public async Task GetAllTagsAsync_ReturnsAllTags_Successfully()
    {
        CreateTagRequest request1 = new CreateTagRequest
        {
            Name = "Tutorial",
            Color = "#FF0000"
        };
        await m_TagService.CreateTagAsync(request1);

        CreateTagRequest request2 = new CreateTagRequest
        {
            Name = "Guide",
            Color = "#00FF00"
        };
        await m_TagService.CreateTagAsync(request2);

        IReadOnlyList<TagDto> tags = await m_TagService.GetAllTagsAsync();

        Assert.NotNull(tags);
        Assert.Equal(2, tags.Count);
    }

    [Fact]
    public async Task GetAllTagsAsync_ReturnsEmptyList_WhenNoTags()
    {
        IReadOnlyList<TagDto> tags = await m_TagService.GetAllTagsAsync();

        Assert.NotNull(tags);
        Assert.Empty(tags);
    }

    [Fact]
    public async Task UpdateTagAsync_UpdatesTag_Successfully()
    {
        CreateTagRequest createRequest = new CreateTagRequest
        {
            Name = "Tutorial",
            Color = "#FF0000"
        };
        TagDto created = await m_TagService.CreateTagAsync(createRequest);

        UpdateTagRequest updateRequest = new UpdateTagRequest
        {
            Name = "Updated Tutorial",
            Color = "#0000FF"
        };

        TagDto updated = await m_TagService.UpdateTagAsync(created.Id, updateRequest);

        Assert.NotNull(updated);
        Assert.Equal("Updated Tutorial", updated.Name);
        Assert.Equal("#0000FF", updated.Color);
    }

    [Fact]
    public async Task UpdateTagAsync_ThrowsNotFoundException_WhenTagDoesNotExist()
    {
        UpdateTagRequest request = new UpdateTagRequest
        {
            Name = "Updated",
            Color = "#FF0000"
        };

        await Assert.ThrowsAsync<NotFoundException>(
            async () => await m_TagService.UpdateTagAsync(999, request));
    }

    [Fact]
    public async Task UpdateTagAsync_ThrowsBadRequestException_WhenUpdatingToDuplicateName()
    {
        CreateTagRequest request1 = new CreateTagRequest
        {
            Name = "Tutorial",
            Color = "#FF0000"
        };
        await m_TagService.CreateTagAsync(request1);

        CreateTagRequest request2 = new CreateTagRequest
        {
            Name = "Guide",
            Color = "#00FF00"
        };
        TagDto tag2 = await m_TagService.CreateTagAsync(request2);

        UpdateTagRequest updateRequest = new UpdateTagRequest
        {
            Name = "Tutorial"
        };

        await Assert.ThrowsAsync<BadRequestException>(
            async () => await m_TagService.UpdateTagAsync(tag2.Id, updateRequest));
    }

    [Fact]
    public async Task UpdateTagAsync_AllowsSameNameForSameTag()
    {
        CreateTagRequest createRequest = new CreateTagRequest
        {
            Name = "Tutorial",
            Color = "#FF0000"
        };
        TagDto created = await m_TagService.CreateTagAsync(createRequest);

        UpdateTagRequest updateRequest = new UpdateTagRequest
        {
            Name = "Tutorial",
            Color = "#0000FF"
        };

        TagDto updated = await m_TagService.UpdateTagAsync(created.Id, updateRequest);

        Assert.NotNull(updated);
        Assert.Equal("Tutorial", updated.Name);
        Assert.Equal("#0000FF", updated.Color);
    }

    [Fact]
    public async Task DeleteTagAsync_DeletesTag_Successfully()
    {
        CreateTagRequest request = new CreateTagRequest
        {
            Name = "To Delete",
            Color = "#FF0000"
        };
        TagDto created = await m_TagService.CreateTagAsync(request);

        await m_TagService.DeleteTagAsync(created.Id);

        IReadOnlyList<TagDto> tags = await m_TagService.GetAllTagsAsync();
        Assert.Empty(tags);
    }

    [Fact]
    public async Task DeleteTagAsync_ThrowsNotFoundException_WhenTagDoesNotExist()
    {
        await Assert.ThrowsAsync<NotFoundException>(
            async () => await m_TagService.DeleteTagAsync(999));
    }
}
