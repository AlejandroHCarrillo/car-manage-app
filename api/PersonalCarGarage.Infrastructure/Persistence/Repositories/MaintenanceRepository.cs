using Microsoft.EntityFrameworkCore;
using PersonalCarGarage.Domain.Entities;
using PersonalCarGarage.Domain.Interfaces;

namespace PersonalCarGarage.Infrastructure.Persistence.Repositories;

public class MaintenanceRepository(AppDbContext db) : IMaintenanceRepository
{
    public Task<MaintenanceRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.MaintenanceRecords
            .Include(x => x.CreatedByUser)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<MaintenanceRecord>> GetByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken = default) =>
        await db.MaintenanceRecords
            .Include(x => x.CreatedByUser)
            .Where(x => x.VehicleId == vehicleId)
            .OrderByDescending(x => x.ServiceDate)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(MaintenanceRecord record, CancellationToken cancellationToken = default) =>
        await db.MaintenanceRecords.AddAsync(record, cancellationToken);

    public void Update(MaintenanceRecord record) => db.MaintenanceRecords.Update(record);

    public void Remove(MaintenanceRecord record) => db.MaintenanceRecords.Remove(record);
}
