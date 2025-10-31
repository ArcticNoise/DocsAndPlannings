using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocsAndPlannings.Api.Controllers;

[ApiController]
[Authorize]
public abstract class BaseApiController : ControllerBase
{
    protected int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
        {
            throw new UnauthorizedAccessException(
                "User ID claim not found in token. Please re-authenticate.");
        }

        if (!int.TryParse(userIdClaim, out int userId))
        {
            throw new UnauthorizedAccessException(
                $"Invalid user ID format in token: '{userIdClaim}'");
        }

        return userId;
    }
}
