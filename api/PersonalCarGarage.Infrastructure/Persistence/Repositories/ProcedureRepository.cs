using Microsoft.EntityFrameworkCore;
using PersonalCarGarage.Domain.Entities;
using PersonalCarGarage.Domain.Interfaces;

namespace PersonalCarGarage.Infrastructure.Persistence.Repositories;

public class ProcedureRepository(AppDbContext db) : IProcedureRepository
{
    public Task<VehicleProcedure?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.VehicleProcedures
            .Include(x => x.Vehicle)
            .Include(x => x.Completions)
            .ThenInclude(x => x.CreatedByUser)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<VehicleProcedure>> GetByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken = default) =>
        await db.VehicleProcedures
            .Include(x => x.Vehicle)
            .Include(x => x.Completions)
            .Where(x => x.VehicleId == vehicleId)
            .OrderBy(x => x.IsCompleted)
            .ThenBy(x => x.DueDate)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<VehicleProcedure>> GetByVehicleIdsAsync(IReadOnlyCollection<Guid> vehicleIds, CancellationToken cancellationToken = default) =>
        await db.VehicleProcedures
            .Include(x => x.Vehicle)
            .Include(x => x.Completions)
            .Where(x => vehicleIds.Contains(x.VehicleId))
            .OrderBy(x => x.IsCompleted)
            .ThenBy(x => x.DueDate)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(VehicleProcedure procedure, CancellationToken cancellationToken = default) =>
        await db.VehicleProcedures.AddAsync(procedure, cancellationToken);

    public async Task AddCompletionAsync(ProcedureCompletion completion, CancellationToken cancellationToken = default) =>
        await db.ProcedureCompletions.AddAsync(completion, cancellationToken);

    public void Update(VehicleProcedure procedure) => db.VehicleProcedures.Update(procedure);

    public void Remove(VehicleProcedure procedure) => db.VehicleProcedures.Remove(procedure);
}
