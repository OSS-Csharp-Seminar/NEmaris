using Microsoft.Extensions.DependencyInjection;
using NEmaris.Application.Auth.Interfaces;
using NEmaris.Application.Auth.Services;

namespace NEmaris.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}