using PersonalCarGarage.Domain.Entities;

namespace PersonalCarGarage.Domain.Interfaces;

public interface IVehicleRepository
{
    Task<Vehicle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Vehicle>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Vehicle>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> LicensePlateExistsAsync(string licensePlate, Guid? excludeVehicleId = null, CancellationToken cancellationToken = default);
    Task<bool> IsAssignedToUserAsync(Guid vehicleId, Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken = default);
    void Update(Vehicle vehicle);
    void Remove(Vehicle vehicle);
    void AddPhoto(VehiclePhoto photo);
    void AddDocument(VehicleDocument document);
    Task AssignUserAsync(Guid vehicleId, Guid userId, CancellationToken cancellationToken = default);
    Task UnassignUserAsync(Guid vehicleId, Guid userId, CancellationToken cancellationToken = default);
}
