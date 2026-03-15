using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NEmaris.Application.Interfaces;
using NEmaris.Domain.Entities;
using NEmaris.Infrastructure.Persistence;
using NEmaris.Infrastructure.Repositories;
using NEmaris.Infrastructure.Services;

namespace NEmaris.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(
                config.GetConnectionString("Default"),
                ServerVersion.AutoDetect(config.GetConnectionString("Default"))
            )
        );

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ITableRepository, TableRepository>();

        return services;
    }
}
