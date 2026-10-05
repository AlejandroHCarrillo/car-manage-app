using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PersonalCarGarage.Application.Abstractions;
using PersonalCarGarage.Application.Options;
using PersonalCarGarage.Domain.Interfaces;
using PersonalCarGarage.Infrastructure.Identity;
using PersonalCarGarage.Infrastructure.Persistence;
using PersonalCarGarage.Infrastructure.Persistence.Repositories;
using PersonalCarGarage.Infrastructure.Storage;

namespace PersonalCarGarage.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<IMaintenanceRepository, MaintenanceRepository>();
        services.AddScoped<IProcedureRepository, ProcedureRepository>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<DatabaseSeeder>();
        services.AddSingleton<IPhotoStorage, LocalPhotoStorage>();

        return services;
    }
}
