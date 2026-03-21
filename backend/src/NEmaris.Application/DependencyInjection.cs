using Microsoft.Extensions.DependencyInjection;
using NEmaris.Application.Interfaces;
using NEmaris.Application.Services;

namespace NEmaris.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITableService, TableService>();
        services.AddScoped<IReservationService, ReservationService>();
        return services;
    }
}
