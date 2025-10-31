using DocsAndPlannings.Core.Data;
using DocsAndPlannings.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DocsAndPlannings.Core.Tests.Data;

public class AuthenticationModelsTests : IDisposable
{
    private readonly ApplicationDbContext _context;

    public AuthenticationModelsTests()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new ApplicationDbContext(options);
    }

    [Fact]
    public void CanCreateRole()
    {
        Role role = new Role
        {
            Name = "Admin",
            Description = "Administrator role",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Roles.Add(role);
        int result = _context.SaveChanges();

        Assert.Equal(1, result);
        Assert.True(role.Id > 0);
        Assert.True(role.IsActive);
    }

    [Fact]
    public void RoleNameMaxLengthIsConfiguredCorrectly()
    {
        Microsoft.EntityFrameworkCore.Metadata.IEntityType? model = _context.Model.FindEntityType(typeof(Role));
        Assert.NotNull(model);

        Microsoft.EntityFrameworkCore.Metadata.IProperty? nameProperty = model.FindProperty(nameof(Role.Name));
        Assert.NotNull(nameProperty);

        Assert.Equal(50, nameProperty.GetMaxLength());
    }

    [Fact]
    public void CanAssignUserToRole()
    {
        User user = new User
        {
            Email = "user@example.com",
            PasswordHash = "hash123",
            FirstName = "John",
            LastName = "Doe",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Role role = new Role
        {
            Name = "User",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        _context.Roles.Add(role);
        _context.SaveChanges();

        UserRole userRole = new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id,
            AssignedAt = DateTime.UtcNow
        };

        _context.UserRoles.Add(userRole);
        int result = _context.SaveChanges();

        Assert.Equal(1, result);
    }

    [Fact]
    public void CanQueryUserRoles()
    {
        User user = new User
        {
            Email = "user@example.com",
            PasswordHash = "hash123",
            FirstName = "John",
            LastName = "Doe",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Role adminRole = new Role
        {
            Name = "Admin",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Role userRole = new Role
        {
            Name = "User",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        _context.Roles.AddRange(adminRole, userRole);
        _context.SaveChanges();

        _context.UserRoles.AddRange(
            new UserRole { UserId = user.Id, RoleId = adminRole.Id, AssignedAt = DateTime.UtcNow },
            new UserRole { UserId = user.Id, RoleId = userRole.Id, AssignedAt = DateTime.UtcNow }
        );
        _context.SaveChanges();

        User? foundUser = _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefault(u => u.Email == "user@example.com");

        Assert.NotNull(foundUser);
        Assert.Equal(2, foundUser.UserRoles.Count);
    }

    [Fact]
    public void DeletingUserCascadesDeleteUserRoles()
    {
        User user = new User
        {
            Email = "user@example.com",
            PasswordHash = "hash123",
            FirstName = "John",
            LastName = "Doe",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Role role = new Role
        {
            Name = "User",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        _context.Roles.Add(role);
        _context.SaveChanges();

        UserRole userRole = new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id,
            AssignedAt = DateTime.UtcNow
        };

        _context.UserRoles.Add(userRole);
        _context.SaveChanges();

        int userId = user.Id;
        int roleId = role.Id;

        _context.Users.Remove(user);
        _context.SaveChanges();

        UserRole? deletedUserRole = _context.UserRoles.FirstOrDefault(ur => ur.UserId == userId && ur.RoleId == roleId);
        Assert.Null(deletedUserRole);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
