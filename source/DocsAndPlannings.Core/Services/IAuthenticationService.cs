using DocsAndPlannings.Core.DTOs;

namespace DocsAndPlannings.Core.Services;

public interface IAuthenticationService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
}
