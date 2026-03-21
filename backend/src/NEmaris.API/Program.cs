using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using NEmaris.Application;
using NEmaris.Domain.Entities;
using NEmaris.Domain.Enums;
using NEmaris.Infrastructure;
using NEmaris.Infrastructure.Persistence;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Layers
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructure(builder.Configuration);

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKey = ResolveJwtKey(builder.Configuration, builder.Environment);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddCors();

var app = builder.Build();
app.UseCors(policy => policy
    .WithOrigins("http://localhost:3000") 
    .AllowAnyHeader()
    .AllowAnyMethod());

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
// Seed admin user
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Make sure tables exist
    await db.Database.EnsureCreatedAsync();

    if (await userManager.FindByEmailAsync("admin@nemaris.com") is null)
    {
        var admin = new ApplicationUser
        {
            UserName = "admin@nemaris.com",
            Email = "admin@nemaris.com",
            FirstName = "Admin",
            LastName = "User",
            Role = UserRole.Admin,
            Status = UserStatus.Active,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(admin, "Admin123!");

        if (result.Succeeded)
            Console.WriteLine("✅ Admin user seeded successfully");
        else
            Console.WriteLine($"❌ Admin seeding failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
    }
    else
    {
        Console.WriteLine("ℹ️ Admin user already exists");
    }
}

app.Run();

static string ResolveJwtKey(IConfiguration configuration, IHostEnvironment environment)
{
    const string developmentFallbackKey = "NEmaris-Development-JWT-Key-AtLeast-32Chars-2026";

    var configuredKey = configuration["Jwt:Key"];
    var jwtKey = string.IsNullOrWhiteSpace(configuredKey) && environment.IsDevelopment()
        ? developmentFallbackKey
        : configuredKey;

    if (string.IsNullOrWhiteSpace(jwtKey))
        throw new InvalidOperationException("JWT key is missing. Set Jwt:Key in configuration or Jwt__Key environment variable.");

    if (jwtKey.Length < 32)
        throw new InvalidOperationException("JWT key is too short. Use at least 32 characters.");

    return jwtKey;
}
