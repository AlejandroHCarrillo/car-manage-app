using Microsoft.Extensions.DependencyInjection;
using PersonalCarGarage.Application.Auth;
using PersonalCarGarage.Application.Maintenance;
using PersonalCarGarage.Application.Users;
using PersonalCarGarage.Application.Procedures;
using PersonalCarGarage.Application.Vehicles;

namespace PersonalCarGarage.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IVehicleService, VehicleService>();
        services.AddScoped<IMaintenanceService, MaintenanceService>();
        services.AddScoped<IProcedureService, ProcedureService>();
        return services;
    }
}
