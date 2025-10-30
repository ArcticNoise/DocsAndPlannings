using DocsAndPlannings.Core.Configuration;
using DocsAndPlannings.Core.Data;
using DocsAndPlannings.Core.DTOs;
using DocsAndPlannings.Core.Models;
using DocsAndPlannings.Core.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DocsAndPlannings.Core.Tests.Services;

public class AuthenticationServiceBugHuntTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IAuthenticationService _authenticationService;
    private readonly JwtSettings _jwtSettings;

    public AuthenticationServiceBugHuntTests()
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
    public async Task BugHunt_RegisterAsync_AllowsDuplicateEmailsWithDifferentCase()
    {
        RegisterRequest request1 = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "password123!",
            FirstName = "John",
            LastName = "Doe"
        };

        await _authenticationService.RegisterAsync(request1);

        RegisterRequest request2 = new RegisterRequest
        {
            Email = "Test@Example.Com",
            Password = "password456!",
            FirstName = "Jane",
            LastName = "Smith"
        };

        // After fix: Should throw InvalidOperationException for duplicate email (case-insensitive)
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _authenticationService.RegisterAsync(request2));

        // Verify only one user was created
        int userCount = await _context.Users.CountAsync();
        Assert.Equal(1, userCount);
    }

    [Fact]
    public async Task BugHunt_LoginAsync_EmailCaseSensitivity()
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

        LoginRequest requestUpperCase = new LoginRequest
        {
            Email = "TEST@EXAMPLE.COM",
            Password = password
        };

        // After fix: Login should succeed with mixed-case email (case-insensitive)
        AuthResponse response = await _authenticationService.LoginAsync(requestUpperCase);

        Assert.NotNull(response);
        Assert.NotNull(response.Token);
        Assert.Equal(user.Email, response.User.Email);
    }

    [Fact]
    public async Task BugHunt_RegisterAsync_WithEmptyRolesList()
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
        Assert.Empty(response.User.Roles);
        Assert.NotNull(response.Token);
    }

    [Fact]
    public async Task BugHunt_LoginAsync_TimingAttackConsistency()
    {
        string password = "password123!";
        string passwordHash = _passwordHasher.HashPassword(password);

        User user = new User
        {
            Email = "existing@example.com",
            PasswordHash = passwordHash,
            FirstName = "John",
            LastName = "Doe",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        LoginRequest invalidEmailRequest = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = password
        };

        LoginRequest invalidPasswordRequest = new LoginRequest
        {
            Email = "existing@example.com",
            Password = "wrongpassword"
        };

        Exception? exception1 = await Record.ExceptionAsync(
            async () => await _authenticationService.LoginAsync(invalidEmailRequest));

        Exception? exception2 = await Record.ExceptionAsync(
            async () => await _authenticationService.LoginAsync(invalidPasswordRequest));

        Assert.NotNull(exception1);
        Assert.NotNull(exception2);
        Assert.IsType<UnauthorizedAccessException>(exception1);
        Assert.IsType<UnauthorizedAccessException>(exception2);

        Assert.Equal(exception1.Message, exception2.Message);
    }

    [Fact]
    public async Task BugHunt_RegisterAsync_WithVeryLongEmail()
    {
        string longEmail = new string('a', 250) + "@example.com";

        RegisterRequest request = new RegisterRequest
        {
            Email = longEmail,
            Password = "password123!",
            FirstName = "John",
            LastName = "Doe"
        };

        Exception? exception = await Record.ExceptionAsync(
            async () => await _authenticationService.RegisterAsync(request));

        if (exception != null)
        {
            Assert.True(
                exception is InvalidOperationException ||
                exception is DbUpdateException,
                $"Expected InvalidOperationException or DbUpdateException but got {exception.GetType().Name}");
        }
    }

    [Fact]
    public async Task BugHunt_RegisterAsync_WithSpecialCharactersInName()
    {
        RegisterRequest request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "password123!",
            FirstName = "John<script>alert('xss')</script>",
            LastName = "O'Brien"
        };

        AuthResponse response = await _authenticationService.RegisterAsync(request);

        Assert.NotNull(response);
        Assert.Equal(request.FirstName, response.User.FirstName);
        Assert.Equal(request.LastName, response.User.LastName);
    }

    [Fact]
    public async Task BugHunt_LoginAsync_WithSqlInjectionAttempt()
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
            Email = "test@example.com' OR '1'='1",
            Password = "password123!"
        };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await _authenticationService.LoginAsync(request));
    }
}
