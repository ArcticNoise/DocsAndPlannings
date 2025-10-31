using System.Security.Claims;
using DocsAndPlannings.Api.Controllers;
using DocsAndPlannings.Core.Data;
using DocsAndPlannings.Core.DTOs.Documents;
using DocsAndPlannings.Core.Models;
using DocsAndPlannings.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DocsAndPlannings.Core.Tests.Controllers;

public sealed class DocumentsControllerTests : IDisposable
{
    private readonly ApplicationDbContext m_Context;
    private readonly IDocumentService m_DocumentService;
    private readonly DocumentsController m_Controller;
    private readonly TestFileStorageService m_FileStorageService;

    public DocumentsControllerTests()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        m_Context = new ApplicationDbContext(options);
        m_FileStorageService = new TestFileStorageService();
        m_DocumentService = new DocumentService(m_Context, m_FileStorageService);

        ILogger<DocumentsController> logger = LoggerFactory.Create(builder => builder.AddConsole())
            .CreateLogger<DocumentsController>();

        m_Controller = new DocumentsController(m_DocumentService, logger);

        SetupControllerContext(1);
        SeedTestData();
    }

    public void Dispose()
    {
        m_Context.Dispose();
        m_FileStorageService.Dispose();
    }

    private void SetupControllerContext(int userId)
    {
        ClaimsPrincipal user = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        ], "TestAuth"));

        m_Controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    private void SeedTestData()
    {
        User user1 = new User
        {
            Id = 1,
            Email = "user1@example.com",
            PasswordHash = "hash",
            FirstName = "User",
            LastName = "One",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        User user2 = new User
        {
            Id = 2,
            Email = "user2@example.com",
            PasswordHash = "hash",
            FirstName = "User",
            LastName = "Two",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        m_Context.Users.AddRange(user1, user2);
        m_Context.SaveChanges();
    }

    [Fact]
    public async Task CreateDocument_ReturnsCreatedResult_WithDocument()
    {
        CreateDocumentRequest request = new CreateDocumentRequest
        {
            Title = "Test Document",
            Content = "Test content",
            IsPublished = true
        };

        ActionResult<DocumentDto> result = await m_Controller.CreateDocument(request);

        CreatedAtActionResult createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        DocumentDto document = Assert.IsType<DocumentDto>(createdResult.Value);
        Assert.Equal("Test Document", document.Title);
        Assert.Equal("Test content", document.Content);
    }

    [Fact]
    public async Task CreateDocument_ReturnsNotFound_WhenParentDoesNotExist()
    {
        CreateDocumentRequest request = new CreateDocumentRequest
        {
            Title = "Child",
            Content = "Content",
            ParentDocumentId = 999,
            IsPublished = true
        };

        ActionResult<DocumentDto> result = await m_Controller.CreateDocument(request);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetDocumentById_ReturnsOkResult_WithDocument()
    {
        CreateDocumentRequest createRequest = new CreateDocumentRequest
        {
            Title = "Test",
            Content = "Content",
            IsPublished = true
        };
        ActionResult<DocumentDto> createResult = await m_Controller.CreateDocument(createRequest);
        CreatedAtActionResult createdResult = Assert.IsType<CreatedAtActionResult>(createResult.Result);
        DocumentDto created = Assert.IsType<DocumentDto>(createdResult.Value);

        ActionResult<DocumentDto> result = await m_Controller.GetDocumentById(created.Id);

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        DocumentDto document = Assert.IsType<DocumentDto>(okResult.Value);
        Assert.Equal(created.Id, document.Id);
    }

    [Fact]
    public async Task GetDocumentById_ReturnsNotFound_WhenDocumentDoesNotExist()
    {
        ActionResult<DocumentDto> result = await m_Controller.GetDocumentById(999);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetDocumentById_ReturnsForbidden_WhenUnpublishedDocumentAccessedByNonAuthor()
    {
        CreateDocumentRequest request = new CreateDocumentRequest
        {
            Title = "Private",
            Content = "Content",
            IsPublished = false
        };
        ActionResult<DocumentDto> createResult = await m_Controller.CreateDocument(request);
        CreatedAtActionResult createdResult = Assert.IsType<CreatedAtActionResult>(createResult.Result);
        DocumentDto created = Assert.IsType<DocumentDto>(createdResult.Value);

        SetupControllerContext(2);

        ActionResult<DocumentDto> result = await m_Controller.GetDocumentById(created.Id);

        ObjectResult forbiddenResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(403, forbiddenResult.StatusCode);
    }

    [Fact]
    public async Task UpdateDocument_ReturnsOkResult_WithUpdatedDocument()
    {
        CreateDocumentRequest createRequest = new CreateDocumentRequest
        {
            Title = "Original",
            Content = "Original content",
            IsPublished = true
        };
        ActionResult<DocumentDto> createResult = await m_Controller.CreateDocument(createRequest);
        CreatedAtActionResult createdResult = Assert.IsType<CreatedAtActionResult>(createResult.Result);
        DocumentDto created = Assert.IsType<DocumentDto>(createdResult.Value);

        UpdateDocumentRequest updateRequest = new UpdateDocumentRequest
        {
            Title = "Updated",
            Content = "Updated content"
        };

        ActionResult<DocumentDto> result = await m_Controller.UpdateDocument(created.Id, updateRequest);

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        DocumentDto updated = Assert.IsType<DocumentDto>(okResult.Value);
        Assert.Equal("Updated", updated.Title);
    }

    [Fact]
    public async Task UpdateDocument_ReturnsNotFound_WhenDocumentDoesNotExist()
    {
        UpdateDocumentRequest request = new UpdateDocumentRequest
        {
            Title = "Updated"
        };

        ActionResult<DocumentDto> result = await m_Controller.UpdateDocument(999, request);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateDocument_ReturnsForbidden_WhenNonAuthorTriesToUpdate()
    {
        CreateDocumentRequest createRequest = new CreateDocumentRequest
        {
            Title = "Doc",
            Content = "Content",
            IsPublished = true
        };
        ActionResult<DocumentDto> createResult = await m_Controller.CreateDocument(createRequest);
        CreatedAtActionResult createdResult = Assert.IsType<CreatedAtActionResult>(createResult.Result);
        DocumentDto created = Assert.IsType<DocumentDto>(createdResult.Value);

        SetupControllerContext(2);

        UpdateDocumentRequest updateRequest = new UpdateDocumentRequest
        {
            Title = "Hacked"
        };

        ActionResult<DocumentDto> result = await m_Controller.UpdateDocument(created.Id, updateRequest);

        ObjectResult forbiddenResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(403, forbiddenResult.StatusCode);
    }

    [Fact]
    public async Task DeleteDocument_ReturnsNoContent_WhenSuccessful()
    {
        CreateDocumentRequest createRequest = new CreateDocumentRequest
        {
            Title = "To Delete",
            Content = "Content",
            IsPublished = true
        };
        ActionResult<DocumentDto> createResult = await m_Controller.CreateDocument(createRequest);
        CreatedAtActionResult createdResult = Assert.IsType<CreatedAtActionResult>(createResult.Result);
        DocumentDto created = Assert.IsType<DocumentDto>(createdResult.Value);

        ActionResult result = await m_Controller.DeleteDocument(created.Id);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteDocument_ReturnsNotFound_WhenDocumentDoesNotExist()
    {
        ActionResult result = await m_Controller.DeleteDocument(999);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task DeleteDocument_ReturnsForbidden_WhenNonAuthorTriesToDelete()
    {
        CreateDocumentRequest createRequest = new CreateDocumentRequest
        {
            Title = "Doc",
            Content = "Content",
            IsPublished = true
        };
        ActionResult<DocumentDto> createResult = await m_Controller.CreateDocument(createRequest);
        CreatedAtActionResult createdResult = Assert.IsType<CreatedAtActionResult>(createResult.Result);
        DocumentDto created = Assert.IsType<DocumentDto>(createdResult.Value);

        SetupControllerContext(2);

        ActionResult result = await m_Controller.DeleteDocument(created.Id);

        ObjectResult forbiddenResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, forbiddenResult.StatusCode);
    }

    [Fact]
    public async Task GetDocumentVersions_ReturnsOkResult_WithVersions()
    {
        CreateDocumentRequest createRequest = new CreateDocumentRequest
        {
            Title = "Doc",
            Content = "V1",
            IsPublished = true
        };
        ActionResult<DocumentDto> createResult = await m_Controller.CreateDocument(createRequest);
        CreatedAtActionResult createdResult = Assert.IsType<CreatedAtActionResult>(createResult.Result);
        DocumentDto created = Assert.IsType<DocumentDto>(createdResult.Value);

        UpdateDocumentRequest updateRequest = new UpdateDocumentRequest { Content = "V2" };
        await m_Controller.UpdateDocument(created.Id, updateRequest);

        ActionResult<IReadOnlyList<DocumentVersionDto>> result = await m_Controller.GetDocumentVersions(created.Id);

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        IReadOnlyList<DocumentVersionDto> versions = Assert.IsAssignableFrom<IReadOnlyList<DocumentVersionDto>>(okResult.Value);
        Assert.Equal(2, versions.Count);
    }

    [Fact]
    public async Task GetChildDocuments_ReturnsOkResult_WithChildren()
    {
        CreateDocumentRequest parentRequest = new CreateDocumentRequest
        {
            Title = "Parent",
            Content = "Parent content",
            IsPublished = true
        };
        ActionResult<DocumentDto> parentResult = await m_Controller.CreateDocument(parentRequest);
        CreatedAtActionResult parentCreated = Assert.IsType<CreatedAtActionResult>(parentResult.Result);
        DocumentDto parent = Assert.IsType<DocumentDto>(parentCreated.Value);

        CreateDocumentRequest childRequest = new CreateDocumentRequest
        {
            Title = "Child",
            Content = "Child content",
            ParentDocumentId = parent.Id,
            IsPublished = true
        };
        await m_Controller.CreateDocument(childRequest);

        ActionResult<IReadOnlyList<DocumentListItemDto>> result = await m_Controller.GetChildDocuments(parent.Id);

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        IReadOnlyList<DocumentListItemDto> children = Assert.IsAssignableFrom<IReadOnlyList<DocumentListItemDto>>(okResult.Value);
        Assert.Single(children);
    }

    [Fact]
    public async Task SearchDocuments_ReturnsOkResult_WithMatchingDocuments()
    {
        CreateDocumentRequest request1 = new CreateDocumentRequest
        {
            Title = "Database Tutorial",
            Content = "Learn SQL",
            IsPublished = true
        };
        await m_Controller.CreateDocument(request1);

        CreateDocumentRequest request2 = new CreateDocumentRequest
        {
            Title = "Web Development",
            Content = "Learn HTML",
            IsPublished = true
        };
        await m_Controller.CreateDocument(request2);

        DocumentSearchRequest searchRequest = new DocumentSearchRequest
        {
            Query = "Database"
        };

        ActionResult<IReadOnlyList<DocumentListItemDto>> result = await m_Controller.SearchDocuments(searchRequest);

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        IReadOnlyList<DocumentListItemDto> documents = Assert.IsAssignableFrom<IReadOnlyList<DocumentListItemDto>>(okResult.Value);
        Assert.Single(documents);
    }

    [Fact]
    public async Task UploadAttachment_ReturnsCreatedResult_WithAttachment()
    {
        CreateDocumentRequest createRequest = new CreateDocumentRequest
        {
            Title = "Doc",
            Content = "Content",
            IsPublished = true
        };
        ActionResult<DocumentDto> createResult = await m_Controller.CreateDocument(createRequest);
        CreatedAtActionResult createdResult = Assert.IsType<CreatedAtActionResult>(createResult.Result);
        DocumentDto document = Assert.IsType<DocumentDto>(createdResult.Value);

        byte[] fileContent = "test file"u8.ToArray();
        MemoryStream stream = new MemoryStream(fileContent);
        FormFile file = new FormFile(stream, 0, fileContent.Length, "file", "test.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };

        ActionResult<DocumentAttachmentDto> result = await m_Controller.UploadAttachment(document.Id, file);

        CreatedAtActionResult uploadResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        DocumentAttachmentDto attachment = Assert.IsType<DocumentAttachmentDto>(uploadResult.Value);
        Assert.Equal("test.png", attachment.FileName);

        stream.Dispose();
    }

    [Fact]
    public async Task UploadAttachment_ReturnsBadRequest_WhenNoFileProvided()
    {
        CreateDocumentRequest createRequest = new CreateDocumentRequest
        {
            Title = "Doc",
            Content = "Content",
            IsPublished = true
        };
        ActionResult<DocumentDto> createResult = await m_Controller.CreateDocument(createRequest);
        CreatedAtActionResult createdResult = Assert.IsType<CreatedAtActionResult>(createResult.Result);
        DocumentDto document = Assert.IsType<DocumentDto>(createdResult.Value);

        ActionResult<DocumentAttachmentDto> result = await m_Controller.UploadAttachment(document.Id, null!);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetDocumentAttachments_ReturnsOkResult_WithAttachments()
    {
        CreateDocumentRequest createRequest = new CreateDocumentRequest
        {
            Title = "Doc",
            Content = "Content",
            IsPublished = true
        };
        ActionResult<DocumentDto> createResult = await m_Controller.CreateDocument(createRequest);
        CreatedAtActionResult createdResult = Assert.IsType<CreatedAtActionResult>(createResult.Result);
        DocumentDto document = Assert.IsType<DocumentDto>(createdResult.Value);

        byte[] fileContent = "test"u8.ToArray();
        using MemoryStream stream = new MemoryStream(fileContent);
        await m_DocumentService.UploadAttachmentAsync(document.Id, stream, "test.png", "image/png", 1);

        ActionResult<IReadOnlyList<DocumentAttachmentDto>> result = await m_Controller.GetDocumentAttachments(document.Id);

        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        IReadOnlyList<DocumentAttachmentDto> attachments = Assert.IsAssignableFrom<IReadOnlyList<DocumentAttachmentDto>>(okResult.Value);
        Assert.Single(attachments);
    }

    [Fact]
    public async Task GetAttachmentFile_ReturnsFileResult()
    {
        CreateDocumentRequest createRequest = new CreateDocumentRequest
        {
            Title = "Doc",
            Content = "Content",
            IsPublished = true
        };
        ActionResult<DocumentDto> createResult = await m_Controller.CreateDocument(createRequest);
        CreatedAtActionResult createdResult = Assert.IsType<CreatedAtActionResult>(createResult.Result);
        DocumentDto document = Assert.IsType<DocumentDto>(createdResult.Value);

        byte[] fileContent = "test content"u8.ToArray();
        using MemoryStream stream = new MemoryStream(fileContent);
        DocumentAttachmentDto attachment = await m_DocumentService.UploadAttachmentAsync(
            document.Id,
            stream,
            "test.png",
            "image/png",
            1);

        IActionResult result = await m_Controller.GetAttachmentFile(document.Id, attachment.Id);

        FileStreamResult fileResult = Assert.IsType<FileStreamResult>(result);
        Assert.Equal("image/png", fileResult.ContentType);
        Assert.Equal("test.png", fileResult.FileDownloadName);
    }

    [Fact]
    public async Task DeleteAttachment_ReturnsNoContent_WhenSuccessful()
    {
        CreateDocumentRequest createRequest = new CreateDocumentRequest
        {
            Title = "Doc",
            Content = "Content",
            IsPublished = true
        };
        ActionResult<DocumentDto> createResult = await m_Controller.CreateDocument(createRequest);
        CreatedAtActionResult createdResult = Assert.IsType<CreatedAtActionResult>(createResult.Result);
        DocumentDto document = Assert.IsType<DocumentDto>(createdResult.Value);

        byte[] fileContent = "test"u8.ToArray();
        using MemoryStream stream = new MemoryStream(fileContent);
        DocumentAttachmentDto attachment = await m_DocumentService.UploadAttachmentAsync(
            document.Id,
            stream,
            "test.png",
            "image/png",
            1);

        IActionResult result = await m_Controller.DeleteAttachment(document.Id, attachment.Id);

        Assert.IsType<NoContentResult>(result);
    }

    private sealed class TestFileStorageService : IFileStorageService, IDisposable
    {
        private readonly Dictionary<string, byte[]> m_Files = new Dictionary<string, byte[]>();

        public Task<string> SaveFileAsync(Stream fileStream, string fileName, string contentType)
        {
            string storedFileName = $"{Guid.NewGuid()}_{fileName}";
            using MemoryStream ms = new MemoryStream();
            fileStream.CopyTo(ms);
            m_Files[storedFileName] = ms.ToArray();
            return Task.FromResult(storedFileName);
        }

        public Task<(Stream FileStream, string ContentType)> GetFileAsync(string storedFileName)
        {
            if (!m_Files.ContainsKey(storedFileName))
            {
                throw new FileNotFoundException($"File {storedFileName} not found");
            }
            Stream stream = new MemoryStream(m_Files[storedFileName]);
            return Task.FromResult<(Stream, string)>((stream, "image/png"));
        }

        public Task DeleteFileAsync(string storedFileName)
        {
            m_Files.Remove(storedFileName);
            return Task.CompletedTask;
        }

        public Task<bool> FileExistsAsync(string storedFileName)
        {
            return Task.FromResult(m_Files.ContainsKey(storedFileName));
        }

        public string GetFilePath(string storedFileName)
        {
            return storedFileName;
        }

        public void ValidateFile(string fileName, string contentType, long fileSizeBytes)
        {
            // Simplified validation for tests
        }

        public void Dispose()
        {
            m_Files.Clear();
        }
    }
}
