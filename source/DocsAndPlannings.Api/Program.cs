using System.Text;
using DocsAndPlannings.Core.Configuration;
using DocsAndPlannings.Core.Data;
using DocsAndPlannings.Core.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=docsandplannings.db"));

// Configure JWT settings
JwtSettings jwtSettings = new JwtSettings
{
    Secret = builder.Configuration["Jwt:Secret"] ?? "default-secret-key-for-development-only-min-32-chars",
    Issuer = builder.Configuration["Jwt:Issuer"] ?? "DocsAndPlannings",
    Audience = builder.Configuration["Jwt:Audience"] ?? "DocsAndPlannings",
    ExpirationMinutes = int.Parse(builder.Configuration["Jwt:ExpirationMinutes"] ?? "60")
};
builder.Services.AddSingleton(jwtSettings);

// Configure authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
        };
    });

builder.Services.AddAuthorization();

// Register application services
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddSingleton<IFileStorageService, FileStorageService>();

// Phase 3: Planning services
builder.Services.AddScoped<IKeyGenerationService, KeyGenerationService>();
builder.Services.AddScoped<IStatusService, StatusService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IEpicService, EpicService>();
builder.Services.AddScoped<IWorkItemService, WorkItemService>();
builder.Services.AddScoped<ICommentService, CommentService>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Seed default statuses
using (var scope = app.Services.CreateScope())
{
    var statusService = scope.ServiceProvider.GetRequiredService<IStatusService>();
    await statusService.CreateDefaultStatusesAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
