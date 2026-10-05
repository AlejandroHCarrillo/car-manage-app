using PersonalCarGarage.Domain.Entities;

namespace PersonalCarGarage.Domain.Interfaces;

public interface IProcedureRepository
{
    Task<VehicleProcedure?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<VehicleProcedure>> GetByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<VehicleProcedure>> GetByVehicleIdsAsync(IReadOnlyCollection<Guid> vehicleIds, CancellationToken cancellationToken = default);
    Task AddAsync(VehicleProcedure procedure, CancellationToken cancellationToken = default);
    Task AddCompletionAsync(ProcedureCompletion completion, CancellationToken cancellationToken = default);
    void Update(VehicleProcedure procedure);
    void Remove(VehicleProcedure procedure);
}
