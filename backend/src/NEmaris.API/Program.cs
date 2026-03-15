using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NEmaris.Application;
using NEmaris.Infrastructure;

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

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
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
