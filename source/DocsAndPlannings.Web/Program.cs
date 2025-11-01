using DocsAndPlannings.Core.Data;
using DocsAndPlannings.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=docsandplannings.db"));

// Register HttpClient for API communication
builder.Services.AddHttpClient<IApiClient, ApiClient>(client =>
{
    string? apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"];
    if (string.IsNullOrEmpty(apiBaseUrl))
    {
        throw new InvalidOperationException("API BaseUrl is not configured in appsettings.json");
    }
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Register HttpContextAccessor for accessing HttpContext in services
builder.Services.AddHttpContextAccessor();

// Configure Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Error/Forbidden";
        options.ExpireTimeSpan = TimeSpan.FromHours(24);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
    });

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Security Headers Middleware
app.Use(async (context, next) =>
{
    // Content Security Policy (CSP)
    // Note: In development, we're more permissive. In production, tighten these policies.
    string cspPolicy = app.Environment.IsDevelopment()
        ? "default-src 'self'; " +
          "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net; " +
          "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
          "img-src 'self' data: https:; " +
          "font-src 'self' https://cdn.jsdelivr.net; " +
          "connect-src 'self'; " +
          "frame-ancestors 'none'; " +
          "base-uri 'self'; " +
          "form-action 'self';"
        : "default-src 'self'; " +
          "script-src 'self' https://cdn.jsdelivr.net; " +
          "style-src 'self' https://cdn.jsdelivr.net; " +
          "img-src 'self' data: https:; " +
          "font-src 'self' https://cdn.jsdelivr.net; " +
          "connect-src 'self'; " +
          "frame-ancestors 'none'; " +
          "base-uri 'self'; " +
          "form-action 'self'; " +
          "upgrade-insecure-requests;";

    context.Response.Headers.Append("Content-Security-Policy", cspPolicy);

    // Additional Security Headers
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Permissions-Policy", "geolocation=(), microphone=(), camera=()");

    await next();
});

// Handle HTTP status codes (404, 401, 403, 500, etc.)
app.UseStatusCodePagesWithReExecute("/Error/StatusCode/{0}");

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
