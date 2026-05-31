using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using NEmaris.Application;
using NEmaris.Domain.Entities;
using NEmaris.Domain.Enums;
using NEmaris.Infrastructure;
using NEmaris.Infrastructure.Persistence;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;

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

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
#pragma warning disable ASPDEPR005 // KnownNetworks is obsolete; container SDK doesn't have KnownIPNetworks yet
    options.KnownNetworks.Clear();
#pragma warning restore ASPDEPR005
    options.KnownProxies.Clear();
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("chat", httpContext =>
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        });
    });
});

var app = builder.Build();
app.UseForwardedHeaders();
app.UseCors(policy => policy
    .WithOrigins("http://localhost:3000")
    .AllowAnyHeader()
    .AllowAnyMethod());

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/api/table-layout/tables", async (NEmaris.Application.Interfaces.ITableService tableService) =>
{
    var tables = await tableService.GetAllAsync();

    var response = tables
        .OrderBy(table => table.Floor)
        .ThenBy(table => table.TableNumber)
        .Select(table => new
        {
            table.Id,
            table.TableNumber,
            table.Capacity,
            table.GuestCount,
            table.Zone,
            Status = (int)table.Status,
            table.Floor,
            table.PositionX,
            table.PositionY,
            Shape = (int)table.Shape,
            table.Rotation,
            table.CreatedAt,
            table.UpdatedAt
        });

    return Results.Ok(response);
});
app.MapControllers();
// Seed admin user
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Make sure tables exist
    await db.Database.EnsureCreatedAsync();
    await EnsureTableGuestCountColumnAsync(db);
    await EnsureMenuItemStockQuantityColumnAsync(db);

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

static async Task EnsureTableGuestCountColumnAsync(AppDbContext db)
{
    const string columnExistsSql = """
        SELECT COUNT(*)
        FROM information_schema.columns
        WHERE table_schema = DATABASE()
          AND table_name = 'restaurant_tables'
          AND column_name = 'guest_count'
        """;

    await db.Database.OpenConnectionAsync();
    try
    {
        await using var command = db.Database.GetDbConnection().CreateCommand();
        command.CommandText = columnExistsSql;
        var columnExists = Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;

        if (!columnExists)
        {
            await db.Database.ExecuteSqlRawAsync("""
                ALTER TABLE restaurant_tables
                ADD COLUMN guest_count INT NOT NULL DEFAULT 0 AFTER capacity
                """);
        }

        await db.Database.ExecuteSqlRawAsync("""
            UPDATE restaurant_tables
            SET guest_count = 1
            WHERE guest_count = 0 AND status IN (1, 2)
            """);
    }
    finally
    {
        await db.Database.CloseConnectionAsync();
    }
}

static async Task EnsureMenuItemStockQuantityColumnAsync(AppDbContext db)
{
    const string columnExistsSql = """
        SELECT COUNT(*)
        FROM information_schema.columns
        WHERE table_schema = DATABASE()
          AND table_name = 'menu_items'
          AND column_name = 'stock_quantity'
        """;

    await db.Database.OpenConnectionAsync();
    try
    {
        await using var command = db.Database.GetDbConnection().CreateCommand();
        command.CommandText = columnExistsSql;
        var columnExists = Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;

        if (!columnExists)
        {
            await db.Database.ExecuteSqlRawAsync("""
                ALTER TABLE menu_items
                ADD COLUMN stock_quantity INT NOT NULL DEFAULT 0 AFTER is_available
                """);
        }

        await db.Database.ExecuteSqlRawAsync("""
            UPDATE menu_items
            SET stock_quantity = CASE sku
                WHEN 'DRK-COL' THEN 24
                WHEN 'COF-CAP' THEN 30
                WHEN 'COF-ESP' THEN 40
                WHEN 'BRK-BEN' THEN 12
                WHEN 'LCH-WRP' THEN 16
                WHEN 'DIN-RUMP-WOK' THEN 8
                ELSE stock_quantity
            END
            WHERE sku IN ('DRK-COL', 'COF-CAP', 'COF-ESP', 'BRK-BEN', 'LCH-WRP', 'DIN-RUMP-WOK')
              AND stock_quantity = 0
            """);
    }
    finally
    {
        await db.Database.CloseConnectionAsync();
    }
}

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
