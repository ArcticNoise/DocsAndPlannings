using System.Diagnostics;

namespace DocsAndPlannings.Core.Services;

public class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string HashPassword(string password)
    {
        Debug.Assert(!string.IsNullOrEmpty(password), "Password cannot be null or empty");

        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        Debug.Assert(!string.IsNullOrEmpty(password), "Password cannot be null or empty");
        Debug.Assert(!string.IsNullOrEmpty(passwordHash), "Password hash cannot be null or empty");

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch
        {
            return false;
        }
    }
}
