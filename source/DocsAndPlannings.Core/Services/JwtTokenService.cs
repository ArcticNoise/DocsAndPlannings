using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DocsAndPlannings.Core.Configuration;
using DocsAndPlannings.Core.Models;
using Microsoft.IdentityModel.Tokens;

namespace DocsAndPlannings.Core.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _jwtSettings;

    public JwtTokenService(JwtSettings jwtSettings)
    {
        Debug.Assert(jwtSettings != null, "JWT settings cannot be null");
        Debug.Assert(!string.IsNullOrEmpty(jwtSettings.Secret), "JWT secret cannot be null or empty");
        Debug.Assert(!string.IsNullOrEmpty(jwtSettings.Issuer), "JWT issuer cannot be null or empty");
        Debug.Assert(!string.IsNullOrEmpty(jwtSettings.Audience), "JWT audience cannot be null or empty");

        _jwtSettings = jwtSettings;
    }

    public string GenerateToken(User user, IEnumerable<string> roles)
    {
        Debug.Assert(user != null, "User cannot be null");
        Debug.Assert(roles != null, "Roles cannot be null");

        List<Claim> claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        byte[] keyBytes = Encoding.UTF8.GetBytes(_jwtSettings.Secret);
        SymmetricSecurityKey key = new SymmetricSecurityKey(keyBytes);
        SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
