using DocsAndPlannings.Core.Configuration;
using DocsAndPlannings.Core.Data;
using DocsAndPlannings.Core.DTOs;
using DocsAndPlannings.Core.Models;
using DocsAndPlannings.Core.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DocsAndPlannings.Core.Tests.Services;

public class AuthenticationServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IAuthenticationService _authenticationService;
    private readonly JwtSettings _jwtSettings;

    public AuthenticationServiceTests()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        _passwordHasher = new PasswordHasher();

        _jwtSettings = new JwtSettings
        {
            Secret = "test-secret-key-min-32-characters-for-testing",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpirationMinutes = 60
        };

        _jwtTokenService = new JwtTokenService(_jwtSettings);

        _authenticationService = new AuthenticationService(
            _context,
            _passwordHasher,
            _jwtTokenService,
            _jwtSettings);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task RegisterAsync_CreatesNewUser()
    {
        RegisterRequest request = new RegisterRequest
        {
            Email = "newuser@example.com",
            Password = "password123!",
            FirstName = "John",
            LastName = "Doe"
        };

        AuthResponse response = await _authenticationService.RegisterAsync(request);

        Assert.NotNull(response);
        Assert.NotNull(response.Token);
        Assert.Equal(request.Email, response.User.Email);
        Assert.Equal(request.FirstName, response.User.FirstName);
        Assert.Equal(request.LastName, response.User.LastName);
        Assert.True(response.User.IsActive);
    }

    [Fact]
    public async Task RegisterAsync_HashesPassword()
    {
        RegisterRequest request = new RegisterRequest
        {
            Email = "newuser@example.com",
            Password = "password123!",
            FirstName = "John",
            LastName = "Doe"
        };

        await _authenticationService.RegisterAsync(request);

        User? user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        Assert.NotNull(user);
        Assert.NotEqual(request.Password, user.PasswordHash);
        Assert.True(_passwordHasher.VerifyPassword(request.Password, user.PasswordHash));
    }

    [Fact]
    public async Task RegisterAsync_ThrowsException_WhenEmailAlreadyExists()
    {
        User existingUser = new User
        {
            Email = "existing@example.com",
            PasswordHash = "hash",
            FirstName = "Jane",
            LastName = "Smith",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        RegisterRequest request = new RegisterRequest
        {
            Email = "existing@example.com",
            Password = "password123!",
            FirstName = "John",
            LastName = "Doe"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _authenticationService.RegisterAsync(request));
    }

    [Fact]
    public async Task RegisterAsync_ReturnsValidToken()
    {
        RegisterRequest request = new RegisterRequest
        {
            Email = "newuser@example.com",
            Password = "password123!",
            FirstName = "John",
            LastName = "Doe"
        };

        AuthResponse response = await _authenticationService.RegisterAsync(request);

        Assert.NotNull(response.Token);
        Assert.NotEmpty(response.Token);
    }

    [Fact]
    public async Task RegisterAsync_SetsExpirationTime()
    {
        RegisterRequest request = new RegisterRequest
        {
            Email = "newuser@example.com",
            Password = "password123!",
            FirstName = "John",
            LastName = "Doe"
        };

        AuthResponse response = await _authenticationService.RegisterAsync(request);

        DateTime expectedExpiry = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);
        TimeSpan difference = (response.ExpiresAt - expectedExpiry).Duration();
        Assert.True(difference < TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task LoginAsync_ReturnsAuthResponse_ForValidCredentials()
    {
        string password = "password123!";
        string passwordHash = _passwordHasher.HashPassword(password);

        User user = new User
        {
            Email = "test@example.com",
            PasswordHash = passwordHash,
            FirstName = "John",
            LastName = "Doe",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        LoginRequest request = new LoginRequest
        {
            Email = "test@example.com",
            Password = password
        };

        AuthResponse response = await _authenticationService.LoginAsync(request);

        Assert.NotNull(response);
        Assert.NotNull(response.Token);
        Assert.Equal(user.Email, response.User.Email);
        Assert.Equal(user.FirstName, response.User.FirstName);
        Assert.Equal(user.LastName, response.User.LastName);
    }

    [Fact]
    public async Task LoginAsync_ThrowsException_ForInvalidEmail()
    {
        LoginRequest request = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "password123!"
        };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await _authenticationService.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_ThrowsException_ForInvalidPassword()
    {
        string password = "password123!";
        string passwordHash = _passwordHasher.HashPassword(password);

        User user = new User
        {
            Email = "test@example.com",
            PasswordHash = passwordHash,
            FirstName = "John",
            LastName = "Doe",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        LoginRequest request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "wrongpassword"
        };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await _authenticationService.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_ThrowsException_ForInactiveUser()
    {
        string password = "password123!";
        string passwordHash = _passwordHasher.HashPassword(password);

        User user = new User
        {
            Email = "test@example.com",
            PasswordHash = passwordHash,
            FirstName = "John",
            LastName = "Doe",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = false
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        LoginRequest request = new LoginRequest
        {
            Email = "test@example.com",
            Password = password
        };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await _authenticationService.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_IncludesUserRolesInResponse()
    {
        string password = "password123!";
        string passwordHash = _passwordHasher.HashPassword(password);

        User user = new User
        {
            Email = "test@example.com",
            PasswordHash = passwordHash,
            FirstName = "John",
            LastName = "Doe",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        Role role1 = new Role
        {
            Name = "User",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Role role2 = new Role
        {
            Name = "Admin",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        _context.Roles.Add(role1);
        _context.Roles.Add(role2);
        await _context.SaveChangesAsync();

        UserRole userRole1 = new UserRole
        {
            UserId = user.Id,
            RoleId = role1.Id,
            AssignedAt = DateTime.UtcNow
        };

        UserRole userRole2 = new UserRole
        {
            UserId = user.Id,
            RoleId = role2.Id,
            AssignedAt = DateTime.UtcNow
        };

        _context.UserRoles.Add(userRole1);
        _context.UserRoles.Add(userRole2);
        await _context.SaveChangesAsync();

        LoginRequest request = new LoginRequest
        {
            Email = "test@example.com",
            Password = password
        };

        AuthResponse response = await _authenticationService.LoginAsync(request);

        Assert.Equal(2, response.User.Roles.Count);
        Assert.Contains("User", response.User.Roles);
        Assert.Contains("Admin", response.User.Roles);
    }
}
