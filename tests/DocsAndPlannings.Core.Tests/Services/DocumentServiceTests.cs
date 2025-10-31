using DocsAndPlannings.Core.Data;
using DocsAndPlannings.Core.DTOs.Documents;
using DocsAndPlannings.Core.Exceptions;
using DocsAndPlannings.Core.Models;
using DocsAndPlannings.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace DocsAndPlannings.Core.Tests.Services;

public sealed class DocumentServiceTests : IDisposable
{
    private readonly ApplicationDbContext m_Context;
    private readonly IDocumentService m_DocumentService;
    private readonly TestFileStorageService m_FileStorageService;

    public DocumentServiceTests()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        m_Context = new ApplicationDbContext(options);
        m_FileStorageService = new TestFileStorageService();
        m_DocumentService = new DocumentService(m_Context, m_FileStorageService);

        SeedTestData();
    }

    public void Dispose()
    {
        m_Context.Dispose();
        m_FileStorageService.Dispose();
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

        DocumentTag tag1 = new DocumentTag
        {
            Id = 1,
            Name = "Tutorial",
            Color = "#FF0000",
            CreatedAt = DateTime.UtcNow
        };

        DocumentTag tag2 = new DocumentTag
        {
            Id = 2,
            Name = "Guide",
            Color = "#00FF00",
            CreatedAt = DateTime.UtcNow
        };

        m_Context.Users.AddRange(user1, user2);
        m_Context.DocumentTags.AddRange(tag1, tag2);
        m_Context.SaveChanges();
    }

    [Fact]
    public async Task CreateDocumentAsync_CreatesDocument_Successfully()
    {
        CreateDocumentRequest request = new CreateDocumentRequest
        {
            Title = "Test Document",
            Content = "Test content",
            IsPublished = true
        };

        DocumentDto result = await m_DocumentService.CreateDocumentAsync(request, 1);

        Assert.NotNull(result);
        Assert.Equal("Test Document", result.Title);
        Assert.Equal("Test content", result.Content);
        Assert.True(result.IsPublished);
        Assert.Equal(1, result.AuthorId);
    }

    [Fact]
    public async Task CreateDocumentAsync_CreatesDocument_WithParent()
    {
        CreateDocumentRequest parentRequest = new CreateDocumentRequest
        {
            Title = "Parent",
            Content = "Parent content",
            IsPublished = true
        };
        DocumentDto parent = await m_DocumentService.CreateDocumentAsync(parentRequest, 1);

        CreateDocumentRequest childRequest = new CreateDocumentRequest
        {
            Title = "Child",
            Content = "Child content",
            ParentDocumentId = parent.Id,
            IsPublished = true
        };

        DocumentDto child = await m_DocumentService.CreateDocumentAsync(childRequest, 1);

        Assert.NotNull(child);
        Assert.Equal(parent.Id, child.ParentDocumentId);
    }

    [Fact]
    public async Task CreateDocumentAsync_CreatesDocument_WithTags()
    {
        CreateDocumentRequest request = new CreateDocumentRequest
        {
            Title = "Tagged Document",
            Content = "Content",
            TagIds = [1, 2],
            IsPublished = true
        };

        DocumentDto result = await m_DocumentService.CreateDocumentAsync(request, 1);

        Assert.NotNull(result);
        Assert.NotNull(result.Tags);
        Assert.Equal(2, result.Tags.Count);
    }

    [Fact]
    public async Task CreateDocumentAsync_ThrowsNotFoundException_WhenParentDoesNotExist()
    {
        CreateDocumentRequest request = new CreateDocumentRequest
        {
            Title = "Child",
            Content = "Content",
            ParentDocumentId = 999,
            IsPublished = true
        };

        await Assert.ThrowsAsync<NotFoundException>(
            async () => await m_DocumentService.CreateDocumentAsync(request, 1));
    }

    [Fact]
    public async Task CreateDocumentAsync_ThrowsNotFoundException_WhenTagDoesNotExist()
    {
        CreateDocumentRequest request = new CreateDocumentRequest
        {
            Title = "Document",
            Content = "Content",
            TagIds = [999],
            IsPublished = true
        };

        await Assert.ThrowsAsync<NotFoundException>(
            async () => await m_DocumentService.CreateDocumentAsync(request, 1));
    }

    [Fact]
    public async Task CreateDocumentAsync_ThrowsBadRequestException_WhenCreatingCircularHierarchy()
    {
        CreateDocumentRequest request1 = new CreateDocumentRequest
        {
            Title = "Doc A",
            Content = "Content A",
            IsPublished = true
        };
        DocumentDto docA = await m_DocumentService.CreateDocumentAsync(request1, 1);

        CreateDocumentRequest request2 = new CreateDocumentRequest
        {
            Title = "Doc B",
            Content = "Content B",
            ParentDocumentId = docA.Id,
            IsPublished = true
        };
        DocumentDto docB = await m_DocumentService.CreateDocumentAsync(request2, 1);

        UpdateDocumentRequest updateRequest = new UpdateDocumentRequest
        {
            ParentDocumentId = docB.Id
        };

        await Assert.ThrowsAsync<BadRequestException>(
            async () => await m_DocumentService.UpdateDocumentAsync(docA.Id, updateRequest, 1));
    }

    [Fact]
    public async Task GetDocumentByIdAsync_ReturnsDocument_Successfully()
    {
        CreateDocumentRequest request = new CreateDocumentRequest
        {
            Title = "Test",
            Content = "Content",
            IsPublished = true
        };
        DocumentDto created = await m_DocumentService.CreateDocumentAsync(request, 1);

        DocumentDto result = await m_DocumentService.GetDocumentByIdAsync(created.Id, 1);

        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.Equal("Test", result.Title);
    }

    [Fact]
    public async Task GetDocumentByIdAsync_ReturnsPublishedDocument_ForAnyUser()
    {
        CreateDocumentRequest request = new CreateDocumentRequest
        {
            Title = "Public",
            Content = "Content",
            IsPublished = true
        };
        DocumentDto created = await m_DocumentService.CreateDocumentAsync(request, 1);

        DocumentDto result = await m_DocumentService.GetDocumentByIdAsync(created.Id, 2);

        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
    }

    [Fact]
    public async Task GetDocumentByIdAsync_ThrowsNotFoundException_WhenDocumentDoesNotExist()
    {
        await Assert.ThrowsAsync<NotFoundException>(
            async () => await m_DocumentService.GetDocumentByIdAsync(999, 1));
    }

    [Fact]
    public async Task GetDocumentByIdAsync_ThrowsForbiddenException_WhenUnpublishedDocumentAccessedByNonAuthor()
    {
        CreateDocumentRequest request = new CreateDocumentRequest
        {
            Title = "Private",
            Content = "Content",
            IsPublished = false
        };
        DocumentDto created = await m_DocumentService.CreateDocumentAsync(request, 1);

        await Assert.ThrowsAsync<ForbiddenException>(
            async () => await m_DocumentService.GetDocumentByIdAsync(created.Id, 2));
    }

    [Fact]
    public async Task UpdateDocumentAsync_UpdatesDocument_Successfully()
    {
        CreateDocumentRequest createRequest = new CreateDocumentRequest
        {
            Title = "Original",
            Content = "Original content",
            IsPublished = true
        };
        DocumentDto created = await m_DocumentService.CreateDocumentAsync(createRequest, 1);

        UpdateDocumentRequest updateRequest = new UpdateDocumentRequest
        {
            Title = "Updated",
            Content = "Updated content"
        };

        DocumentDto updated = await m_DocumentService.UpdateDocumentAsync(created.Id, updateRequest, 1);

        Assert.NotNull(updated);
        Assert.Equal("Updated", updated.Title);
        Assert.Equal("Updated content", updated.Content);
    }

    [Fact]
    public async Task UpdateDocumentAsync_CreatesNewVersion_WhenContentChanges()
    {
        CreateDocumentRequest createRequest = new CreateDocumentRequest
        {
            Title = "Doc",
            Content = "Version 1",
            IsPublished = true
        };
        DocumentDto created = await m_DocumentService.CreateDocumentAsync(createRequest, 1);

        UpdateDocumentRequest updateRequest = new UpdateDocumentRequest
        {
            Content = "Version 2"
        };
        await m_DocumentService.UpdateDocumentAsync(created.Id, updateRequest, 1);

        IReadOnlyList<DocumentVersionDto> versions = await m_DocumentService.GetDocumentVersionsAsync(created.Id, 1);

        Assert.NotNull(versions);
        Assert.Equal(2, versions.Count);
    }

    [Fact]
    public async Task UpdateDocumentAsync_ThrowsNotFoundException_WhenDocumentDoesNotExist()
    {
        UpdateDocumentRequest request = new UpdateDocumentRequest
        {
            Title = "Updated"
        };

        await Assert.ThrowsAsync<NotFoundException>(
            async () => await m_DocumentService.UpdateDocumentAsync(999, request, 1));
    }

    [Fact]
    public async Task UpdateDocumentAsync_ThrowsForbiddenException_WhenNonAuthorTriesToUpdate()
    {
        CreateDocumentRequest createRequest = new CreateDocumentRequest
        {
            Title = "Doc",
            Content = "Content",
            IsPublished = true
        };
        DocumentDto created = await m_DocumentService.CreateDocumentAsync(createRequest, 1);

        UpdateDocumentRequest updateRequest = new UpdateDocumentRequest
        {
            Title = "Hacked"
        };

        await Assert.ThrowsAsync<ForbiddenException>(
            async () => await m_DocumentService.UpdateDocumentAsync(created.Id, updateRequest, 2));
    }

    [Fact]
    public async Task DeleteDocumentAsync_SoftDeletesDocument_Successfully()
    {
        CreateDocumentRequest request = new CreateDocumentRequest
        {
            Title = "To Delete",
            Content = "Content",
            IsPublished = true
        };
        DocumentDto created = await m_DocumentService.CreateDocumentAsync(request, 1);

        await m_DocumentService.DeleteDocumentAsync(created.Id, 1);

        await Assert.ThrowsAsync<NotFoundException>(
            async () => await m_DocumentService.GetDocumentByIdAsync(created.Id, 1));
    }

    [Fact]
    public async Task DeleteDocumentAsync_ThrowsNotFoundException_WhenDocumentDoesNotExist()
    {
        await Assert.ThrowsAsync<NotFoundException>(
            async () => await m_DocumentService.DeleteDocumentAsync(999, 1));
    }

    [Fact]
    public async Task DeleteDocumentAsync_ThrowsForbiddenException_WhenNonAuthorTriesToDelete()
    {
        CreateDocumentRequest request = new CreateDocumentRequest
        {
            Title = "Doc",
            Content = "Content",
            IsPublished = true
        };
        DocumentDto created = await m_DocumentService.CreateDocumentAsync(request, 1);

        await Assert.ThrowsAsync<ForbiddenException>(
            async () => await m_DocumentService.DeleteDocumentAsync(created.Id, 2));
    }

    [Fact]
    public async Task GetDocumentVersionsAsync_ReturnsAllVersions_Successfully()
    {
        CreateDocumentRequest createRequest = new CreateDocumentRequest
        {
            Title = "Doc",
            Content = "V1",
            IsPublished = true
        };
        DocumentDto created = await m_DocumentService.CreateDocumentAsync(createRequest, 1);

        UpdateDocumentRequest update1 = new UpdateDocumentRequest { Content = "V2" };
        await m_DocumentService.UpdateDocumentAsync(created.Id, update1, 1);

        UpdateDocumentRequest update2 = new UpdateDocumentRequest { Content = "V3" };
        await m_DocumentService.UpdateDocumentAsync(created.Id, update2, 1);

        IReadOnlyList<DocumentVersionDto> versions = await m_DocumentService.GetDocumentVersionsAsync(created.Id, 1);

        Assert.NotNull(versions);
        Assert.Equal(3, versions.Count);
    }

    [Fact]
    public async Task GetChildDocumentsAsync_ReturnsChildDocuments_Successfully()
    {
        CreateDocumentRequest parentRequest = new CreateDocumentRequest
        {
            Title = "Parent",
            Content = "Parent content",
            IsPublished = true
        };
        DocumentDto parent = await m_DocumentService.CreateDocumentAsync(parentRequest, 1);

        CreateDocumentRequest child1Request = new CreateDocumentRequest
        {
            Title = "Child 1",
            Content = "Content 1",
            ParentDocumentId = parent.Id,
            IsPublished = true
        };
        await m_DocumentService.CreateDocumentAsync(child1Request, 1);

        CreateDocumentRequest child2Request = new CreateDocumentRequest
        {
            Title = "Child 2",
            Content = "Content 2",
            ParentDocumentId = parent.Id,
            IsPublished = true
        };
        await m_DocumentService.CreateDocumentAsync(child2Request, 1);

        IReadOnlyList<DocumentListItemDto> children = await m_DocumentService.GetChildDocumentsAsync(parent.Id, 1);

        Assert.NotNull(children);
        Assert.Equal(2, children.Count);
    }

    [Fact]
    public async Task SearchDocumentsAsync_SearchesByQuery_Successfully()
    {
        CreateDocumentRequest request1 = new CreateDocumentRequest
        {
            Title = "Database Tutorial",
            Content = "Learn SQL",
            IsPublished = true
        };
        await m_DocumentService.CreateDocumentAsync(request1, 1);

        CreateDocumentRequest request2 = new CreateDocumentRequest
        {
            Title = "Web Development",
            Content = "Learn HTML",
            IsPublished = true
        };
        await m_DocumentService.CreateDocumentAsync(request2, 1);

        DocumentSearchRequest searchRequest = new DocumentSearchRequest
        {
            Query = "Database"
        };

        IReadOnlyList<DocumentListItemDto> results = await m_DocumentService.SearchDocumentsAsync(searchRequest, 1);

        Assert.NotNull(results);
        Assert.Single(results);
        Assert.Contains("Database", results[0].Title);
    }

    [Fact]
    public async Task SearchDocumentsAsync_FiltersByTags_Successfully()
    {
        CreateDocumentRequest request1 = new CreateDocumentRequest
        {
            Title = "Doc 1",
            Content = "Content",
            TagIds = [1],
            IsPublished = true
        };
        await m_DocumentService.CreateDocumentAsync(request1, 1);

        CreateDocumentRequest request2 = new CreateDocumentRequest
        {
            Title = "Doc 2",
            Content = "Content",
            TagIds = [2],
            IsPublished = true
        };
        await m_DocumentService.CreateDocumentAsync(request2, 1);

        DocumentSearchRequest searchRequest = new DocumentSearchRequest
        {
            TagIds = [1]
        };

        IReadOnlyList<DocumentListItemDto> results = await m_DocumentService.SearchDocumentsAsync(searchRequest, 1);

        Assert.NotNull(results);
        Assert.Single(results);
    }

    [Fact]
    public async Task SearchDocumentsAsync_FiltersByAuthor_Successfully()
    {
        CreateDocumentRequest request1 = new CreateDocumentRequest
        {
            Title = "User 1 Doc",
            Content = "Content",
            IsPublished = true
        };
        await m_DocumentService.CreateDocumentAsync(request1, 1);

        CreateDocumentRequest request2 = new CreateDocumentRequest
        {
            Title = "User 2 Doc",
            Content = "Content",
            IsPublished = true
        };
        await m_DocumentService.CreateDocumentAsync(request2, 2);

        DocumentSearchRequest searchRequest = new DocumentSearchRequest
        {
            AuthorId = 1
        };

        IReadOnlyList<DocumentListItemDto> results = await m_DocumentService.SearchDocumentsAsync(searchRequest, 1);

        Assert.NotNull(results);
        Assert.Single(results);
        Assert.Equal("User 1 Doc", results[0].Title);
    }

    [Fact]
    public async Task UploadAttachmentAsync_UploadsFile_Successfully()
    {
        CreateDocumentRequest request = new CreateDocumentRequest
        {
            Title = "Doc with attachment",
            Content = "Content",
            IsPublished = true
        };
        DocumentDto document = await m_DocumentService.CreateDocumentAsync(request, 1);

        byte[] fileContent = "test file content"u8.ToArray();
        using MemoryStream stream = new MemoryStream(fileContent);

        DocumentAttachmentDto attachment = await m_DocumentService.UploadAttachmentAsync(
            document.Id,
            stream,
            "test.png",
            "image/png",
            1);

        Assert.NotNull(attachment);
        Assert.Equal("test.png", attachment.FileName);
        Assert.Equal("image/png", attachment.ContentType);
    }

    [Fact]
    public async Task UploadAttachmentAsync_ThrowsForbiddenException_WhenNonAuthorTriesToUpload()
    {
        CreateDocumentRequest request = new CreateDocumentRequest
        {
            Title = "Doc",
            Content = "Content",
            IsPublished = true
        };
        DocumentDto document = await m_DocumentService.CreateDocumentAsync(request, 1);

        byte[] fileContent = "test"u8.ToArray();
        using MemoryStream stream = new MemoryStream(fileContent);

        await Assert.ThrowsAsync<ForbiddenException>(
            async () => await m_DocumentService.UploadAttachmentAsync(
                document.Id,
                stream,
                "test.png",
                "image/png",
                2));
    }

    [Fact]
    public async Task GetDocumentAttachmentsAsync_ReturnsAttachments_Successfully()
    {
        CreateDocumentRequest request = new CreateDocumentRequest
        {
            Title = "Doc",
            Content = "Content",
            IsPublished = true
        };
        DocumentDto document = await m_DocumentService.CreateDocumentAsync(request, 1);

        byte[] fileContent = "test"u8.ToArray();
        using MemoryStream stream1 = new MemoryStream(fileContent);
        await m_DocumentService.UploadAttachmentAsync(document.Id, stream1, "file1.png", "image/png", 1);

        using MemoryStream stream2 = new MemoryStream(fileContent);
        await m_DocumentService.UploadAttachmentAsync(document.Id, stream2, "file2.png", "image/png", 1);

        IReadOnlyList<DocumentAttachmentDto> attachments = await m_DocumentService.GetDocumentAttachmentsAsync(document.Id, 1);

        Assert.NotNull(attachments);
        Assert.Equal(2, attachments.Count);
    }

    [Fact]
    public async Task GetAttachmentFileAsync_ReturnsFile_Successfully()
    {
        CreateDocumentRequest request = new CreateDocumentRequest
        {
            Title = "Doc",
            Content = "Content",
            IsPublished = true
        };
        DocumentDto document = await m_DocumentService.CreateDocumentAsync(request, 1);

        byte[] fileContent = "test content"u8.ToArray();
        using MemoryStream uploadStream = new MemoryStream(fileContent);
        DocumentAttachmentDto attachment = await m_DocumentService.UploadAttachmentAsync(
            document.Id,
            uploadStream,
            "test.png",
            "image/png",
            1);

        (Stream fileStream, string contentType, string fileName) = await m_DocumentService.GetAttachmentFileAsync(
            document.Id,
            attachment.Id,
            1);

        Assert.NotNull(fileStream);
        Assert.Equal("image/png", contentType);
        Assert.Equal("test.png", fileName);

        using MemoryStream ms = new MemoryStream();
        await fileStream.CopyToAsync(ms);
        byte[] retrievedContent = ms.ToArray();
        Assert.Equal(fileContent, retrievedContent);

        fileStream.Dispose();
    }

    [Fact]
    public async Task DeleteAttachmentAsync_DeletesAttachment_Successfully()
    {
        CreateDocumentRequest request = new CreateDocumentRequest
        {
            Title = "Doc",
            Content = "Content",
            IsPublished = true
        };
        DocumentDto document = await m_DocumentService.CreateDocumentAsync(request, 1);

        byte[] fileContent = "test"u8.ToArray();
        using MemoryStream stream = new MemoryStream(fileContent);
        DocumentAttachmentDto attachment = await m_DocumentService.UploadAttachmentAsync(
            document.Id,
            stream,
            "test.png",
            "image/png",
            1);

        await m_DocumentService.DeleteAttachmentAsync(document.Id, attachment.Id, 1);

        IReadOnlyList<DocumentAttachmentDto> attachments = await m_DocumentService.GetDocumentAttachmentsAsync(document.Id, 1);
        Assert.Empty(attachments);
    }

    [Fact]
    public async Task DeleteAttachmentAsync_ThrowsForbiddenException_WhenNonAuthorTriesToDelete()
    {
        CreateDocumentRequest request = new CreateDocumentRequest
        {
            Title = "Doc",
            Content = "Content",
            IsPublished = true
        };
        DocumentDto document = await m_DocumentService.CreateDocumentAsync(request, 1);

        byte[] fileContent = "test"u8.ToArray();
        using MemoryStream stream = new MemoryStream(fileContent);
        DocumentAttachmentDto attachment = await m_DocumentService.UploadAttachmentAsync(
            document.Id,
            stream,
            "test.png",
            "image/png",
            1);

        await Assert.ThrowsAsync<ForbiddenException>(
            async () => await m_DocumentService.DeleteAttachmentAsync(document.Id, attachment.Id, 2));
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
            if (fileSizeBytes > 10 * 1024 * 1024)
            {
                throw new BadRequestException("File size exceeds maximum allowed size");
            }

            string[] allowedTypes = ["image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp", "image/bmp"];
            if (!allowedTypes.Contains(contentType.ToLowerInvariant()))
            {
                throw new BadRequestException($"File type {contentType} is not allowed");
            }
        }

        public void Dispose()
        {
            m_Files.Clear();
        }
    }
}
