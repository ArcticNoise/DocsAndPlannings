using DocsAndPlannings.Core.Services;
using Xunit;

namespace DocsAndPlannings.Core.Tests.Services;

public class PasswordHasherTests
{
    private readonly IPasswordHasher _passwordHasher;

    public PasswordHasherTests()
    {
        _passwordHasher = new PasswordHasher();
    }

    [Fact]
    public void HashPassword_ReturnsNonEmptyString()
    {
        string password = "testPassword123!";

        string hash = _passwordHasher.HashPassword(password);

        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
    }

    [Fact]
    public void HashPassword_GeneratesDifferentHashesForSamePassword()
    {
        string password = "testPassword123!";

        string hash1 = _passwordHasher.HashPassword(password);
        string hash2 = _passwordHasher.HashPassword(password);

        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void VerifyPassword_ReturnsTrue_ForCorrectPassword()
    {
        string password = "testPassword123!";
        string hash = _passwordHasher.HashPassword(password);

        bool result = _passwordHasher.VerifyPassword(password, hash);

        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_ReturnsFalse_ForIncorrectPassword()
    {
        string password = "testPassword123!";
        string wrongPassword = "wrongPassword456!";
        string hash = _passwordHasher.HashPassword(password);

        bool result = _passwordHasher.VerifyPassword(wrongPassword, hash);

        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_ReturnsFalse_ForInvalidHash()
    {
        string password = "testPassword123!";
        string invalidHash = "invalid-hash-string";

        bool result = _passwordHasher.VerifyPassword(password, invalidHash);

        Assert.False(result);
    }

    [Fact]
    public void HashPassword_WorksWithComplexPasswords()
    {
        string complexPassword = "P@ssw0rd!2024#$%^&*()_+-=[]{}|;:',.<>?/~`";

        string hash = _passwordHasher.HashPassword(complexPassword);
        bool result = _passwordHasher.VerifyPassword(complexPassword, hash);

        Assert.True(result);
    }

    [Fact]
    public void HashPassword_WorksWithLongPasswords()
    {
        string longPassword = new string('a', 200);

        string hash = _passwordHasher.HashPassword(longPassword);
        bool result = _passwordHasher.VerifyPassword(longPassword, hash);

        Assert.True(result);
    }
}
