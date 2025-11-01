using System.Security.Claims;
using DocsAndPlannings.Core.DTOs;
using DocsAndPlannings.Web.Services;
using DocsAndPlannings.Web.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocsAndPlannings.Web.Controllers;

public class AccountController : Controller
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        IApiClient apiClient,
        ILogger<AccountController> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    // GET: /Account/Login
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated ?? false)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    // POST: /Account/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            LoginRequest request = new()
            {
                Email = model.Email,
                Password = model.Password
            };

            AuthResponse? response = await _apiClient.PostAsync<AuthResponse>("/api/auth/login", request);

            if (response is null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt");
                return View(model);
            }

            // Create claims for the user
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, response.User.Id.ToString()),
                new Claim(ClaimTypes.Name, $"{response.User.FirstName} {response.User.LastName}"),
                new Claim(ClaimTypes.Email, response.User.Email)
            };

            // Add role claims
            foreach (string role in response.User.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            ClaimsIdentity identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);

            AuthenticationProperties authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = response.ExpiresAt
            };

            // Sign in the user with cookie authentication
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authProperties);

            // Store JWT token in secure cookie for API calls
            CookieOptions cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = response.ExpiresAt
            };

            Response.Cookies.Append("AuthToken", response.Token, cookieOptions);

            _logger.LogInformation("User {Email} logged in successfully", model.Email);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Dashboard");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "An error occurred during login. Please try again.");
            return View(model);
        }
    }

    // GET: /Account/Register
    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated ?? false)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        return View(new RegisterViewModel());
    }

    // POST: /Account/Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            RegisterRequest request = new()
            {
                Email = model.Email,
                Password = model.Password,
                FirstName = model.FirstName,
                LastName = model.LastName
            };

            AuthResponse? response = await _apiClient.PostAsync<AuthResponse>("/api/auth/register", request);

            if (response is null)
            {
                ModelState.AddModelError(string.Empty, "Registration failed. Please try again.");
                return View(model);
            }

            // Create claims for the user
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, response.User.Id.ToString()),
                new Claim(ClaimTypes.Name, $"{response.User.FirstName} {response.User.LastName}"),
                new Claim(ClaimTypes.Email, response.User.Email)
            };

            // Add role claims
            foreach (string role in response.User.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            ClaimsIdentity identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);

            AuthenticationProperties authProperties = new AuthenticationProperties
            {
                IsPersistent = false,
                ExpiresUtc = response.ExpiresAt
            };

            // Sign in the user with cookie authentication
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authProperties);

            // Store JWT token in secure cookie for API calls
            CookieOptions cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = response.ExpiresAt
            };

            Response.Cookies.Append("AuthToken", response.Token, cookieOptions);

            _logger.LogInformation("User {Email} registered successfully", model.Email);

            TempData["SuccessMessage"] = "Registration successful! Welcome to Docs & Plannings.";

            return RedirectToAction("Index", "Dashboard");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for user {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "An error occurred during registration. Please try again.");
            return View(model);
        }
    }

    // POST: /Account/Logout
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        string? userEmail = User.FindFirstValue(ClaimTypes.Email);

        // Sign out from cookie authentication
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // Remove JWT token cookie
        Response.Cookies.Delete("AuthToken");

        _logger.LogInformation("User {Email} logged out", userEmail);

        return RedirectToAction("Index", "Home");
    }

    // GET: /Account/Profile
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        try
        {
            UserDto? user = await _apiClient.GetAsync<UserDto>("/api/users/me");

            if (user is null)
            {
                _logger.LogWarning("Failed to retrieve user profile");
                TempData["ErrorMessage"] = "Failed to load profile information.";
                return RedirectToAction("Index", "Dashboard");
            }

            ProfileViewModel viewModel = new ProfileViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive,
                Roles = user.Roles
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user profile");
            TempData["ErrorMessage"] = "An error occurred while loading your profile.";
            return RedirectToAction("Index", "Dashboard");
        }
    }
}
