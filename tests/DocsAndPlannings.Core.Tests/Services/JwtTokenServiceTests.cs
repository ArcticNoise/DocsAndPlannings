using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DocsAndPlannings.Core.Configuration;
using DocsAndPlannings.Core.Models;
using DocsAndPlannings.Core.Services;
using Xunit;

namespace DocsAndPlannings.Core.Tests.Services;

public class JwtTokenServiceTests
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly JwtSettings _jwtSettings;

    public JwtTokenServiceTests()
    {
        _jwtSettings = new JwtSettings
        {
            Secret = "test-secret-key-min-32-characters-for-testing",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpirationMinutes = 60
        };

        _jwtTokenService = new JwtTokenService(_jwtSettings);
    }

    [Fact]
    public void GenerateToken_ReturnsNonEmptyString()
    {
        User user = new User
        {
            Id = 1,
            Email = "test@example.com",
            PasswordHash = "hash",
            FirstName = "John",
            LastName = "Doe",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        List<string> roles = new List<string> { "User" };

        string token = _jwtTokenService.GenerateToken(user, roles);

        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public void GenerateToken_CreatesValidJwtToken()
    {
        User user = new User
        {
            Id = 1,
            Email = "test@example.com",
            PasswordHash = "hash",
            FirstName = "John",
            LastName = "Doe",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        List<string> roles = new List<string> { "User" };

        string token = _jwtTokenService.GenerateToken(user, roles);

        JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
        Assert.True(handler.CanReadToken(token));
    }

    [Fact]
    public void GenerateToken_IncludesUserIdClaim()
    {
        User user = new User
        {
            Id = 123,
            Email = "test@example.com",
            PasswordHash = "hash",
            FirstName = "John",
            LastName = "Doe",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        List<string> roles = new List<string> { "User" };

        string token = _jwtTokenService.GenerateToken(user, roles);

        JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
        JwtSecurityToken jwtToken = handler.ReadJwtToken(token);
        Claim? userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

        Assert.NotNull(userIdClaim);
        Assert.Equal("123", userIdClaim.Value);
    }

    [Fact]
    public void GenerateToken_IncludesEmailClaim()
    {
        User user = new User
        {
            Id = 1,
            Email = "test@example.com",
            PasswordHash = "hash",
            FirstName = "John",
            LastName = "Doe",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        List<string> roles = new List<string> { "User" };

        string token = _jwtTokenService.GenerateToken(user, roles);

        JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
        JwtSecurityToken jwtToken = handler.ReadJwtToken(token);
        Claim? emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);

        Assert.NotNull(emailClaim);
        Assert.Equal("test@example.com", emailClaim.Value);
    }

    [Fact]
    public void GenerateToken_IncludesRoleClaims()
    {
        User user = new User
        {
            Id = 1,
            Email = "test@example.com",
            PasswordHash = "hash",
            FirstName = "John",
            LastName = "Doe",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        List<string> roles = new List<string> { "User", "Admin" };

        string token = _jwtTokenService.GenerateToken(user, roles);

        JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
        JwtSecurityToken jwtToken = handler.ReadJwtToken(token);
        List<Claim> roleClaims = jwtToken.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();

        Assert.Equal(2, roleClaims.Count);
        Assert.Contains(roleClaims, c => c.Value == "User");
        Assert.Contains(roleClaims, c => c.Value == "Admin");
    }

    [Fact]
    public void GenerateToken_IncludesIssuerAndAudience()
    {
        User user = new User
        {
            Id = 1,
            Email = "test@example.com",
            PasswordHash = "hash",
            FirstName = "John",
            LastName = "Doe",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        List<string> roles = new List<string> { "User" };

        string token = _jwtTokenService.GenerateToken(user, roles);

        JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
        JwtSecurityToken jwtToken = handler.ReadJwtToken(token);

        Assert.Equal(_jwtSettings.Issuer, jwtToken.Issuer);
        Assert.Contains(_jwtSettings.Audience, jwtToken.Audiences);
    }

    [Fact]
    public void GenerateToken_SetsExpirationTime()
    {
        User user = new User
        {
            Id = 1,
            Email = "test@example.com",
            PasswordHash = "hash",
            FirstName = "John",
            LastName = "Doe",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        List<string> roles = new List<string> { "User" };

        string token = _jwtTokenService.GenerateToken(user, roles);

        JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
        JwtSecurityToken jwtToken = handler.ReadJwtToken(token);

        DateTime expectedExpiry = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);
        TimeSpan difference = (jwtToken.ValidTo - expectedExpiry).Duration();

        Assert.True(difference < TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GenerateToken_WorksWithEmptyRolesList()
    {
        User user = new User
        {
            Id = 1,
            Email = "test@example.com",
            PasswordHash = "hash",
            FirstName = "John",
            LastName = "Doe",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        List<string> roles = new List<string>();

        string token = _jwtTokenService.GenerateToken(user, roles);

        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }
}
