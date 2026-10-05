using PersonalCarGarage.Application.Abstractions;
using PersonalCarGarage.Domain.Entities;
using PersonalCarGarage.Domain.Exceptions;
using PersonalCarGarage.Domain.Interfaces;

namespace PersonalCarGarage.Application.Maintenance;

public interface IMaintenanceService
{
    Task<IReadOnlyList<MaintenanceResponse>> GetByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default);
    Task<MaintenanceResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<MaintenanceResponse> CreateAsync(Guid vehicleId, UpsertMaintenanceRequest request, CancellationToken cancellationToken = default);
    Task<MaintenanceResponse> UpdateAsync(Guid id, UpsertMaintenanceRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public class MaintenanceService(
    IMaintenanceRepository maintenance,
    IVehicleRepository vehicles,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser) : IMaintenanceService
{
    public async Task<IReadOnlyList<MaintenanceResponse>> GetByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        await EnsureVehicleAccessAsync(vehicleId, cancellationToken);
        var items = await maintenance.GetByVehicleIdAsync(vehicleId, cancellationToken);
        return items.Select(Map).ToList();
    }

    public async Task<MaintenanceResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var record = await maintenance.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Registro de mantenimiento no encontrado.");

        await EnsureVehicleAccessAsync(record.VehicleId, cancellationToken);
        return Map(record);
    }

    public async Task<MaintenanceResponse> CreateAsync(Guid vehicleId, UpsertMaintenanceRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);
        var vehicle = await EnsureVehicleAccessAsync(vehicleId, cancellationToken);

        var record = new MaintenanceRecord
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicle.Id,
            CreatedByUserId = currentUser.UserId,
            Title = request.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            ServiceDate = request.ServiceDate,
            Mileage = request.Mileage,
            Cost = request.Cost,
            CreatedAt = DateTime.UtcNow
        };

        if (request.Mileage > vehicle.CurrentMileage)
        {
            vehicle.CurrentMileage = request.Mileage;
            vehicle.UpdatedAt = DateTime.UtcNow;
            vehicles.Update(vehicle);
        }

        await maintenance.AddAsync(record, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await maintenance.GetByIdAsync(record.Id, cancellationToken) ?? record;
        return Map(created);
    }

    public async Task<MaintenanceResponse> UpdateAsync(Guid id, UpsertMaintenanceRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);
        var record = await maintenance.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Registro de mantenimiento no encontrado.");

        await EnsureVehicleAccessAsync(record.VehicleId, cancellationToken);

        record.Title = request.Title.Trim();
        record.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        record.ServiceDate = request.ServiceDate;
        record.Mileage = request.Mileage;
        record.Cost = request.Cost;

        maintenance.Update(record);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(record);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var record = await maintenance.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Registro de mantenimiento no encontrado.");

        await EnsureVehicleAccessAsync(record.VehicleId, cancellationToken);

        maintenance.Remove(record);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Vehicle> EnsureVehicleAccessAsync(Guid vehicleId, CancellationToken cancellationToken)
    {
        var vehicle = await vehicles.GetByIdAsync(vehicleId, cancellationToken)
            ?? throw new NotFoundException("Vehículo no encontrado.");

        if (!currentUser.IsAdmin &&
            !await vehicles.IsAssignedToUserAsync(vehicleId, currentUser.UserId, cancellationToken))
        {
            throw new ForbiddenException("No tienes acceso a este vehículo.");
        }

        return vehicle;
    }

    private static void Validate(UpsertMaintenanceRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new AppException("El título es obligatorio.");
        }

        if (request.Mileage < 0)
        {
            throw new AppException("El kilometraje no puede ser negativo.");
        }

        if (request.Cost < 0)
        {
            throw new AppException("El costo no puede ser negativo.");
        }
    }

    private static MaintenanceResponse Map(MaintenanceRecord record) =>
        new(
            record.Id,
            record.VehicleId,
            record.Title,
            record.Description,
            record.ServiceDate,
            record.Mileage,
            record.Cost,
            record.CreatedByUserId,
            record.CreatedByUser?.FullName ?? string.Empty,
            record.CreatedAt);
}
