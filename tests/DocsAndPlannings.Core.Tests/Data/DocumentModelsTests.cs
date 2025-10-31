using DocsAndPlannings.Core.Data;
using DocsAndPlannings.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DocsAndPlannings.Core.Tests.Data;

public class DocumentModelsTests : IDisposable
{
    private readonly ApplicationDbContext _context;

    public DocumentModelsTests()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new ApplicationDbContext(options);
    }

    [Fact]
    public void CanCreateDocument()
    {
        User author = new User
        {
            Email = "author@example.com",
            PasswordHash = "hash123",
            FirstName = "Jane",
            LastName = "Author",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(author);
        _context.SaveChanges();

        Document doc = new Document
        {
            Title = "Test Document",
            Content = "This is test content",
            AuthorId = author.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Documents.Add(doc);
        int result = _context.SaveChanges();

        Assert.Equal(1, result);
        Assert.True(doc.Id > 0);
        Assert.Equal(1, doc.CurrentVersion);
        Assert.False(doc.IsPublished);
        Assert.False(doc.IsDeleted);
    }

    [Fact]
    public void CanCreateDocumentVersion()
    {
        User author = new User
        {
            Email = "author@example.com",
            PasswordHash = "hash123",
            FirstName = "Jane",
            LastName = "Author",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(author);
        _context.SaveChanges();

        Document doc = new Document
        {
            Title = "Test Document",
            Content = "Version 1",
            AuthorId = author.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Documents.Add(doc);
        _context.SaveChanges();

        DocumentVersion version = new DocumentVersion
        {
            DocumentId = doc.Id,
            VersionNumber = 1,
            Title = "Test Document",
            Content = "Version 1",
            ModifiedById = author.Id,
            ChangeDescription = "Initial version",
            CreatedAt = DateTime.UtcNow
        };

        _context.DocumentVersions.Add(version);
        int result = _context.SaveChanges();

        Assert.Equal(1, result);
        Assert.True(version.Id > 0);
    }

    [Fact]
    public void CanCreateDocumentTag()
    {
        DocumentTag tag = new DocumentTag
        {
            Name = "Tutorial",
            Color = "#FF5733",
            CreatedAt = DateTime.UtcNow
        };

        _context.DocumentTags.Add(tag);
        int result = _context.SaveChanges();

        Assert.Equal(1, result);
        Assert.True(tag.Id > 0);
        Assert.True(tag.IsActive);
    }

    [Fact]
    public void CanAssignTagToDocument()
    {
        User author = new User
        {
            Email = "author@example.com",
            PasswordHash = "hash123",
            FirstName = "Jane",
            LastName = "Author",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(author);
        _context.SaveChanges();

        Document doc = new Document
        {
            Title = "Test Document",
            Content = "Content",
            AuthorId = author.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        DocumentTag tag = new DocumentTag
        {
            Name = "Tutorial",
            CreatedAt = DateTime.UtcNow
        };

        _context.Documents.Add(doc);
        _context.DocumentTags.Add(tag);
        _context.SaveChanges();

        DocumentTagMap tagMap = new DocumentTagMap
        {
            DocumentId = doc.Id,
            TagId = tag.Id,
            AssignedAt = DateTime.UtcNow
        };

        _context.DocumentTagMaps.Add(tagMap);
        int result = _context.SaveChanges();

        Assert.Equal(1, result);
    }

    [Fact]
    public void CanCreateParentChildDocuments()
    {
        User author = new User
        {
            Email = "author@example.com",
            PasswordHash = "hash123",
            FirstName = "Jane",
            LastName = "Author",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(author);
        _context.SaveChanges();

        Document parent = new Document
        {
            Title = "Parent Document",
            Content = "Parent content",
            AuthorId = author.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Documents.Add(parent);
        _context.SaveChanges();

        Document child = new Document
        {
            Title = "Child Document",
            Content = "Child content",
            AuthorId = author.Id,
            ParentDocumentId = parent.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Documents.Add(child);
        int result = _context.SaveChanges();

        Assert.Equal(1, result);
        Assert.Equal(parent.Id, child.ParentDocumentId);
    }

    [Fact]
    public void DeletingDocumentCascadesDeleteVersions()
    {
        User author = new User
        {
            Email = "author@example.com",
            PasswordHash = "hash123",
            FirstName = "Jane",
            LastName = "Author",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(author);
        _context.SaveChanges();

        Document doc = new Document
        {
            Title = "Test Document",
            Content = "Content",
            AuthorId = author.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Documents.Add(doc);
        _context.SaveChanges();

        DocumentVersion version = new DocumentVersion
        {
            DocumentId = doc.Id,
            VersionNumber = 1,
            Title = "Test",
            Content = "Content",
            ModifiedById = author.Id,
            CreatedAt = DateTime.UtcNow
        };

        _context.DocumentVersions.Add(version);
        _context.SaveChanges();

        int documentId = doc.Id;

        _context.Documents.Remove(doc);
        _context.SaveChanges();

        DocumentVersion? deletedVersion = _context.DocumentVersions.FirstOrDefault(v => v.DocumentId == documentId);
        Assert.Null(deletedVersion);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
