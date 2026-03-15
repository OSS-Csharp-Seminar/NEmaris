using Microsoft.Extensions.DependencyInjection;
using NEmaris.Application.Auth.Interfaces;
using NEmaris.Application.Auth.Services;
using NEmaris.Application.Interace_s;
using NEmaris.Application.Service_s;

namespace NEmaris.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITableService, TableService>();
        return services;
    }
}
