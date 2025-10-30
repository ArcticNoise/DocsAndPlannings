using DocsAndPlannings.Core.Models;

namespace DocsAndPlannings.Core.Services;

public interface IJwtTokenService
{
    string GenerateToken(User user, IEnumerable<string> roles);
}
