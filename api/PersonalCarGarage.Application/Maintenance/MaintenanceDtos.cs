namespace PersonalCarGarage.Application.Maintenance;

public record MaintenanceResponse(
    Guid Id,
    Guid VehicleId,
    string Title,
    string? Description,
    DateTime ServiceDate,
    int Mileage,
    decimal Cost,
    Guid CreatedByUserId,
    string CreatedByName,
    DateTime CreatedAt);

public record UpsertMaintenanceRequest(
    string Title,
    string? Description,
    DateTime ServiceDate,
    int Mileage,
    decimal Cost);
