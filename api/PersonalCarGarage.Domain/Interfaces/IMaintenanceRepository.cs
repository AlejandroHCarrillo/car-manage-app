using PersonalCarGarage.Domain.Entities;

namespace PersonalCarGarage.Domain.Interfaces;

public interface IMaintenanceRepository
{
    Task<MaintenanceRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MaintenanceRecord>> GetByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken = default);
    Task AddAsync(MaintenanceRecord record, CancellationToken cancellationToken = default);
    void Update(MaintenanceRecord record);
    void Remove(MaintenanceRecord record);
}
