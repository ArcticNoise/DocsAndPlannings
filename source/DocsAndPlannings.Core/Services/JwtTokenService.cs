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
        if (jwtSettings == null)
        {
            throw new ArgumentNullException(nameof(jwtSettings));
        }

        if (string.IsNullOrEmpty(jwtSettings.Secret))
        {
            throw new ArgumentException("JWT secret cannot be null or empty", nameof(jwtSettings));
        }

        if (jwtSettings.Secret.Length < 32)
        {
            throw new ArgumentException("JWT secret must be at least 32 characters (256 bits)", nameof(jwtSettings));
        }

        if (string.IsNullOrEmpty(jwtSettings.Issuer))
        {
            throw new ArgumentException("JWT issuer cannot be null or empty", nameof(jwtSettings));
        }

        if (string.IsNullOrEmpty(jwtSettings.Audience))
        {
            throw new ArgumentException("JWT audience cannot be null or empty", nameof(jwtSettings));
        }

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
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
