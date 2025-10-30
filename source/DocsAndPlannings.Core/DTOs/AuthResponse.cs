namespace DocsAndPlannings.Core.DTOs;

public class AuthResponse
{
    public required string Token { get; set; }
    public required UserDto User { get; set; }
    public DateTime ExpiresAt { get; set; }
}
