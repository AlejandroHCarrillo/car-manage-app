using PersonalCarGarage.Application.Abstractions;
using PersonalCarGarage.Domain.Entities;
using PersonalCarGarage.Domain.Enums;
using PersonalCarGarage.Domain.Exceptions;
using PersonalCarGarage.Domain.Interfaces;

namespace PersonalCarGarage.Application.Procedures;

public interface IProcedureService
{
    Task<IReadOnlyList<ProcedureResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProcedureResponse>> GetByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default);
    Task<ProcedureResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProcedureCompletionResponse>> GetHistoryAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProcedureResponse> CreateAsync(Guid vehicleId, UpsertProcedureRequest request, CancellationToken cancellationToken = default);
    Task<ProcedureResponse> UpdateAsync(Guid id, UpsertProcedureRequest request, CancellationToken cancellationToken = default);
    Task<ProcedureResponse> CompleteAsync(Guid id, CompleteProcedureRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public class ProcedureService(
    IProcedureRepository procedures,
    IVehicleRepository vehicles,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser) : IProcedureService
{
    public async Task<IReadOnlyList<ProcedureResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var vehicleIds = await GetAccessibleVehicleIdsAsync(cancellationToken);
        var items = await procedures.GetByVehicleIdsAsync(vehicleIds, cancellationToken);
        return items.Select(Map).OrderBy(x => x.IsCompleted).ThenBy(x => x.DueDate).ToList();
    }

    public async Task<IReadOnlyList<ProcedureResponse>> GetByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        await EnsureVehicleAccessAsync(vehicleId, cancellationToken);
        var items = await procedures.GetByVehicleIdAsync(vehicleId, cancellationToken);
        return items.Select(Map).ToList();
    }

    public async Task<ProcedureResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var procedure = await GetAccessibleProcedureAsync(id, cancellationToken);
        return Map(procedure);
    }

    public async Task<IReadOnlyList<ProcedureCompletionResponse>> GetHistoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var procedure = await GetAccessibleProcedureAsync(id, cancellationToken);
        return procedure.Completions
            .OrderByDescending(x => x.CompletedAt)
            .Select(x => new ProcedureCompletionResponse(
                x.Id,
                x.ProcedureId,
                x.CompletedAt,
                x.DueDateCovered,
                x.Cost,
                x.Notes,
                x.CreatedByUser?.FullName ?? string.Empty,
                x.CreatedAt))
            .ToList();
    }

    public async Task<ProcedureResponse> CreateAsync(Guid vehicleId, UpsertProcedureRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);
        var vehicle = await EnsureVehicleAccessAsync(vehicleId, cancellationToken);

        var procedure = new VehicleProcedure
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicle.Id,
            CreatedByUserId = currentUser.UserId,
            Title = request.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            Frequency = request.Frequency,
            DueDate = request.DueDate.Date,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        };

        await procedures.AddAsync(procedure, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await procedures.GetByIdAsync(procedure.Id, cancellationToken) ?? procedure;
        return Map(created);
    }

    public async Task<ProcedureResponse> UpdateAsync(Guid id, UpsertProcedureRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);
        var procedure = await GetAccessibleProcedureAsync(id, cancellationToken);

        procedure.Title = request.Title.Trim();
        procedure.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        procedure.Frequency = request.Frequency;
        procedure.DueDate = request.DueDate.Date;
        procedure.UpdatedAt = DateTime.UtcNow;

        if (procedure.Frequency != ProcedureFrequency.Once)
        {
            procedure.IsCompleted = false;
        }

        procedures.Update(procedure);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(procedure);
    }

    public async Task<ProcedureResponse> CompleteAsync(Guid id, CompleteProcedureRequest request, CancellationToken cancellationToken = default)
    {
        var procedure = await GetAccessibleProcedureAsync(id, cancellationToken);

        if (procedure.IsCompleted && procedure.Frequency == ProcedureFrequency.Once)
        {
            throw new AppException("Este trámite de una sola vez ya fue completado.");
        }

        if (request.Cost < 0)
        {
            throw new AppException("El costo no puede ser negativo.");
        }

        var coveredDue = procedure.DueDate.Date;
        var completedAt = request.CompletedAt == default ? DateTime.UtcNow : request.CompletedAt;

        await procedures.AddCompletionAsync(new ProcedureCompletion
        {
            Id = Guid.NewGuid(),
            ProcedureId = procedure.Id,
            CreatedByUserId = currentUser.UserId,
            CompletedAt = completedAt,
            DueDateCovered = coveredDue,
            Cost = request.Cost,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        procedure.LastCost = request.Cost;
        procedure.UpdatedAt = DateTime.UtcNow;

        if (procedure.Frequency == ProcedureFrequency.Once)
        {
            procedure.IsCompleted = true;
        }
        else
        {
            var nextDue = procedure.NextDueDate(coveredDue);
            while (nextDue.Date <= completedAt.Date)
            {
                nextDue = procedure.NextDueDate(nextDue);
            }

            procedure.DueDate = nextDue.Date;
            procedure.IsCompleted = false;
        }

        procedures.Update(procedure);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await procedures.GetByIdAsync(procedure.Id, cancellationToken) ?? procedure;
        return Map(updated);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var procedure = await GetAccessibleProcedureAsync(id, cancellationToken);
        procedures.Remove(procedure);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<Guid>> GetAccessibleVehicleIdsAsync(CancellationToken cancellationToken)
    {
        var items = currentUser.IsAdmin
            ? await vehicles.GetAllAsync(cancellationToken)
            : await vehicles.GetByUserIdAsync(currentUser.UserId, cancellationToken);

        return items.Select(x => x.Id).ToList();
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

    private async Task<VehicleProcedure> GetAccessibleProcedureAsync(Guid id, CancellationToken cancellationToken)
    {
        var procedure = await procedures.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Trámite no encontrado.");

        await EnsureVehicleAccessAsync(procedure.VehicleId, cancellationToken);
        return procedure;
    }

    private static void Validate(UpsertProcedureRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new AppException("El título del trámite es obligatorio.");
        }

        if (!Enum.IsDefined(request.Frequency))
        {
            throw new AppException("La periodicidad no es válida.");
        }
    }

    private static ProcedureResponse Map(VehicleProcedure procedure)
    {
        var today = DateTime.UtcNow.Date;
        var status = procedure.IsCompleted
            ? "Completed"
            : procedure.DueDate.Date < today
                ? "Overdue"
                : procedure.DueDate.Date <= today.AddDays(30)
                    ? "Upcoming"
                    : "Pending";

        return new ProcedureResponse(
            procedure.Id,
            procedure.VehicleId,
            procedure.Vehicle is null ? string.Empty : procedure.Vehicle.DisplayName,
            procedure.Vehicle?.LicensePlate ?? string.Empty,
            procedure.Title,
            procedure.Description,
            procedure.Frequency,
            procedure.DueDate,
            procedure.LastCost,
            procedure.IsCompleted,
            status,
            procedure.Completions.Count,
            procedure.CreatedAt);
    }
}
