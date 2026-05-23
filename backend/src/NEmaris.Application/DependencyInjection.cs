using Microsoft.Extensions.DependencyInjection;
using NEmaris.Application.Interfaces;
using NEmaris.Application.Services;
using NEmaris.Application.Services.ChatTools;

namespace NEmaris.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITableService, TableService>();
        services.AddScoped<IMenuCategoryService, MenuCategoryService>();
        services.AddScoped<IMenuItemService, MenuItemService>();
        services.AddScoped<IReservationService, ReservationService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IChatService, ChatService>();

        services.AddScoped<IChatTool, GetAvailableTablesTool>();
        services.AddScoped<IChatTool, CreateReservationTool>();
        services.AddScoped<IChatTool, FindReservationsByPhoneTool>();
        services.AddScoped<IChatTool, CancelReservationTool>();
        services.AddScoped<IChatTool, UpdateReservationTool>();

        return services;
    }
}



