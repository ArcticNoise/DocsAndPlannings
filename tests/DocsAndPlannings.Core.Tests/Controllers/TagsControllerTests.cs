using System.Security.Claims;
using DocsAndPlannings.Api.Controllers;
using DocsAndPlannings.Core.Data;
using DocsAndPlannings.Core.DTOs.Tags;
using DocsAndPlannings.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DocsAndPlannings.Core.Tests.Controllers;

public sealed class TagsControllerTests : IDisposable
{
    private readonly ApplicationDbContext m_Context;
    private readonly ITagService m_TagService;
    private readonly TagsController m_Controller;

    public TagsControllerTests()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        m_Context = new ApplicationDbContext(options);
        m_TagService = new TagService(m_Context);

        ILogger<TagsController> logger = LoggerFactory.Create(builder => builder.AddConsole())
            .CreateLogger<TagsController>();

        m_Controller = new TagsController(m_TagService, logger);

        SetupControllerContext();
    }

    public void Dispose()
    {
        m_Context.Dispose();
    }

    private void SetupControllerContext()
    {
        ClaimsPrincipal user = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, "Admin")
        ], "TestAuth"));

        m_Controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task CreateTag_ReturnsCreatedResult_WithTag()
    {
        CreateTagRequest request = new CreateTagRequest
        {
            Name = "Tutorial",
            Color = "#FF0000"
        };

        ActionResult<TagDto> result = await m_Controller.CreateTag(request);

        CreatedAtActionResult createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        TagDto tag = Assert.IsType<TagDto>(createdResult.Value);
        Assert.Equal("Tutorial", tag.Name);
        Assert.Equal("#FF0000", tag.Color);
    }

    [Fact]
    public async Task CreateTag_ReturnsBadRequest_WhenTagNameAlreadyExists()
    {
        CreateTagRequest request1 = new CreateTagRequest
        {
            Name = "Tutorial",
            Color = "#FF0000"
        };
        await m_Controller.CreateTag(request1);

        CreateTagRequest request2 = new CreateTagRequest
        {
            Name = "Tutorial",
            Color = "#00FF00"
        };

        ActionResult<TagDto> result = await m_Controller.CreateTag(request2);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetTagById_ReturnsOkResult_WithTag()
    {
        CreateTagRequest createRequest = new CreateTagRequest
        {
            Name = "Guide",
            Color = "#00FF00"
        };
        ActionResult<TagDto> createResult = await m_Controller.CreateTag(createRequest);
        CreatedAtActionResult createdResult = Assert.IsType<CreatedAtActionResult>(createResult.Result);
        TagDto created = Assert.IsType<TagDto>(createdResult.Value);

        ActionResult<TagDto> result = await m_Controller.GetTagById(created.Id);

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        TagDto tag = Assert.IsType<TagDto>(okResult.Value);
        Assert.Equal(created.Id, tag.Id);
        Assert.Equal("Guide", tag.Name);
    }

    [Fact]
    public async Task GetTagById_ReturnsNotFound_WhenTagDoesNotExist()
    {
        ActionResult<TagDto> result = await m_Controller.GetTagById(999);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetAllTags_ReturnsOkResult_WithAllTags()
    {
        CreateTagRequest request1 = new CreateTagRequest
        {
            Name = "Tutorial",
            Color = "#FF0000"
        };
        await m_Controller.CreateTag(request1);

        CreateTagRequest request2 = new CreateTagRequest
        {
            Name = "Guide",
            Color = "#00FF00"
        };
        await m_Controller.CreateTag(request2);

        ActionResult<IReadOnlyList<TagDto>> result = await m_Controller.GetAllTags();

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        IReadOnlyList<TagDto> tags = Assert.IsAssignableFrom<IReadOnlyList<TagDto>>(okResult.Value);
        Assert.Equal(2, tags.Count);
    }

    [Fact]
    public async Task GetAllTags_ReturnsEmptyList_WhenNoTags()
    {
        ActionResult<IReadOnlyList<TagDto>> result = await m_Controller.GetAllTags();

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        IReadOnlyList<TagDto> tags = Assert.IsAssignableFrom<IReadOnlyList<TagDto>>(okResult.Value);
        Assert.Empty(tags);
    }

    [Fact]
    public async Task UpdateTag_ReturnsOkResult_WithUpdatedTag()
    {
        CreateTagRequest createRequest = new CreateTagRequest
        {
            Name = "Tutorial",
            Color = "#FF0000"
        };
        ActionResult<TagDto> createResult = await m_Controller.CreateTag(createRequest);
        CreatedAtActionResult createdResult = Assert.IsType<CreatedAtActionResult>(createResult.Result);
        TagDto created = Assert.IsType<TagDto>(createdResult.Value);

        UpdateTagRequest updateRequest = new UpdateTagRequest
        {
            Name = "Updated Tutorial",
            Color = "#0000FF"
        };

        ActionResult<TagDto> result = await m_Controller.UpdateTag(created.Id, updateRequest);

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        TagDto updated = Assert.IsType<TagDto>(okResult.Value);
        Assert.Equal("Updated Tutorial", updated.Name);
        Assert.Equal("#0000FF", updated.Color);
    }

    [Fact]
    public async Task UpdateTag_ReturnsNotFound_WhenTagDoesNotExist()
    {
        UpdateTagRequest request = new UpdateTagRequest
        {
            Name = "Updated",
            Color = "#FF0000"
        };

        ActionResult<TagDto> result = await m_Controller.UpdateTag(999, request);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateTag_ReturnsBadRequest_WhenUpdatingToDuplicateName()
    {
        CreateTagRequest request1 = new CreateTagRequest
        {
            Name = "Tutorial",
            Color = "#FF0000"
        };
        await m_Controller.CreateTag(request1);

        CreateTagRequest request2 = new CreateTagRequest
        {
            Name = "Guide",
            Color = "#00FF00"
        };
        ActionResult<TagDto> createResult = await m_Controller.CreateTag(request2);
        CreatedAtActionResult createdResult = Assert.IsType<CreatedAtActionResult>(createResult.Result);
        TagDto tag2 = Assert.IsType<TagDto>(createdResult.Value);

        UpdateTagRequest updateRequest = new UpdateTagRequest
        {
            Name = "Tutorial"
        };

        ActionResult<TagDto> result = await m_Controller.UpdateTag(tag2.Id, updateRequest);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task DeleteTag_ReturnsNoContent_WhenSuccessful()
    {
        CreateTagRequest createRequest = new CreateTagRequest
        {
            Name = "To Delete",
            Color = "#FF0000"
        };
        ActionResult<TagDto> createResult = await m_Controller.CreateTag(createRequest);
        CreatedAtActionResult createdResult = Assert.IsType<CreatedAtActionResult>(createResult.Result);
        TagDto created = Assert.IsType<TagDto>(createdResult.Value);

        ActionResult result = await m_Controller.DeleteTag(created.Id);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteTag_ReturnsNotFound_WhenTagDoesNotExist()
    {
        ActionResult result = await m_Controller.DeleteTag(999);

        Assert.IsType<NotFoundObjectResult>(result);
    }
}
