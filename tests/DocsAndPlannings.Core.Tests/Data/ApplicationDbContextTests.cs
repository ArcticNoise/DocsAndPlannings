using Microsoft.EntityFrameworkCore;
using DocsAndPlannings.Core.Data;
using DocsAndPlannings.Core.Models;

namespace DocsAndPlannings.Core.Tests.Data;

public class ApplicationDbContextTests : IDisposable
{
    private readonly ApplicationDbContext _context;

    public ApplicationDbContextTests()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new ApplicationDbContext(options);
    }

    [Fact]
    public void CanCreateDatabase()
    {
        bool created = _context.Database.EnsureCreated();
        Assert.True(created || _context.Database.CanConnect());
    }

    [Fact]
    public void CanAddUser()
    {
        User user = new User
        {
            Email = "test@example.com",
            PasswordHash = "hash123",
            FirstName = "John",
            LastName = "Doe",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Users.Add(user);
        int result = _context.SaveChanges();

        Assert.Equal(1, result);
        Assert.True(user.Id > 0);
    }

    [Fact(Skip = "InMemory database doesn't enforce unique constraints. Integration tests with SQLite will validate this.")]
    public void EmailMustBeUnique()
    {
        User user1 = new User
        {
            Email = "duplicate@example.com",
            PasswordHash = "hash1",
            FirstName = "User",
            LastName = "One",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        User user2 = new User
        {
            Email = "duplicate@example.com",
            PasswordHash = "hash2",
            FirstName = "User",
            LastName = "Two",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Users.Add(user1);
        _context.SaveChanges();

        _context.Users.Add(user2);

        Assert.Throws<InvalidOperationException>(() => _context.SaveChanges());
    }

    [Fact]
    public void CanQueryUsers()
    {
        User user = new User
        {
            Email = "query@example.com",
            PasswordHash = "hash123",
            FirstName = "Query",
            LastName = "Test",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        User? foundUser = _context.Users.FirstOrDefault(u => u.Email == "query@example.com");

        Assert.NotNull(foundUser);
        Assert.Equal("Query", foundUser.FirstName);
        Assert.Equal("Test", foundUser.LastName);
    }

    [Fact]
    public void CanUpdateUser()
    {
        User user = new User
        {
            Email = "update@example.com",
            PasswordHash = "hash123",
            FirstName = "Original",
            LastName = "Name",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        user.FirstName = "Updated";
        user.UpdatedAt = DateTime.UtcNow;
        _context.SaveChanges();

        User? updatedUser = _context.Users.Find(user.Id);

        Assert.NotNull(updatedUser);
        Assert.Equal("Updated", updatedUser.FirstName);
    }

    [Fact]
    public void CanDeleteUser()
    {
        User user = new User
        {
            Email = "delete@example.com",
            PasswordHash = "hash123",
            FirstName = "Delete",
            LastName = "Me",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        int userId = user.Id;

        _context.Users.Remove(user);
        _context.SaveChanges();

        User? deletedUser = _context.Users.Find(userId);

        Assert.Null(deletedUser);
    }

    [Fact]
    public void EmailMaxLengthIsConfiguredCorrectly()
    {
        Microsoft.EntityFrameworkCore.Metadata.IEntityType? model = _context.Model.FindEntityType(typeof(User));
        Assert.NotNull(model);

        Microsoft.EntityFrameworkCore.Metadata.IProperty? emailProperty = model.FindProperty(nameof(User.Email));
        Assert.NotNull(emailProperty);

        Assert.Equal(256, emailProperty.GetMaxLength());
    }

    [Fact]
    public void IsActiveDefaultsToTrue()
    {
        User user = new User
        {
            Email = "default@example.com",
            PasswordHash = "hash123",
            FirstName = "Default",
            LastName = "User",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        User? savedUser = _context.Users.Find(user.Id);

        Assert.NotNull(savedUser);
        Assert.True(savedUser.IsActive);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
