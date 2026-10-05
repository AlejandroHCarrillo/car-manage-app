using Microsoft.EntityFrameworkCore;
using PersonalCarGarage.Application.Abstractions;
using PersonalCarGarage.Domain.Entities;
using PersonalCarGarage.Domain.Enums;

namespace PersonalCarGarage.Infrastructure.Persistence;

public class DatabaseSeeder(AppDbContext db, IPasswordHasher passwordHasher)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await db.Database.MigrateAsync(cancellationToken);

        if (await db.Users.AnyAsync(cancellationToken))
        {
            await SeedProceduresAsync(cancellationToken);
            return;
        }

        var admin = new User
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Email = "admin@personalcargarage.com",
            PasswordHash = passwordHasher.Hash("Admin123!"),
            FirstName = "Ana",
            LastName = "Admin",
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var manager = new User
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Email = "manager@personalcargarage.com",
            PasswordHash = passwordHasher.Hash("Manager123!"),
            FirstName = "Mario",
            LastName = "Gestor",
            Role = UserRole.VehicleManager,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var civic = new Vehicle
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Brand = "Honda",
            Model = "Civic",
            Year = 2021,
            LicensePlate = "ABC-1234",
            Vin = "1HGCM82633A004352",
            Color = "Gris",
            CurrentMileage = 45200,
            Notes = "Sedán familiar. Cambio de aceite cada 8,000 km.",
            CreatedAt = DateTime.UtcNow
        };

        var rav4 = new Vehicle
        {
            Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            Brand = "Toyota",
            Model = "RAV4",
            Year = 2023,
            LicensePlate = "XYZ-9087",
            Vin = "JTMRWRFV0PD123456",
            Color = "Blanco",
            CurrentMileage = 18750,
            Notes = "SUV para viajes. Seguro vigente hasta diciembre.",
            CreatedAt = DateTime.UtcNow
        };

        db.Users.AddRange(admin, manager);
        db.Vehicles.AddRange(civic, rav4);
        db.UserVehicles.Add(new UserVehicle
        {
            UserId = manager.Id,
            VehicleId = civic.Id,
            AssignedAt = DateTime.UtcNow
        });
        db.MaintenanceRecords.Add(new MaintenanceRecord
        {
            Id = Guid.NewGuid(),
            VehicleId = civic.Id,
            CreatedByUserId = manager.Id,
            Title = "Cambio de aceite",
            Description = "Aceite sintético 5W-30 y filtro original.",
            ServiceDate = DateTime.UtcNow.AddDays(-20),
            Mileage = 44800,
            Cost = 85.50m,
            CreatedAt = DateTime.UtcNow.AddDays(-20)
        });

        await db.SaveChangesAsync(cancellationToken);
        await SeedProceduresAsync(cancellationToken);
    }

    private async Task SeedProceduresAsync(CancellationToken cancellationToken)
    {
        var civicId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var rav4Id = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var managerId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var obsolete = await db.VehicleProcedures
            .Where(x => x.Title == "Marchamo" || x.Title == "Póliza de seguro")
            .ToListAsync(cancellationToken);
        if (obsolete.Count > 0)
        {
            db.VehicleProcedures.RemoveRange(obsolete);
        }

        await EnsureProcedureAsync(
            civicId,
            managerId,
            "Verificación semestral",
            "Verificación vehicular de emisiones, cada seis meses.",
            ProcedureFrequency.Semiannual,
            DateTime.UtcNow.Date.AddDays(-10),
            cancellationToken);

        await EnsureProcedureAsync(
            civicId,
            managerId,
            "Pago de tenencia y refrendo",
            "Pago anual de tenencia y refrendo de la tarjeta de circulación.",
            ProcedureFrequency.Annual,
            DateTime.UtcNow.Date.AddMonths(2),
            cancellationToken);

        await EnsureProcedureAsync(
            civicId,
            managerId,
            "Cambio de placas",
            "Trámite único de reemplazo o reposición de placas.",
            ProcedureFrequency.Once,
            DateTime.UtcNow.Date.AddDays(20),
            cancellationToken);

        await EnsureProcedureAsync(
            civicId,
            managerId,
            "Revisión técnica",
            "Inspección mecánica del vehículo.",
            ProcedureFrequency.Semiannual,
            DateTime.UtcNow.Date.AddMonths(1),
            cancellationToken);

        await EnsureProcedureAsync(
            rav4Id,
            managerId,
            "Renovación del seguro",
            "Renovación anual de la póliza de seguro del automóvil.",
            ProcedureFrequency.Annual,
            DateTime.UtcNow.Date.AddMonths(1),
            cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureProcedureAsync(
        Guid vehicleId,
        Guid createdByUserId,
        string title,
        string description,
        ProcedureFrequency frequency,
        DateTime dueDate,
        CancellationToken cancellationToken)
    {
        if (!await db.Vehicles.AnyAsync(x => x.Id == vehicleId, cancellationToken))
        {
            return;
        }

        var exists = await db.VehicleProcedures.AnyAsync(
            x => x.VehicleId == vehicleId && x.Title == title,
            cancellationToken);

        if (exists)
        {
            return;
        }

        db.VehicleProcedures.Add(new VehicleProcedure
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicleId,
            CreatedByUserId = createdByUserId,
            Title = title,
            Description = description,
            Frequency = frequency,
            DueDate = dueDate,
            CreatedAt = DateTime.UtcNow
        });
    }
}
