using System.Diagnostics;
using DocsAndPlannings.Core.Configuration;
using DocsAndPlannings.Core.Data;
using DocsAndPlannings.Core.DTOs;
using DocsAndPlannings.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DocsAndPlannings.Core.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly JwtSettings _jwtSettings;

    public AuthenticationService(
        ApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        JwtSettings jwtSettings)
    {
        Debug.Assert(context != null, "Database context cannot be null");
        Debug.Assert(passwordHasher != null, "Password hasher cannot be null");
        Debug.Assert(jwtTokenService != null, "JWT token service cannot be null");
        Debug.Assert(jwtSettings != null, "JWT settings cannot be null");

        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _jwtSettings = jwtSettings;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        Debug.Assert(request != null, "Register request cannot be null");

        bool emailExists = await _context.Users.AnyAsync(u => u.Email == request.Email);
        if (emailExists)
        {
            throw new InvalidOperationException($"User with email '{request.Email}' already exists");
        }

        string passwordHash = _passwordHasher.HashPassword(request.Password);

        User user = new User
        {
            Email = request.Email,
            PasswordHash = passwordHash,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        await _context.Entry(user).Collection(u => u.UserRoles).LoadAsync();
        List<string> roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();

        string token = _jwtTokenService.GenerateToken(user, roles);

        return new AuthResponse
        {
            Token = token,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive,
                Roles = roles
            },
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes)
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        Debug.Assert(request != null, "Login request cannot be null");

        User? user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("User account is inactive");
        }

        bool isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        List<string> roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        string token = _jwtTokenService.GenerateToken(user, roles);

        return new AuthResponse
        {
            Token = token,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive,
                Roles = roles
            },
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes)
        };
    }
}
