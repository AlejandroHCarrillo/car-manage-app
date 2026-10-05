using Microsoft.EntityFrameworkCore;
using PersonalCarGarage.Domain.Entities;
using PersonalCarGarage.Domain.Interfaces;

namespace PersonalCarGarage.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<UserVehicle> UserVehicles => Set<UserVehicle>();
    public DbSet<MaintenanceRecord> MaintenanceRecords => Set<MaintenanceRecord>();
    public DbSet<VehicleProcedure> VehicleProcedures => Set<VehicleProcedure>();
    public DbSet<ProcedureCompletion> ProcedureCompletions => Set<ProcedureCompletion>();
    public DbSet<VehiclePhoto> VehiclePhotos => Set<VehiclePhoto>();
    public DbSet<VehicleDocument> VehicleDocuments => Set<VehicleDocument>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
