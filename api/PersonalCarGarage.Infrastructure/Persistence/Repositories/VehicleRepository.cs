using Microsoft.EntityFrameworkCore;
using PersonalCarGarage.Domain.Entities;
using PersonalCarGarage.Domain.Interfaces;

namespace PersonalCarGarage.Infrastructure.Persistence.Repositories;

public class VehicleRepository(AppDbContext db) : IVehicleRepository
{
    public Task<Vehicle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Vehicles
            .Include(x => x.UserVehicles)
            .ThenInclude(x => x.User)
            .Include(x => x.Photos)
            .Include(x => x.Documents)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Vehicle>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await db.Vehicles
            .Include(x => x.UserVehicles)
            .ThenInclude(x => x.User)
            .Include(x => x.Photos)
            .Include(x => x.Documents)
            .OrderBy(x => x.Brand)
            .ThenBy(x => x.Model)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Vehicle>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await db.Vehicles
            .Include(x => x.UserVehicles)
            .ThenInclude(x => x.User)
            .Include(x => x.Photos)
            .Include(x => x.Documents)
            .Where(x => x.UserVehicles.Any(uv => uv.UserId == userId))
            .OrderBy(x => x.Brand)
            .ThenBy(x => x.Model)
            .ToListAsync(cancellationToken);

    public Task<bool> LicensePlateExistsAsync(string licensePlate, Guid? excludeVehicleId = null, CancellationToken cancellationToken = default)
    {
        var normalized = licensePlate.Trim().ToUpper();
        return db.Vehicles.AnyAsync(
            x => x.LicensePlate == normalized && (!excludeVehicleId.HasValue || x.Id != excludeVehicleId.Value),
            cancellationToken);
    }

    public Task<bool> IsAssignedToUserAsync(Guid vehicleId, Guid userId, CancellationToken cancellationToken = default) =>
        db.UserVehicles.AnyAsync(x => x.VehicleId == vehicleId && x.UserId == userId, cancellationToken);

    public async Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken = default) =>
        await db.Vehicles.AddAsync(vehicle, cancellationToken);

    public void Update(Vehicle vehicle) => db.Vehicles.Update(vehicle);

    public void Remove(Vehicle vehicle) => db.Vehicles.Remove(vehicle);

    public void AddPhoto(VehiclePhoto photo)
    {
        db.ChangeTracker.Clear();
        db.VehiclePhotos.Add(photo);
    }

    public void AddDocument(VehicleDocument document)
    {
        db.ChangeTracker.Clear();
        db.VehicleDocuments.Add(document);
    }

    public async Task AssignUserAsync(Guid vehicleId, Guid userId, CancellationToken cancellationToken = default)
    {
        await db.UserVehicles.AddAsync(new UserVehicle
        {
            VehicleId = vehicleId,
            UserId = userId,
            AssignedAt = DateTime.UtcNow
        }, cancellationToken);
    }

    public async Task UnassignUserAsync(Guid vehicleId, Guid userId, CancellationToken cancellationToken = default)
    {
        var assignment = await db.UserVehicles
            .FirstOrDefaultAsync(x => x.VehicleId == vehicleId && x.UserId == userId, cancellationToken);

        if (assignment is not null)
        {
            db.UserVehicles.Remove(assignment);
        }
    }
}
